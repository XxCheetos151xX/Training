using UnityEngine;

public class NoisyFocusTargetMovement : MonoBehaviour
{
    private Rigidbody2D target_rb;
    private Camera cam;
    private float screen_center_x;
    private float speed;

    [HideInInspector] public bool isclicked;

    public bool isleft;
    public CircleCollider2D inverted_collider;

    private void Awake()
    {
        target_rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void OnEnable()
    {
        // Recalculate everything each time it's reused
        isclicked = false;
        target_rb.linearVelocity = Vector2.zero;

        screen_center_x = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)).x;
        isleft = transform.position.x < screen_center_x;
        speed = MiddleMan.Instance.noisyfocus_manager.spawned_target_speed;
    }

    private void FixedUpdate()
    {
        if (isclicked) return;

        if (isleft)
            target_rb.linearVelocity = new Vector2(speed, 0); // move right
        else
            target_rb.linearVelocity = new Vector2(-speed, 0); // move left
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Middle Line"))
        {
            gameObject.SetActive(false);
            MiddleMan.Instance.noisyfocus_manager.pooling.Enqueue(gameObject);
            MiddleMan.Instance.score_manager.misses++;
            MiddleMan.Instance.score_manager.LoseALife();
        }
    }
}