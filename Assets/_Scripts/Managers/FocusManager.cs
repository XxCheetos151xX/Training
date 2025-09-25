using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class FocusManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private GameObject focus_point;
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private Transform screen_center;
    [SerializeField] private TextMeshProUGUI prestart_txt;
    [SerializeField] private FlickeringManager flickering_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private UIManager ui_manager;
    [SerializeField] private BackgroundGenerator background_generator;
    [SerializeField] private InputActionReference ClickAction;

    [Header("Game Settings")]
    [SerializeField] private float targetSize = 1;
    [SerializeField] private float score_tobe_added;
    [SerializeField] private Color first_color;
    [SerializeField] private Color second_color;
    [SerializeField] private List<float> radius;
    [SerializeField] private UnityEvent GameEnded;

    private FocusSO activeFocusSO;
    private GameObject spawned_target1;
    private GameObject spawned_target2;
    private Camera mainCam;
    private Vector3 spawn_pos;
    private float angle;
    private float timer;
    private float life_span;
    private float ellipse_width;
    private float ellipse_height;
    private float scale;
    private float score_ratio;
    private int streak;
    private int used_radius;
    private bool has_started;
    private List<float> levelstarttime = new List<float>();
    private List<float> lifespan = new List<float>();
    private List<float> flickeringspeed = new List<float>();
    private List<float> _scale = new List<float>();
    private List<float> _scoreratio = new List<float>();
    private List<bool> flickeringenabled = new List<bool>();
    private Dictionary<float, float> left_eye_data = new Dictionary<float, float>();
    private Dictionary<float, float> right_eye_data = new Dictionary<float, float>();

    public Queue<GameObject> targetQueue = new Queue<GameObject>();

    private void Awake()
    {
        ClickAction.action.performed += StartGame;
        ClickAction.action.Enable();
    }

    private void OnDisable()
    {
        ClickAction.action.performed += StartGame;
        ClickAction.action.Disable();
    }


    private void Start()
    {
        GameSetup();
        background_generator.GenerateConstantBackGround(0.5f);
    }


    void StartGame(InputAction.CallbackContext ctx)
    {
        if (!has_started)
        {
            Vector2 screenPos = ctx.ReadValue<Vector2>();

           
            Vector3 worldPos = mainCam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCam.transform.position.z))
            );

         
            focus_point.transform.position = new Vector3(0f, worldPos.y, 0f);

            focus_point.SetActive(true);

            has_started = true;
            focus_point.SetActive(true);

            prestart_txt.enabled = false;

            StartCoroutine(GameLoop());
            StartCoroutine(ui_manager.Timer());
            StartCoroutine(Spawntarget());
            StartCoroutine(ChangeRadius());
            StartCoroutine(flickering_manager.Flickering());
        }
    }

    void GameSetup()
    {
        initial_timer = activeFocusSO.timer;
        timer = 0;

        has_started = false;

        mainCam = Camera.main;

        var t1 = Instantiate(target_prefab);
        var t2 = Instantiate(target_prefab);

        t1.gameObject.GetComponent<SpriteRenderer>().color = first_color;
        t2.gameObject.GetComponent<SpriteRenderer>().color = second_color;

        t1.GetComponent<ClickableObject>().OnClick.AddListener(TargetCaptured);
        t2.GetComponent<ClickableObject>().OnClick.AddListener(TargetCaptured);

        targetQueue.Enqueue(t1);
        targetQueue.Enqueue(t2);

        t1.SetActive(false);
        t2.SetActive(false);

        if (activeFocusSO != null)
        {
            for (int i = 0; i < activeFocusSO.focuslevels.Count; i++)
            {
                levelstarttime.Add(activeFocusSO.focuslevels[i].starttime);
                lifespan.Add(activeFocusSO.focuslevels[i].lifespan);
                flickeringspeed.Add(activeFocusSO.focuslevels[i].flickeringspeed);
                flickeringenabled.Add(activeFocusSO.focuslevels[i].isflickering);
                _scale.Add(activeFocusSO.focuslevels[i].scale);
                _scoreratio.Add(activeFocusSO.focuslevels[i].scoreratio);
            }
        }
    }

    public void TargetCaptured(GameObject clickedObj)
    {
        if (activeFocusSO.focuslevels.Count > 1)
        {
            score_manager.user_score += score_tobe_added * score_ratio;
        }
        else
            score_manager.user_score += score_tobe_added;
        streak++;
        targetQueue.Enqueue(clickedObj);
        clickedObj.SetActive(false);
    }

    public void GameEnd()
    {
        StopAllCoroutines();
        
        Destroy(spawned_target1);
        Destroy(spawned_target2);
    }

    public void SetActiveFocusSO(FocusSO val) => activeFocusSO = val;

    IEnumerator ChangeRadius()
    {
        while (true)
        {
            if (streak >= 4)
            {
                if (used_radius < radius.Count - 1)
                {
                    used_radius++;
                    print(used_radius);
                }
                streak = 0;
            }
            yield return null;
        }
    }

    IEnumerator Spawntarget()
    {
        float x, y;

        while (true)
        {
            
            while (targetQueue.Count < 2)
                yield return null;

            angle = Random.Range(-45, 45) * Mathf.Deg2Rad;

            ellipse_width = radius[used_radius];
            ellipse_height = radius[used_radius] * 0.6f;

            x = Mathf.Cos(angle) * ellipse_width;
            y = Mathf.Sin(angle) * ellipse_height;

            spawn_pos = screen_center.position + new Vector3(x, y, 0);
            screen_center.localScale = new Vector3(targetSize, targetSize, targetSize);

            spawned_target1 = targetQueue.Dequeue();
            spawned_target2 = targetQueue.Dequeue();

            spawned_target1.SetActive(true);
            spawned_target2.SetActive(true);

            spawned_target1.transform.position = spawn_pos;
            spawned_target2.transform.position = -spawn_pos;

            spawned_target1.transform.localScale = new Vector3(scale, scale, scale);
            spawned_target2.transform.localScale = new Vector3(scale, scale, scale);

            score_manager.total_score += (score_tobe_added * 2);

            float t = 0f;
            while (t < life_span && (spawned_target1.activeSelf || spawned_target2.activeSelf))
            {
                t += Time.deltaTime;
                yield return null;
            }



            if (spawned_target1.activeSelf)
            {
                score_manager.misses++;
                spawned_target1.SetActive(false);
                targetQueue.Enqueue(spawned_target1);
                left_eye_data.Add(score_manager.misses, timer);
            }

            if (spawned_target2.activeSelf)
            {
                score_manager.misses++;
                spawned_target2.SetActive(false);
                targetQueue.Enqueue(spawned_target2);
                right_eye_data.Add(score_manager.misses, timer);
            }
        }
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            initial_timer -= Time.deltaTime;

            for (int i = levelstarttime.Count - 1; i >= 0; i--)
            {
                if (timer >= levelstarttime[i])
                {
                    life_span = lifespan[i];
                    flickering_manager.flickeringspeed = flickeringspeed[i];
                    flickering_manager.isflickering = flickeringenabled[i];
                    scale = _scale[i];
                    score_ratio = _scoreratio[i];
                    print(score_ratio);
                    break;
                }
            }

            if (initial_timer <= 0)
            {
                GameEnded.Invoke();
            }

            yield return null;
        }
    }
}
