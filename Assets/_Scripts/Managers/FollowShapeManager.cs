using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using NUnit.Framework;

public class FollowShapeManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Material line_mat;
    [SerializeField] private BackgroundGenerator backgroundGenerator;
    [SerializeField] private List<LineRenderer> shapes;

    [Header("Game Settings")]
    [SerializeField] private float delay_between_shapes = 1f;
    [SerializeField] private float visible_shape_timer = 30f;
    [SerializeField] private float invisible_shape_timer = 15f;
    [SerializeField] private Color visible_color;
    [SerializeField] private Color invisible_color;
    [SerializeField] private Color drawing_color;
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
        ShuffleShapes();
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
                    Debug.Log("Streak: " + streak);
                    HandleStreak(streak);
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
        if (streakCount >= 2)
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
        else if (streakCount >= 1)
        {
            line_mat.color = invisible_color;
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

}
