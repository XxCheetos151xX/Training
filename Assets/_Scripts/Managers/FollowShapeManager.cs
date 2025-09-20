using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class FollowShapeManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Material system_line_mat;
    [SerializeField] private Material user_line_mat;
    [SerializeField] private BackgroundGenerator backgroundGenerator;
    [SerializeField] private LineRenderer userLine; // assign in Inspector
    [SerializeField] private List<LineRenderer> shapes;

    [Header("Game Settings")]
    [SerializeField] private float delay_between_shapes = 1f;
    [SerializeField] private Color visible_color;
    [SerializeField] private Color drawing_color;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference touchPositionAction; // Vector2
    [SerializeField] private InputActionReference touchPressAction;    // Button

    [SerializeField] private UnityEvent GameEnded;

    private Camera mainCam;
    private LineRenderer active_shape;
    private int streak = 0;
    private int current_shape_index = 0;

    // sequential-following state
    private int currentSegment = 0;
    private bool goingForward = true;

    private bool is_touching = false;
    private bool is_drawing = false;
    private List<Vector3> userPoints = new List<Vector3>();
    private List<Vector3> lastSystemShapePoints = new List<Vector3>();

    private const float SEGMENT_REACH_TOLERANCE = 0.05f;

    private void Awake()
    {
        touchPositionAction.action.Enable();
        touchPressAction.action.Enable();
    }

    private void OnDestroy()
    {
        touchPositionAction.action.Disable();
        touchPressAction.action.Disable();
    }

    private void Start()
    {
        mainCam = Camera.main;

        system_line_mat.color = visible_color;
        user_line_mat.color = drawing_color;

        // configure user line renderer
        if (userLine != null)
        {
            userLine.material = user_line_mat;
            userLine.startWidth = 0.05f;
            userLine.endWidth = 0.05f;
            userLine.enabled = false;
        }

        backgroundGenerator.GenerateConstantBackGround(0.5f);
        StartCoroutine(HandleTouch());
        GenerateShape();
    }

    // This handles touch input events and movement logic.
    void OnTouchperformed()
    {
        if (!is_touching || player == null)
            return;

        Vector2 touchScreenPos = touchPositionAction.action.ReadValue<Vector2>();
        Vector3 touchWorldPos = mainCam.ScreenToWorldPoint(
            new Vector3(touchScreenPos.x, touchScreenPos.y, -mainCam.transform.position.z));

        // If a system shape exists, constrain the player to move along it, segment-by-segment.
        if (active_shape != null)
        {
            // Ensure segment indices valid
            if (currentSegment < 0) currentSegment = 0;
            if (active_shape.positionCount < 2) return;

            if (goingForward)
            {
                // Stay on current segment [currentSegment, currentSegment+1]
                if (currentSegment < active_shape.positionCount - 1)
                {
                    Vector3 a = active_shape.GetPosition(currentSegment);
                    Vector3 b = active_shape.GetPosition(currentSegment + 1);

                    Vector3 projected = ProjectPointOnSegment(a, b, (Vector2)touchWorldPos);
                    player.transform.position = projected;

                    // If reached end of segment, advance
                    if (Vector3.Distance(player.transform.position, b) < SEGMENT_REACH_TOLERANCE)
                    {
                        currentSegment++;
                    }
                }

                // If reached final point, switch direction to backward
                if (currentSegment >= active_shape.positionCount - 1 &&
                    Vector3.Distance(player.transform.position, active_shape.GetPosition(active_shape.positionCount - 1)) < SEGMENT_REACH_TOLERANCE)
                {
                    goingForward = false;
                    currentSegment = active_shape.positionCount - 1;
                }
            }
            else // going backward
            {
                // Move along [currentSegment, currentSegment-1]
                if (currentSegment > 0)
                {
                    Vector3 a = active_shape.GetPosition(currentSegment);
                    Vector3 b = active_shape.GetPosition(currentSegment - 1);

                    Vector3 projected = ProjectPointOnSegment(a, b, (Vector2)touchWorldPos);
                    player.transform.position = projected;

                    if (Vector3.Distance(player.transform.position, b) < SEGMENT_REACH_TOLERANCE)
                    {
                        currentSegment--;
                    }
                }

                // If returned to start => full forward+back cycle complete
                if (currentSegment <= 0 &&
                    Vector3.Distance(player.transform.position, active_shape.GetPosition(0)) < SEGMENT_REACH_TOLERANCE)
                {
                    streak++;
                    Debug.Log("Streak: " + streak);
                    HandleStreak(streak);
                    // After HandleStreak, active_shape may have been destroyed and is_drawing set.
                    goingForward = true;
                    currentSegment = 0;
                }
            }
        }
        else
        {
            // No system shape exists -> drawing phase: free movement
            player.transform.position = touchWorldPos;
        }
    }

    private Vector3 ProjectPointOnSegment(Vector3 a, Vector3 b, Vector2 p)
    {
        Vector2 ap = p - (Vector2)a;
        Vector2 ab = (b - a);
        float ab2 = Vector2.Dot(ab, ab);
        float t = (ab2 > 0f) ? Mathf.Clamp01(Vector2.Dot(ap, ab) / ab2) : 0f;
        return a + t * (Vector3)ab;
    }

    // Called by ClickableObject when touch begins on the ball
    public void TouchedTheBall()
    {
        is_touching = true;
    }

    // You kept GameEnd / shape lifecycle - unchanged (besides using DestroyShape below)
    public void GameEnd()
    {
        Destroy(player);
        DestroyShape();
        StopAllCoroutines();
    }

    void HandleStreak(int streakCount)
    {
        if (streakCount >= 2)
        {
            // move to next shape (no drawing phase)
            current_shape_index++;
            if (current_shape_index >= shapes.Count)
            {
                GameEnded.Invoke();
                return;
            }
            this.streak = 0;
            StartCoroutine(ShapeHandler());
        }
        else if (streakCount >= 1)
        {
            // destroy system shape and switch to drawing phase
            if (active_shape != null)
            {
                lastSystemShapePoints.Clear();
                for (int i = 0; i < active_shape.positionCount; i++)
                    lastSystemShapePoints.Add(active_shape.GetPosition(i));

                DestroyShape();
            }

            is_drawing = true;
            // UserDrawingRoutine is started by HandleTouch when it detects touch on the ball while is_drawing==true
        }
    }

    void GenerateShape()
    {
        active_shape = Instantiate(shapes[current_shape_index], Vector2.zero, shapes[current_shape_index].transform.rotation);

        // Ensure it renders
        active_shape.material = system_line_mat;
        active_shape.startWidth = 0.05f;
        active_shape.endWidth = 0.05f;
        active_shape.enabled = true;

        if (player != null)
        {
            player.transform.position = active_shape.GetPosition(0);
        }

        // reset sequential-follow state
        currentSegment = 0;
        goingForward = true;
    }

    void DestroyShape()
    {
        if (active_shape != null)
            Destroy(active_shape.gameObject);
        active_shape = null;
    }

    IEnumerator HandleTouch()
    {
        CircleCollider2D col = player.GetComponent<CircleCollider2D>();

        while (true)
        {
            bool pressed = touchPressAction.action.IsPressed();
            if (pressed)
            {
                Vector2 screenPos = touchPositionAction.action.ReadValue<Vector2>();
                Vector3 worldPos = mainCam.ScreenToWorldPoint(
                    new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));

                if (col != null && col.OverlapPoint(worldPos))
                {
                    is_touching = true;
                    OnTouchperformed();

                    // If we entered drawing phase and haven't started collecting points yet -> start routine
                    if (is_drawing && userPoints.Count == 0)
                        StartCoroutine(UserDrawingRoutine());
                }
                else
                {
                    is_touching = false;
                }
            }
            else
            {
                is_touching = false;
            }

            yield return null;
        }
    }

    IEnumerator ShapeHandler()
    {
        // destroy before waiting so there's no duplicate shapes staying around
        DestroyShape();
        yield return new WaitForSeconds(delay_between_shapes);
        GenerateShape();
    }

    IEnumerator UserDrawingRoutine()
    {
        userPoints.Clear();
        if (userLine != null)
        {
            userLine.positionCount = 0;
            userLine.enabled = false; // hidden while drawing
        }

        // collect until release
        while (touchPressAction.action.IsPressed())
        {
            Vector2 screenPos = touchPositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));

            if (userPoints.Count == 0 || Vector3.Distance(userPoints[userPoints.Count - 1], worldPos) > 0.05f)
            {
                userPoints.Add(worldPos);
            }

            yield return null;
        }

        // show user drawing
        if (userLine != null)
        {
            userLine.enabled = true;
            userLine.positionCount = userPoints.Count;
            userLine.SetPositions(userPoints.ToArray());
        }

        // compare with system shape (normalize so absolute screen position doesn't matter)
        if (lastSystemShapePoints.Count > 1)
        {
            bool similar = CompareShapes(lastSystemShapePoints, userPoints);
            Debug.Log(similar ? "Similar" : "Not Similar");
        }

        
        yield return new WaitForSeconds(delay_between_shapes);

        if (userLine != null)
        {
            userLine.enabled = false;
            userLine.positionCount = 0;
        }

        is_drawing = false;
        streak = 0;
        current_shape_index++;
        if (current_shape_index < shapes.Count)
            GenerateShape();
        else
            GameEnded.Invoke();
    }

    bool CompareShapes(List<Vector3> systemShapePoints, List<Vector3> userShapePoints)
    {
        int sampleCount = 32;
        List<Vector3> sysResampled = ResampleShape(systemShapePoints, sampleCount);
        List<Vector3> userResampled = ResampleShape(userShapePoints, sampleCount);

        NormalizePoints(sysResampled);
        NormalizePoints(userResampled);

        float totalDist = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            totalDist += Vector3.Distance(sysResampled[i], userResampled[i]);
        }

        float avgDist = totalDist / sampleCount;
        return avgDist < 0.5f; 
    }

    List<Vector3> ResampleShape(List<Vector3> input, int count)
    {
        List<Vector3> result = new List<Vector3>();
        if (input.Count < 2) return result;

        float totalLength = 0f;
        for (int i = 0; i < input.Count - 1; i++)
            totalLength += Vector3.Distance(input[i], input[i + 1]);

        float step = totalLength / (count - 1);
        float distSoFar = 0f;

        result.Add(input[0]);
        int currentIndex = 0;

        for (int i = 1; i < count; i++)
        {
            float targetDist = i * step;

            while (currentIndex < input.Count - 2 &&
                   distSoFar + Vector3.Distance(input[currentIndex], input[currentIndex + 1]) < targetDist)
            {
                distSoFar += Vector3.Distance(input[currentIndex], input[currentIndex + 1]);
                currentIndex++;
            }

            float segmentLen = Vector3.Distance(input[currentIndex], input[currentIndex + 1]);
            float t = (segmentLen > 0f) ? (targetDist - distSoFar) / segmentLen : 0f;
            Vector3 newPoint = Vector3.Lerp(input[currentIndex], input[currentIndex + 1], t);
            result.Add(newPoint);
        }

        return result;
    }

    void NormalizePoints(List<Vector3> pts)
    {
        if (pts.Count == 0) return;

        Vector3 centroid = Vector3.zero;
        foreach (var p in pts) centroid += p;
        centroid /= pts.Count;

        for (int i = 0; i < pts.Count; i++) pts[i] -= centroid;

        float maxDist = 0f;
        foreach (var p in pts) maxDist = Mathf.Max(maxDist, p.magnitude);

        if (maxDist > 0f)
        {
            for (int i = 0; i < pts.Count; i++) pts[i] /= maxDist;
        }
    }
}
