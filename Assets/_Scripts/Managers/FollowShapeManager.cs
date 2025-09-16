using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FollowShapeManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Material line_mat;
    [SerializeField] private BackgroundGenerator backgroundGenerator;
    [SerializeField] private UIManager uimanager;
    [SerializeField] private List<LineRenderer> shapes;

    [Header("Game Settings")]
    [SerializeField] private float delay_between_shapes = 1f;
    [SerializeField] private float visible_shape_timer = 30f;
    [SerializeField] private float invisible_shape_timer = 15f;
    [SerializeField] private Color visible_color;
    [SerializeField] private Color invisible_color;
    [SerializeField] private InputActionReference TouchAction;
    [SerializeField] private UnityEvent GameEnded;

    private Camera mainCam;
    private LineRenderer active_shape;
    private int streak = 0;
    private int currentSegment = 0;
    private int current_shape_index = 0;
    private bool is_touching = false;
    private bool is_invisible = false;
    private bool goingforward = true; 
  

    private void Awake()
    {
        TouchAction.action.performed += OnTouchperformed;
        TouchAction.action.Enable();
    }

    private void OnDestroy()
    {
        TouchAction.action.performed -= OnTouchperformed;
        TouchAction.action.Disable();
    }

    private void Start()
    {
        mainCam = Camera.main;

        line_mat.color = visible_color;

        ScreenSetup();
        backgroundGenerator.GenerateConstantBackGround(0.5f);
        StartCoroutine(uimanager.Timer());
        StartCoroutine(HandleTouch());
        GenerateShape();
    }

    void ScreenSetup()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
    }

    void OnTouchperformed(InputAction.CallbackContext ctx)
    {
        if (is_touching && active_shape != null && player != null)
        {
            if (goingforward)
            {
                // forward direction
                if (currentSegment < active_shape.positionCount - 1)
                {
                    Vector3 a = active_shape.GetPosition(currentSegment);
                    Vector3 b = active_shape.GetPosition(currentSegment + 1);

                    Vector2 touchPos = mainCam.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
                    Vector3 projected = ProjectPointOnSegment(a, b, touchPos);

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
                // backward direction
                if (currentSegment > 0)
                {
                    Vector3 a = active_shape.GetPosition(currentSegment);
                    Vector3 b = active_shape.GetPosition(currentSegment - 1);

                    Vector2 touchPos = mainCam.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
                    Vector3 projected = ProjectPointOnSegment(a, b, touchPos);

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
                    StopCoroutine(GameLoop());
                    Debug.Log("Streak: " + streak);
                    HandleStreak(streak);
                    StartCoroutine(GameLoop());
                    goingforward = true;
                    currentSegment = 0;
                }
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

    void HandleStreak(int streakCount)
    {
        if (streakCount >= 5)
        {
            is_invisible = false;
            current_shape_index++;
            if (current_shape_index >= shapes.Count)
            {
                GameEnded.Invoke();
            }
            line_mat.color = visible_color;
            this.streak = 0;
            StartCoroutine(ShapeHandler());
        }
        else if (streakCount >= 3)
        {
            line_mat.color = invisible_color;
            is_invisible = true;
        }
    }

    void GenerateShape()
    {
        active_shape = Instantiate(shapes[current_shape_index], Vector2.zero, Quaternion.identity);
        if (player != null)
        {
            player.transform.position = active_shape.GetPosition(0);
        }
        currentSegment = 0;
        goingforward = true;
        StartCoroutine(GameLoop());
    }

    void DestroyShape()
    {
        Destroy(active_shape.gameObject);
        StopCoroutine(GameLoop());
    }

    IEnumerator HandleTouch()
    {
        CircleCollider2D col = player.GetComponent<CircleCollider2D>();

        while (true)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                Vector2 screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));

                if (col != null && col.OverlapPoint(worldPos))
                {
                    is_touching = true;
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

    IEnumerator GameLoop()
    {
        if (!is_invisible)
            initial_timer = visible_shape_timer;
        else
            initial_timer = invisible_shape_timer;

        while (initial_timer > 0)
        {
            initial_timer -= Time.deltaTime;
            yield return null;
        }

        if (initial_timer <= 0)
        {
            if (active_shape != null)
            {
                GameEnded.Invoke();
            }
        }
    }
}
