using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Collections.LowLevel.Unsafe;

public class FollowShapeManager : AbstractGameManager
{
    [Header("Game Refrences")]
    [SerializeField] private GameObject player;
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
    private int current_shape_index = 0;
    private bool is_touching = false;
    private bool is_invisible = false;

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

        ScreenSetup();
        backgroundGenerator.GenerateConstantBackGround(0.5f);
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
        if (is_touching && active_shape != null)
        {
            Vector2 screenPos = ctx.ReadValue<Vector2>();
            Vector3 touchPos = new Vector3(screenPos.x, screenPos.y, 10);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(touchPos);

            Vector3 nearest = GetNearestPointOnLine(worldPos);
            player.transform.position = nearest;

            // check if reached end
            if (Vector3.Distance(nearest, active_shape.GetPosition(active_shape.positionCount - 1)) < 0.2f)
            {
                streak++;
                HandleStreak(streak);
            }
        }
    }

    private Vector3 GetNearestPointOnLine(Vector3 target)
    {
        Vector3 nearestPoint = Vector3.zero;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < active_shape.positionCount - 1; i++)
        {
            Vector3 a = active_shape.GetPosition(i);
            Vector3 b = active_shape.GetPosition(i + 1);

            Vector3 projected = ProjectPointOnSegment(a, b, target);
            float dist = (target - projected).sqrMagnitude;

            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPoint = projected;
            }
        }
        return nearestPoint;
    }

    private Vector3 ProjectPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    public void TouchPerformed() => is_touching = true;
    public void TouchCanceled() => is_touching = false;

    public void GameEnd()
    {
        StopAllCoroutines();
    }

    void HandleStreak(int streak)
    {
        if (streak >= 3)
        {
            is_invisible = true;
            active_shape.startColor = invisible_color;
            active_shape.endColor = invisible_color;
        }
        else if (streak >= 5)
        {
            current_shape_index++;
        }
    }

    void GenerateShape()
    {
        active_shape = Instantiate(shapes[current_shape_index], new Vector2(0, 0), Quaternion.identity);
    }

    void DestroyShape()
    {
        Destroy(active_shape);
    }


}
