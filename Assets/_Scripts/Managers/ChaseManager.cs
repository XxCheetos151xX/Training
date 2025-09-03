using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class ChaseManager : AbstractGameManager
{
    [Header("Game Refrences")]
    [SerializeField] private LayerMask player_mask;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject target;
    [SerializeField] private Camera mainCam;
    [SerializeField] private LineRenderer line;
    [SerializeField] private InputActionReference TouchActionRef;
    [SerializeField] private InputActionReference ClickActionRef; 
    [SerializeField] private FlickeringManager flickering_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private UIManager ui_manager;
    [SerializeField] private BackgroundGenerator background_generator;

    [Header("Game Settings")]
    [SerializeField] private float max_dist;
    [SerializeField] private Color normal_color;
    [SerializeField] private Color right_color;
    [SerializeField] private UnityEvent GameEnd;

    private float target_speed;
    private float target_angle;
    private float minY, minX, maxX, maxY;
    private float distance;
    private float timer;
    private bool has_started;
    private bool is_touching;
    private ChaseSO activeChaseSO;
    private Vector3 target_direction;
    private RaycastHit2D hit;
    private Vector3 target_currentPos;
    private List<float> levelstarttime = new List<float>();
    private List<float> _target_speed = new List<float>();
    private List<float> _flickerspeed  = new List<float>();
    private List<bool> _flickerenabled = new List<bool>();
   
    
    private void Awake()
    {
        Application.targetFrameRate = -1;

        TouchActionRef.action.performed += OnTouchPerformed;
        ClickActionRef.action.performed += OnClickPerformed;
        ClickActionRef.action.canceled += OnClickCanceled;
        TouchActionRef.action.Enable();
        ClickActionRef.action.Enable();
    }

    private void OnDestroy()
    {
        TouchActionRef.action.performed -= OnTouchPerformed;
        ClickActionRef.action.performed -= OnClickPerformed;
        ClickActionRef.action.canceled -= OnClickCanceled;
        TouchActionRef.action.Disable();
        ClickActionRef.action.Disable();
    }

    private void Start()
    {
        Setup();
        GameSetup();
        PickDirection();
        background_generator.GenerateConstantBackGround(0.5f);       
    }

    public void SetActiveChaseSO(ChaseSO val) => activeChaseSO = val;


    void Setup()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = Camera.main.transform.position.y - halfHeight + 0.5f;
        minX = Camera.main.transform.position.x - halfWidth + 0.5f;
        maxX = Camera.main.transform.position.x + halfWidth - 0.5f;
        maxY = Camera.main.transform.position.y + halfHeight - 0.5f;
        timer = 0;
        initial_timer = activeChaseSO.timer;
        score_manager.total_score = activeChaseSO.timer;
        has_started = false;
        is_touching = false;
        line.positionCount = 2;
    }

    void GameSetup()
    {
        if (activeChaseSO != null)
        {
            for (int i = 0; i < activeChaseSO.ChaseLevels.Count; i++)
            {
                levelstarttime.Add (activeChaseSO.ChaseLevels[i].startTime);
                _target_speed.Add (activeChaseSO.ChaseLevels[i].ballSpeed);
                _flickerspeed.Add (activeChaseSO.ChaseLevels[i].flickerSpeed);
                _flickerenabled.Add(activeChaseSO.ChaseLevels[i].isFlickering);
            }
        }
    }

    private void OnTouchPerformed(InputAction.CallbackContext ctx)
    {   
        Vector2 screenPos = ctx.ReadValue<Vector2>();
        Vector3 touchPos = new Vector3(screenPos.x, screenPos.y, 10);
        player.transform.position = mainCam.ScreenToWorldPoint(touchPos);

        if (!has_started)
        {
            StartCoroutine(ui_manager.Timer());
            StartCoroutine(TargetBehaviour());
            StartCoroutine(GameLoop());
            StartCoroutine(flickering_manager.Flickering());
            StartCoroutine(LineAdjustment());

            has_started = true;
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        is_touching = true;
    }

    private void OnClickCanceled(InputAction.CallbackContext ctx)
    {
        is_touching = false;
    }
   

    void PickDirection()
    {
        target_angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        target_direction = new Vector3(Mathf.Cos(target_angle), Mathf.Sin(target_angle), 0).normalized;
        target_currentPos = target.transform.position + target_direction * Random.Range(1f, 3f);
    }
    
    
    public void EndGame()
    {   
        StopAllCoroutines();
        player.SetActive(false);
        target.SetActive(false);
        line.enabled = false;
    }
    
    
    IEnumerator TargetBehaviour()
    {
        while (true)
        {
            hit = Physics2D.Raycast(target.transform.position, target_direction, 20, player_mask);

            target.transform.position += target_direction * target_speed * Time.deltaTime;

            if (hit.collider != null)
            {
                PickDirection();
            }

            if (Vector3.Distance (target.transform.position, target_currentPos) < 0.1f)
            {
                PickDirection();
            }

            if (target.transform.position.x < minX)
            {
                target_direction.x *= -1;
                target.transform.position = new Vector3(minX, target.transform.position.y, target.transform.position.z);
            }
            else if (target.transform.position.x > maxX)
            {
                target_direction.x *= -1;
                target.transform.position = new Vector3(maxX, target.transform.position.y, target.transform.position.z);
            }

            if (target.transform.position.y < minY)
            {
                target_direction.y *= -1;
                target.transform.position = new Vector3(target.transform.position.x, minY, target.transform.position.z);
            }
            else if (target.transform.position.y > maxY)
            {
                target_direction.y *= -1;
                target.transform.position = new Vector3(target.transform.position.x, maxY, target.transform.position.z);
            }


            yield return null;
        }
    }
    IEnumerator LineAdjustment()
    {
        while (true)
        {
            distance = Vector3.Distance(player.transform.position, target.transform.position);

            line.SetPosition (0, player.transform.position);
            line.SetPosition(1, target.transform.position);

            if (distance <= max_dist && is_touching)
            {
                line.startColor = right_color;
                line.endColor = right_color;
                score_manager.user_score += Time.deltaTime;
            }
            else
            {
                line.startColor = normal_color;
                line.endColor = normal_color;
            }
            yield return null;
        }

    }

    IEnumerator GameLoop()
    {
        while (timer != activeChaseSO.timer)
        {
            timer += Time.deltaTime;
            for (int i = levelstarttime.Count - 1; i >= 0; i--)
            {
                if (timer >= levelstarttime[i])
                {
                    target_speed = _target_speed[i];
                    flickering_manager.flickeringspeed = _flickerspeed[i];
                    flickering_manager.isflickering = _flickerenabled[i];
                    break;
                }
            }
            if (initial_timer <= 0)
            {
                GameEnd.Invoke();
            }
            yield return null;
        }
    }



}
