using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ClickManager : MonoBehaviour
{
    private Camera mainCam;

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

    private void OnFingerDown(Finger finger)
    {
        Vector2 screenPos = finger.screenPosition;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCam.nearClipPlane));
        worldPos.z = 0;

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


    private void OnFingerUp(Finger finger)
    {
        Vector2 screenPos = finger.screenPosition;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCam.nearClipPlane));
        worldPos.z = 0;

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        foreach (var col in hits)
        {
            ClickableObject clickable = col.GetComponent<ClickableObject>();
            if (clickable != null)
            {
                clickable.HandleRelease(); // true = released, not canceled
            }
        }
    }

}
