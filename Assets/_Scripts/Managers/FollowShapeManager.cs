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
    [SerializeField] private Animator result_screen_animator;
    [SerializeField] private BackgroundGenerator backgroundGenerator;
    [SerializeField] private LineRenderer userLine; 
    [SerializeField] private List<LineRenderer> shapes;

    [Header("Game Settings")]
    [SerializeField] private float delay_between_shapes;
    [SerializeField] private float threshold;
    [SerializeField] private float smoothing_factor;
    [SerializeField] private Color visible_color;
    [SerializeField] private Color invisible_color;
    [SerializeField] private Color drawing_color;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference touchPositionAction; // Vector2
    [SerializeField] private InputActionReference touchPressAction;    // Button

    [SerializeField] private UnityEvent GameEnded;

    private Camera mainCam;
    private LineRenderer active_shape;
    private float dtwWeight = 0.4f;
    private float hausdorffWeight = 0.5f;
    private float directionWeight = 0.1f;
    private int streak = 0;
    private int currentSegment = 0;
    private int current_shape_index = 0;
    private bool is_touching = false;
    private bool is_invisible = false;
    private bool goingforward = true;
    private List<Vector3> userPoints = new List<Vector3>();

    // internal state flags        
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
               
                streak++;
                HandleStreak(streak);

                Debug.Log("Full cycle completed and streak committed immediately.");
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

        // If the player is already holding their finger, keep ball active
        if (touchPressAction.action.IsPressed())
        {
            is_touching = true;
            OnTouchperformed();
        }

    }

    void DestroyShape()
    {
        if (active_shape != null)
            Destroy(active_shape.gameObject);
        active_shape = null;
    }


    public Vector3[] PreprocessLine(Vector3[] rawPoints, int targetPointCount)
    {
        if (rawPoints == null || rawPoints.Length == 0)
            return new Vector3[0];

        // Step 1: Resample to fixed point count
        Vector3[] resampled = ResamplePoints(rawPoints, targetPointCount);

        // Step 2: Normalize scale and position
        Vector3[] normalized = NormalizePoints(resampled);

        // Step 3: Smooth to reduce noise
        Vector3[] smoothed = SmoothPoints(normalized, smoothing_factor);

        return smoothed;
    }

    private Vector3[] ResamplePoints(Vector3[] points, int targetCount)
    {
        if (points.Length <= 1 || targetCount <= 1) return points;

        // Calculate total path length
        float totalLength = 0f;
        for (int i = 1; i < points.Length; i++)
        {
            totalLength += Vector3.Distance(points[i - 1], points[i]);
        }

        float interval = totalLength / (targetCount - 1);
        Vector3[] resampled = new Vector3[targetCount];
        resampled[0] = points[0];

        float currentDistance = interval;
        int currentIndex = 1;

        for (int i = 1; i < targetCount - 1; i++)
        {
            float segmentDistance = 0f;

            // Find the segment where currentDistance lies
            while (currentIndex < points.Length - 1 &&
                   currentDistance > segmentDistance + Vector3.Distance(points[currentIndex - 1], points[currentIndex]))
            {
                segmentDistance += Vector3.Distance(points[currentIndex - 1], points[currentIndex]);
                currentIndex++;
            }

            // Interpolate within the segment
            float segmentLength = Vector3.Distance(points[currentIndex - 1], points[currentIndex]);
            float t = (currentDistance - segmentDistance) / segmentLength;
            resampled[i] = Vector3.Lerp(points[currentIndex - 1], points[currentIndex], t);

            currentDistance += interval;
        }

        resampled[targetCount - 1] = points[points.Length - 1];
        return resampled;
    }


    private Vector3[] NormalizePoints(Vector3[] points)
    {
        if (points.Length == 0) return points;

        // Find bounding box
        Vector3 min = points[0];
        Vector3 max = points[0];

        foreach (Vector3 point in points)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        // Calculate center and size
        Vector3 center = (min + max) * 0.5f;
        float size = Mathf.Max(max.x - min.x, max.y - min.y);
        if (size == 0) size = 1f; // Avoid division by zero

        // Normalize to unit square centered at origin
        Vector3[] normalized = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            normalized[i] = (points[i] - center) / size;
        }

        return normalized;
    }

    private Vector3[] SmoothPoints(Vector3[] points, float smoothFactor)
    {
        if (points.Length <= 2) return points;

        Vector3[] smoothed = new Vector3[points.Length];
        smoothed[0] = points[0];
        smoothed[points.Length - 1] = points[points.Length - 1];

        for (int i = 1; i < points.Length - 1; i++)
        {
            // Simple moving average
            smoothed[i] = Vector3.Lerp(points[i],
                (points[i - 1] + points[i] + points[i + 1]) / 3f,
                smoothFactor);
        }

        return smoothed;
    }

    private bool CompareShapes(LineRenderer systemShape, List<Vector3> userPoints, int sampleCount, float threshold)
    {
        if (systemShape == null || userPoints.Count < 2) return false;

        // Get system shape points
        Vector3[] systemPoints = new Vector3[systemShape.positionCount];
        systemShape.GetPositions(systemPoints);

        // Convert user points to array
        Vector3[] userPointsArray = userPoints.ToArray();

        // Preprocess both curves
        Vector3[] processedSystem = PreprocessLine(systemPoints, sampleCount);
        Vector3[] processedUser = PreprocessLine(userPointsArray, sampleCount);

        // Calculate similarity score (0-1 where 1 = perfect match)
        float similarity = CalculateSimilarity(processedSystem, processedUser);

        Debug.Log($"Shape similarity: {similarity:F2} (threshold: {threshold})");

        return similarity >= threshold;
    }

    private float CalculateSimilarity(Vector3[] template, Vector3[] userInput)
    {
        if (template.Length == 0 || userInput.Length == 0) return 0f;

        // Calculate multiple similarity metrics
        float dtwDistance = CalculateDTW(template, userInput);
        float hausdorffDist = CalculateHausdorff(template, userInput);
        float directionalSimilarity = CalculateDirectionSimilarity(template, userInput);

        // Normalize scores (0 = perfect, 1 = terrible)
        float normalizedDTW = NormalizeDTWScore(dtwDistance);
        float normalizedHausdorff = NormalizeHausdorffScore(hausdorffDist);
        float normalizedDirection = 1f - directionalSimilarity;

        // Weighted combination
        float finalDistance = dtwWeight * normalizedDTW +
                             hausdorffWeight * normalizedHausdorff +
                             directionWeight * normalizedDirection;

        // Convert to similarity (1 = perfect match)
        return Mathf.Clamp01(1f - finalDistance);
    }

    private float CalculateDTW(Vector3[] sequence1, Vector3[] sequence2)
    {
        int n = sequence1.Length;
        int m = sequence2.Length;

        float[,] dtw = new float[n + 1, m + 1];

        // Initialize with large values
        for (int i = 0; i <= n; i++)
            for (int j = 0; j <= m; j++)
                dtw[i, j] = float.MaxValue;

        dtw[0, 0] = 0f;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                float cost = Vector3.Distance(sequence1[i - 1], sequence2[j - 1]);
                dtw[i, j] = cost + Mathf.Min(
                    dtw[i - 1, j],     // insertion
                    dtw[i, j - 1],     // deletion  
                    dtw[i - 1, j - 1]  // match
                );
            }
        }

        return dtw[n, m];
    }

    private float CalculateHausdorff(Vector3[] setA, Vector3[] setB)
    {
        float maxMinDistance = 0f;

        // For each point in setA, find closest point in setB
        foreach (Vector3 a in setA)
        {
            float minDistance = float.MaxValue;
            foreach (Vector3 b in setB)
            {
                float dist = Vector3.Distance(a, b);
                if (dist < minDistance) minDistance = dist;
            }
            if (minDistance > maxMinDistance) maxMinDistance = minDistance;
        }

        return maxMinDistance;
    }

    private float CalculateDirectionSimilarity(Vector3[] template, Vector3[] userInput)
    {
        int minLength = Mathf.Min(template.Length, userInput.Length);
        if (minLength < 2) return 0f;

        float totalDot = 0f;
        int comparisons = 0;

        for (int i = 1; i < minLength; i++)
        {
            Vector3 templateDir = (template[i] - template[i - 1]).normalized;
            Vector3 userDir = (userInput[i] - userInput[i - 1]).normalized;


            float dot = Vector3.Dot(templateDir, userDir);


            float similarity = (dot + 1f) * 0.5f;

            totalDot += similarity;
            comparisons++;
        }

        return comparisons > 0 ? totalDot / comparisons : 0f;
    }

    private float NormalizeDTWScore(float dtwDistance)
    {

        float maxExpected = 5f;
        return Mathf.Clamp01(dtwDistance / maxExpected);
    }

    private float NormalizeHausdorffScore(float hausdorffDistance)
    {

        float maxExpected = 2f;
        return Mathf.Clamp01(hausdorffDistance / maxExpected);
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

            

            if (drawingAllowed && !userDrawingRunning)
            {
                userDrawingRunning = true;
                StartCoroutine(UserDrawingRoutine());
            }

            if (!is_invisible && col != null && pressed)
            {
                // Don’t require overlap strictly on first frame of new shape
                if (col.OverlapPoint(worldPos) || is_touching)
                {
                    is_touching = true;
                    OnTouchperformed();
                }
            }
            else
            {
                is_touching = false;
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

        bool matched = false;
        if (active_shape != null && userPoints.Count > 1)
        {
            matched = CompareShapes(active_shape, userPoints, 64, threshold);
          

            if (matched)
            {
                streak = 0; 
                result_screen_animator.Play("Matched", -1, 0);
            }
            else
            {
                result_screen_animator.Play("Didn't Match", -1, 0);
            }
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




   

}
