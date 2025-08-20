using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ClickManager : MonoBehaviour
{
    private Camera mainCam;
    public static Vector2 LastClickWorldPos;

    private void OnEnable()
    {
        mainCam = Camera.main;

        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick(Mouse.current.position.ReadValue());
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleRelease(Mouse.current.position.ReadValue());
        }
    }

    private void OnFingerDown(Finger finger)
    {
        HandleClick(finger.screenPosition);
    }

    private void OnFingerUp(Finger finger)
    {
        HandleRelease(finger.screenPosition);
    }

    private void HandleClick(Vector2 screenPos)
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCam.nearClipPlane));
        worldPos.z = 0;

        LastClickWorldPos = worldPos;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        foreach (var col in hits)
        {
            ClickableObject clickable = col.GetComponent<ClickableObject>();
            if (clickable != null)
            {
                clickable.HandleClick(clickable.gameObject);
            }
        }
    }

    private void HandleRelease(Vector2 screenPos)
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCam.nearClipPlane));
        worldPos.z = 0;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        foreach (var col in hits)
        {
            ClickableObject clickable = col.GetComponent<ClickableObject>();
            if (clickable != null)
            {
                clickable.HandleRelease();
            }
        }
    }
}
