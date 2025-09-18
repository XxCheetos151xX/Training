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
    [SerializeField] private float visible_shape_timer = 30f;
    [SerializeField] private float invisible_shape_timer = 15f;
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
                Debug.Log("Streak: " + streak);
                HandleStreak(streak);
                goingforward = true;
                currentSegment = 0;
            }
        }

        if (is_invisible)
        {
            player.transform.position = touchWorldPos;
            StartCoroutine(UserDrawingRoutine());
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

    void HandleStreak(int streakCount)
    {
        if (streakCount >= 2)
        {
            is_invisible = false;
            current_shape_index++;
            if (current_shape_index >= shapes.Count)
            {
                GameEnded.Invoke();
            }
            system_line_mat.color = visible_color;
            this.streak = 0;
            StartCoroutine(ShapeHandler());
        }
        else if (streakCount >= 1)
        {
            system_line_mat.color = invisible_color;
            is_invisible = true;
        }
    }

    void GenerateShape()
    {
        active_shape = Instantiate(shapes[current_shape_index], Vector2.zero, shapes[current_shape_index].transform.rotation);
        if (player != null)
        {
            player.transform.position = active_shape.GetPosition(0);
        }
        currentSegment = 0;
        goingforward = true;
    }

    void DestroyShape()
    {
        Destroy(active_shape.gameObject);
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
                Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));

                if (col != null && col.OverlapPoint(worldPos))
                {
                    is_touching = true;
                    OnTouchperformed(); // invoke logic here
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
        if (active_shape != null)
        {
            DestroyShape();
            yield return new WaitForSeconds(delay_between_shapes);
            GenerateShape();
        }
    }


    IEnumerator UserDrawingRoutine()
    {
        userPoints.Clear();
        userLine.positionCount = 0;

        
        while (touchPressAction.action.IsPressed())
        {
            Vector2 screenPos = touchPositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));
            userLine.enabled = false;
            
            if (userPoints.Count == 0 || Vector3.Distance(userPoints[userPoints.Count - 1], worldPos) > 0.05f)
            {
                userPoints.Add(worldPos);
                userLine.positionCount = userPoints.Count;
                userLine.SetPosition(userPoints.Count - 1, worldPos);
            }

            yield return null; 
        }

        Debug.Log("User finished drawing with " + userPoints.Count + " points.");
        userLine.enabled = true;
       
    }

}
