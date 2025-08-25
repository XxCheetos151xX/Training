using UnityEngine;

public class NoisyFocusTargetMovement : MonoBehaviour
{
    private Rigidbody2D target_rb;
    private Camera cam;
    private float screen_center_x;
    private float speed;
    private bool isleft;
    private void Start()
    {
        target_rb = gameObject.GetComponent<Rigidbody2D>();
        cam = Camera.main;
        screen_center_x = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)).x;
        isleft = gameObject.transform.position.x < screen_center_x;
        speed = MiddleMan.Instance.noisyfocus_manager.spawned_target_speed;
    }

    private void FixedUpdate()
    {
        if (isleft)
        {
            MoveLeft();
        }
        else
        {
            MoveRight();
        }
    }

    void MoveRight()
    {
        target_rb.linearVelocity = new Vector2(-speed, 0); 
    }

    void MoveLeft()
    {
        target_rb.linearVelocity = new Vector2(speed, 0); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Middle Line"))
        {
            Destroy(gameObject);
            MiddleMan.Instance.score_manager.misses++;
        }
    }
}
