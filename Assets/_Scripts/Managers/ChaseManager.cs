using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class ChaseManager : MonoBehaviour
{
    [Header("Game Refrences")]
    [SerializeField] private LayerMask player_mask;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject target;
    [SerializeField] private Image blackscreen;
    [SerializeField] private Camera mainCam;
    [SerializeField] private LineRenderer line;
    [SerializeField] private InputActionReference TouchActionRef;
    [SerializeField] private InputActionReference ClickActionRef; 
    [SerializeField] private UnityEvent GameEnd;

    [Header("Game Settings")]
    [SerializeField] private float min_dist;
    [SerializeField] private float max_dist;
    [SerializeField] private TextMeshProUGUI timer_txt;
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private GameObject end_panel;

    private float target_speed;
    private float target_angle;
    private float minY, minX, maxX, maxY;
    private float distance;
    private float timer;
    private float initial_time;
    private float green_line_timer;
    private float score;
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
    }

    private void Start()
    {
        Setup();
        GameSetup();
        PickDirection();
    }

    public void SetActiveChaseSO(ChaseSO val) => activeChaseSO = val;


    void Setup()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = Camera.main.transform.position.y - halfHeight;
        minX = Camera.main.transform.position.x - halfWidth;
        maxX = Camera.main.transform.position.x + halfWidth;
        maxY = Camera.main.transform.position.y + halfHeight;
        timer = 0;
        green_line_timer = 0;
        initial_time = activeChaseSO.timer;
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
            StartCoroutine(TargetBehaviour());
            StartCoroutine(GameLoop());
            StartCoroutine(Flickering());
            StartCoroutine(LineAdjustment());

            has_started = true;
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        is_touching = true;
        print(is_touching);
    }

    private void OnClickCanceled(InputAction.CallbackContext ctx)
    {
        is_touching = false;
        print(is_touching);
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
        timer_txt.alpha = 0f;
        score = (green_line_timer / activeChaseSO.timer) * 100;
        score_txt.text = score.ToString("F2") + "%";
        end_panel.SetActive(true);
        
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

            if (target.transform.position.x < minX || target.transform.position.x > maxX)
            {
                target_direction.x *= -1;
            }

            if (target.transform.position.y < minY || target.transform.position.y > maxY)
            {
                target_direction.y *= -1;
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

            if (distance >= min_dist && distance <= max_dist && is_touching)
            {
                line.startColor = Color.blue;
                line.endColor = Color.blue;
                green_line_timer += Time.deltaTime;
            }
            else
            {
                line.startColor = Color.yellow;
                line.endColor = Color.yellow;
            }
            yield return null;
        }

    }

    IEnumerator GameLoop()
    {
        while (timer != activeChaseSO.timer)
        {
            timer += Time.deltaTime;
            initial_time -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(initial_time / 60f);
            int seconds = Mathf.FloorToInt(initial_time % 60f);
            timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            for (int i = levelstarttime.Count - 1; i >= 0; i--)
            {
                if (timer >= levelstarttime[i])
                {
                    target_speed = _target_speed[i];
                    break;
                }
            }
            if (initial_time <= 0)
            {
                GameEnd.Invoke();
            }
            yield return null;
        }
    }


    IEnumerator Flickering()
    {
        float flickerTimer = 0f;
        float currentFlickerSpeed = 0f;
        bool isFlickering = false;

        while (true)
        {
            for (int i = levelstarttime.Count - 1; i >= 0; i--)
            {
                if (timer >= levelstarttime[i])
                {
                    isFlickering = _flickerenabled[i];
                    currentFlickerSpeed = _flickerspeed[i];
                    break;
                }
            }

            if (isFlickering)
            {
                flickerTimer += Time.deltaTime;
                if (flickerTimer >= currentFlickerSpeed)
                {
                    blackscreen.enabled = !blackscreen.enabled;
                    flickerTimer = 0f;
                }
            }
            else
            {
                if (blackscreen.enabled)
                    blackscreen.enabled = false;
            }

            yield return null;
        }
    }

}
