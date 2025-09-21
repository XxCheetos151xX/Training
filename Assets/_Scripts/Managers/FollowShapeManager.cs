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
    [SerializeField] private Color invisible_color;
    [SerializeField] private Color drawing_color;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference touchPositionAction; // Vector2
    [SerializeField] private InputActionReference touchPressAction;    // Button

    [SerializeField] private UnityEvent GameEnded;

    private Camera mainCam;
    private LineRenderer active_shape;
    private int streak = 0;
    private int currentSegment = 0;
    private int current_shape_index = 0;
    private bool is_touching = false;
    private bool is_invisible = false;
    private bool goingforward = true;
    private List<Vector3> userPoints = new List<Vector3>();

    // internal state flags
    private bool pendingCycle = false;         // set when forward+back completes while finger still down
    private bool drawingAllowed = false;       // true when system is in drawing-ready (shape invisible) state
    private bool userDrawingRunning = false;   // prevent multiple drawing coroutines

    private void Awake()
    {
        touchPositionAction.action.Enable();
        touchPressAction.action.Enable();
        ShuffleShapes();
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

        // configure userLine if present
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

    void ShuffleShapes()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            int randomIndex = Random.Range(i, shapes.Count);
            (shapes[i], shapes[randomIndex]) = (shapes[randomIndex], shapes[i]);
        }
    }

    void OnTouchperformed()
    {
        if (!is_touching || active_shape == null || player == null) return;
        if (is_invisible) return;

        Vector2 touchScreenPos = touchPositionAction.action.ReadValue<Vector2>();
        Vector3 touchWorldPos = mainCam.ScreenToWorldPoint(new Vector3(touchScreenPos.x, touchScreenPos.y, -mainCam.transform.position.z));

        if (goingforward)
        {
            if (currentSegment < active_shape.positionCount - 1)
            {
                Vector3 a = active_shape.GetPosition(currentSegment);
                Vector3 b = active_shape.GetPosition(currentSegment + 1);

                Vector3 projected = ProjectPointOnSegment(a, b, touchWorldPos);
                player.transform.position = projected;

                if (Vector3.Distance(player.transform.position, b) < 0.05f)
                {
                    currentSegment++;
                }
            }

            if (currentSegment >= active_shape.positionCount - 1 &&
                Vector3.Distance(player.transform.position, active_shape.GetPosition(active_shape.positionCount - 1)) < 0.05f)
            {
                goingforward = false;
                currentSegment = active_shape.positionCount - 1;
            }
        }
        else
        {
            if (currentSegment > 0)
            {
                Vector3 a = active_shape.GetPosition(currentSegment);
                Vector3 b = active_shape.GetPosition(currentSegment - 1);

                Vector3 projected = ProjectPointOnSegment(a, b, touchWorldPos);
                player.transform.position = projected;

                if (Vector3.Distance(player.transform.position, b) < 0.05f)
                {
                    currentSegment--;
                }
            }

            if (currentSegment <= 0 &&
                Vector3.Distance(player.transform.position, active_shape.GetPosition(0)) < 0.05f)
            {
                pendingCycle = true;
                Debug.Log("Full cycle completed (pending streak, will commit on release).");
                goingforward = true;
                currentSegment = 0;
            }
        }
    }

    private Vector3 ProjectPointOnSegment(Vector3 a, Vector3 b, Vector2 p)
    {
        Vector2 ap = p - (Vector2)a;
        Vector2 ab = (b - a);
        float ab2 = Vector2.Dot(ab, ab);
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab2);
        return a + t * (Vector3)ab;
    }

    public void TouchedTheBall()
    {
        is_touching = true;
    }

    public void GameEnd()
    {
        Destroy(player);
        DestroyShape();
        StopAllCoroutines();
    }

    // ✅ EDITED
    void HandleStreak(int streakCount)
    {
        if (streakCount >= 2)
        {
            is_invisible = false;
            drawingAllowed = false;
            userDrawingRunning = false;

            system_line_mat.color = visible_color;
            streak = 0;

            StartCoroutine(AdvanceToNextShapeAfterDelay());
        }
        else if (streakCount >= 1)
        {
            is_invisible = true;
            drawingAllowed = true;
            system_line_mat.color = invisible_color;

            Debug.Log("Entered drawing-ready mode. User can start drawing now.");
        }
    }

    void GenerateShape()
    {
        if (current_shape_index < 0 || current_shape_index >= shapes.Count)
        {
            GameEnded.Invoke();
            return;
        }

        active_shape = Instantiate(shapes[current_shape_index], Vector2.zero, shapes[current_shape_index].transform.rotation);

        active_shape.material = system_line_mat;
        active_shape.startWidth = 0.05f;
        active_shape.endWidth = 0.05f;
        active_shape.enabled = true;

        if (player != null)
        {
            player.transform.position = active_shape.GetPosition(0);
            player.SetActive(true);
        }

        currentSegment = 0;
        goingforward = true;

        is_invisible = false;
        drawingAllowed = false;
        userDrawingRunning = false;
    }

    void DestroyShape()
    {
        if (active_shape != null)
            Destroy(active_shape.gameObject);
        active_shape = null;
    }

    IEnumerator HandleTouch()
    {
        bool prevPressed = false;
        CircleCollider2D col = player != null ? player.GetComponent<CircleCollider2D>() : null;

        while (true)
        {
            bool pressed = touchPressAction.action.IsPressed();

            Vector2 screenPos = touchPositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));

            bool justPressed = pressed && !prevPressed;
            bool justReleased = !pressed && prevPressed;

            if (drawingAllowed && justPressed && !userDrawingRunning)
            {
                userDrawingRunning = true;
                StartCoroutine(UserDrawingRoutine());
            }

            if (!is_invisible && col != null && col.OverlapPoint(worldPos) && pressed)
            {
                is_touching = true;
                OnTouchperformed();
            }
            else
            {
                is_touching = false;
            }

            if (justReleased)
            {
                if (pendingCycle)
                {
                    pendingCycle = false;
                    streak++;
                    Debug.Log("Committed streak on release. streak=" + streak);
                    HandleStreak(streak);
                }
            }

            prevPressed = pressed;
            yield return null;
        }
    }

    IEnumerator AdvanceToNextShapeAfterDelay()
    {
        yield return new WaitForSeconds(delay_between_shapes);

        DestroyShape();

        current_shape_index++;
        if (current_shape_index < shapes.Count)
        {
            GenerateShape();
        }
        else
        {
            GameEnded.Invoke();
        }
    }

    // ✅ EDITED
    IEnumerator UserDrawingRoutine()
    {
        userPoints.Clear();
        if (userLine != null)
        {
            userLine.positionCount = 0;
            userLine.enabled = false;
        }


        while (touchPressAction.action.IsPressed())
        {
            Vector2 screenPos = touchPositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));

            player.transform.position = worldPos;

            if (userPoints.Count == 0 || Vector3.Distance(userPoints[userPoints.Count - 1], worldPos) > 0.03f)
            {
                userPoints.Add(worldPos);
            }

            yield return null;
        }

        if (userPoints.Count > 1 && userLine != null)
        {
            userLine.positionCount = userPoints.Count;
            userLine.SetPositions(userPoints.ToArray());
            userLine.enabled = true;
        }

        system_line_mat.color = visible_color;
        is_invisible = false;
        drawingAllowed = false;

        if (player != null)
            player.SetActive(false);

        Debug.Log("User finished drawing with " + userPoints.Count + " points.");

        bool matched = false;
        if (active_shape != null && userPoints.Count > 1)
        {
            matched = CompareShapes(active_shape, userPoints);
            Debug.Log(matched ? "✅ Shape matched!" : "❌ Shape did not match.");
        }

        yield return new WaitForSeconds(delay_between_shapes);

        if (userLine != null)
        {
            userLine.enabled = false;
            userLine.positionCount = 0;
        }

        DestroyShape();

        streak = 0;
        current_shape_index++;
        userDrawingRunning = false;

        if (current_shape_index < shapes.Count)
            GenerateShape();
        else
            GameEnded.Invoke();
    }

    private bool CompareShapes(LineRenderer systemShape, List<Vector3> userPts, int sampleCount = 64, float threshold = 0.12f)
    {
        if (systemShape == null || userPts == null || userPts.Count < 2) return false;

        List<Vector3> systemPoints = new List<Vector3>();
        for (int i = 0; i < systemShape.positionCount; i++)
            systemPoints.Add(systemShape.GetPosition(i));

        List<Vector2> sys = Resample(systemPoints, sampleCount);
        List<Vector2> usr = Resample(userPts, sampleCount);

        if (sys.Count == 0 || usr.Count == 0) return false;

        sys = Normalize(sys);
        usr = Normalize(usr);

        float sum = 0f;
        for (int i = 0; i < sampleCount; i++)
            sum += Vector2.Distance(sys[i], usr[i]);

        float avg = sum / sampleCount;
        Debug.Log($"CompareShapes avgDist={avg}");
        return avg < threshold;
    }

    private List<Vector2> Resample(List<Vector3> pts3, int sampleCount)
    {
        List<Vector2> path = new List<Vector2>(pts3.Count);
        foreach (var p in pts3) path.Add(new Vector2(p.x, p.y));
        return Resample(path, sampleCount);
    }

    private List<Vector2> Resample(List<Vector2> path, int sampleCount)
    {
        List<Vector2> outPts = new List<Vector2>();
        if (path.Count == 0) return outPts;
        if (path.Count == 1)
        {
            for (int i = 0; i < sampleCount; i++) outPts.Add(path[0]);
            return outPts;
        }

        float total = PathLength(path);
        if (total <= Mathf.Epsilon)
        {
            for (int i = 0; i < sampleCount; i++) outPts.Add(path[0]);
            return outPts;
        }

        float interval = total / (sampleCount - 1);
        outPts.Add(path[0]);
        int idx = 1;
        float acc = 0f;

        while (outPts.Count < sampleCount && idx < path.Count)
        {
            Vector2 a = path[idx - 1];
            Vector2 b = path[idx];
            float seg = Vector2.Distance(a, b);

            if (seg <= Mathf.Epsilon)
            {
                idx++;
                continue;
            }

            if (acc + seg >= interval)
            {
                float t = (interval - acc) / seg;
                Vector2 np = Vector2.Lerp(a, b, t);
                outPts.Add(np);
                path[idx - 1] = np;
                acc = 0f;
            }
            else
            {
                acc += seg;
                idx++;
            }
        }

        if (outPts.Count < sampleCount)
            outPts.Add(path[path.Count - 1]);

        return outPts;
    }

    private float PathLength(List<Vector2> pts)
    {
        float len = 0f;
        for (int i = 1; i < pts.Count; i++) len += Vector2.Distance(pts[i - 1], pts[i]);
        return len;
    }

    private List<Vector2> Normalize(List<Vector2> pts)
    {
        List<Vector2> outp = new List<Vector2>(pts.Count);
        if (pts.Count == 0) return outp;

        Vector2 c = Vector2.zero;
        foreach (var p in pts) c += p;
        c /= pts.Count;

        List<Vector2> trans = new List<Vector2>();
        foreach (var p in pts) trans.Add(p - c);

        float minX = trans[0].x, maxX = trans[0].x, minY = trans[0].y, maxY = trans[0].y;
        foreach (var p in trans)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        float w = maxX - minX;
        float h = maxY - minY;
        float scale = Mathf.Max(w, h);
        if (scale <= Mathf.Epsilon) scale = 1f;

        foreach (var p in trans) outp.Add(p / scale);
        return outp;
    }
}
