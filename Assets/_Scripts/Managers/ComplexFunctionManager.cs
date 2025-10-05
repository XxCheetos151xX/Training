using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class ComplexFunctionManager : AbstractGameManager
{
    [Header("Game Refrences")]
    [SerializeField] private GameObject left_central_point;
    [SerializeField] private GameObject right_central_point;
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private ScoreManager scoremanager;
    [SerializeField] private UIManager uimanager;
    [SerializeField] private BackgroundGenerator backgroundgenerator;

    [Header("Game Settings")]
    [SerializeField] private float score_tobe_added;
    [SerializeField] private float flicker_speed;
    [SerializeField] private UnityEvent GameEnded;
    [SerializeField] private UnityEvent SequenceEnd;

    private ComplexFunctionsSO activeComplexFunctionsSO;
    private Camera cam;
    private GameObject left_spawned_target;
    private GameObject right_spawned_target;
    private SpriteRenderer left_central_point_renderer;
    private SpriteRenderer right_central_point_renderer;
    private float minX, minY, maxX, maxY, midX;
    private float timer;
    private float flickering_time;
    private float min_flickering_cooldown;
    private float max_flickering_cooldown;
    private float target_life_span;
    private float min_delay_between_targets;
    private float max_delay_between_targets;
    private float score_ratio;
    private bool is_flickering;
    private bool is_flickering_together;
    private bool isright;
    private bool istimeless;
    private string chosen_mode;
    private Vector2 rightpos, leftpos;
    private List<float> _starttime = new List<float>();
    private List<float> _flickeringtime = new List<float>();
    private List<float> _minflickeringcooldown = new List<float>();
    private List<float> _maxflickeringcooldown = new List<float>();
    private List<float> _targetlifespan = new List<float>();
    private List<float> _mindelaybetweentargets = new List<float>();
    private List<float> _maxdelaybetweentargets = new List<float>();
    private List<float> _scoreratio = new List<float>();
    private List<bool> _isflickeringtogether = new List<bool>();
    private List<GameObject> active_targets = new List<GameObject>();

    void Start()
    {
        SetupScreen();
        GameSetup();
        backgroundgenerator.GenerateConstantBackGround(0.5f);
        StartCoroutine(uimanager.Timer());
        StartCoroutine(GameLoop());
        StartCoroutine(StartFlickering());
        StartCoroutine(SpawnTargets());
    }

    void SetupScreen()
    {
        chosen_mode = PlayerPrefs.GetString("GameMode");

        cam = Camera.main;

        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = cam.transform.position.y - halfHeight + 0.5f;
        minX = cam.transform.position.x - halfWidth + 0.5f;
        maxX = cam.transform.position.x + halfWidth - 0.5f;
        maxY = cam.transform.position.y + halfHeight - 0.5f;

        midX = (minX + maxX) / 2;

    }


    void GameSetup()
    {
        initial_timer = activeComplexFunctionsSO.timer;

        timer = 0;

        istimeless = PlayerPrefs.GetInt("IsTimeless") == 1;

        left_central_point_renderer = left_central_point.GetComponent<SpriteRenderer>();
        right_central_point_renderer = right_central_point.GetComponent<SpriteRenderer>();

        left_central_point.transform.position = new Vector3(((minX - midX) / 2), 0, 0);
        right_central_point.transform.position = new Vector3(((maxX - midX) / 2), 0, 0);


        for (int i = 0; i < activeComplexFunctionsSO.complexfunctionslevels.Count; i++)
        {
            _starttime.Add(activeComplexFunctionsSO.complexfunctionslevels[i].starttime);
            _flickeringtime.Add(activeComplexFunctionsSO.complexfunctionslevels[i].flickeringtime);
            _minflickeringcooldown.Add(activeComplexFunctionsSO.complexfunctionslevels[i].minflickeringcooldown);
            _maxflickeringcooldown.Add(activeComplexFunctionsSO.complexfunctionslevels[i].maxflickeringcooldown);
            _targetlifespan.Add(activeComplexFunctionsSO.complexfunctionslevels[i].targetlifespan);
            _mindelaybetweentargets.Add(activeComplexFunctionsSO.complexfunctionslevels[i].mindelaybetweentargets);
            _maxdelaybetweentargets.Add(activeComplexFunctionsSO.complexfunctionslevels[i].maxdelaybetweentargets);
            _isflickeringtogether.Add(activeComplexFunctionsSO.complexfunctionslevels[i].isflickeringtogether);
            _scoreratio.Add(activeComplexFunctionsSO.complexfunctionslevels[i].scoreratio);
        }
    }

    void ClearTargets()
    {
        foreach (var t in active_targets)
        {
            Destroy(t);
        }
    }

    public void GameEnd()
    {
        StopAllCoroutines();
        ClearTargets();
    }

    public void ContinueSequence()
    {
        chosen_mode = PlayerPrefs.GetString("GameMode", "None");
        if (chosen_mode == GameMode.GeneralEval.ToString() && SequenceManager.Instance != null)
        {
            SequenceManager.Instance.LoadNextScene();
        }
    }



    void GeneratePositions()
    {

        float leftRadius = left_central_point_renderer.bounds.extents.magnitude;
        float rightRadius = right_central_point_renderer.bounds.extents.magnitude;

        leftpos = new Vector2(Random.Range(minX, midX - 0.5f), Random.Range(minY, maxY));
        Vector2 leftDir = (leftpos - (Vector2)left_central_point.transform.position).normalized;
        float leftDist = Vector2.Distance(leftpos, left_central_point.transform.position);
        if (leftDist < leftRadius)
            leftpos = (Vector2)left_central_point.transform.position + leftDir * leftRadius;

        
        rightpos = new Vector2(Random.Range(midX + 0.5f, maxX), Random.Range(minY, maxY));
        Vector2 rightDir = (rightpos - (Vector2)right_central_point.transform.position).normalized;
        float rightDist = Vector2.Distance(rightpos, right_central_point.transform.position);
        if (rightDist < rightRadius)
            rightpos = (Vector2)right_central_point.transform.position + rightDir * rightRadius;
    }


    void SpawnRightTarget()
    {
        right_spawned_target = Instantiate(target_prefab, rightpos, Quaternion.identity);
        right_spawned_target.GetComponent<ClickableObject>().OnClick.AddListener(TargetClicked);
        active_targets.Add(right_spawned_target);
        StartCoroutine(HandleTarget(right_spawned_target));
        if (is_flickering_together && is_flickering)
        {
            scoremanager.total_score += score_tobe_added;
        }
        else if (!is_flickering_together && is_flickering && isright)
        {
            scoremanager.total_score += score_tobe_added;
        }
    }


   void SpawnLeftTarget()
    {
        GeneratePositions();
        left_spawned_target = Instantiate(target_prefab, leftpos, Quaternion.identity);
        left_spawned_target.GetComponent<ClickableObject>().OnClick.AddListener(TargetClicked);
        active_targets.Add(left_spawned_target);
        StartCoroutine(HandleTarget(left_spawned_target));
        if (is_flickering_together && is_flickering)
        {
            scoremanager.total_score += score_tobe_added;
            print(scoremanager.total_score);
        }
        else if (!is_flickering_together && is_flickering && !isright)
        {
            scoremanager.total_score += score_tobe_added;
            print(scoremanager.total_score);
        }
    }

    public override void TargetClicked(GameObject clickedtarget)
    {
        if (!is_flickering)
        {
            scoremanager.user_score -= (score_tobe_added * 2);
            scoremanager.LoseALife();
        }

        if (activeComplexFunctionsSO.complexfunctionslevels.Count > 1)
        {
            if (!is_flickering_together)
            {
                if (clickedtarget == left_spawned_target && !isright)
                {
                    scoremanager.user_score += score_tobe_added * score_ratio;
                }
                else if (clickedtarget == right_spawned_target && isright)
                {
                    scoremanager.user_score += score_tobe_added * score_ratio;
                }
            }
            else
            {
                scoremanager.user_score += score_tobe_added * score_ratio;
            }
        }

        else
        {
            if (!is_flickering_together)
            {
                if (clickedtarget == left_spawned_target && !isright)
                {
                    scoremanager.user_score += score_tobe_added;
                }
                else if (clickedtarget == right_spawned_target && isright)
                {
                    scoremanager.user_score += score_tobe_added;
                }
            }
            else
            {
                scoremanager.user_score += score_tobe_added;
            }
        }
    }



    public void SetActiveComplexFunctionsSO(ComplexFunctionsSO val) => activeComplexFunctionsSO = val;


    IEnumerator SpawnTargets()
    {
        while (true)
        {
            SpawnRightTarget();
            SpawnLeftTarget();
            yield return new WaitForSeconds(Random.Range(min_delay_between_targets, max_delay_between_targets));
        }
    }

    IEnumerator HandleTarget(GameObject target)
    {
        if (target != null)
        {
            yield return new WaitForSeconds(target_life_span);
            Destroy(target);
        }
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            initial_timer -= Time.deltaTime;    

            for (int i = 0; i < _starttime.Count; i++)
            {
                if (timer >= _starttime[i])
                {
                    flickering_time = _flickeringtime[i];
                    min_flickering_cooldown = _minflickeringcooldown[i];
                    max_flickering_cooldown = _maxflickeringcooldown[i];
                    target_life_span = _targetlifespan[i];
                    min_delay_between_targets = _mindelaybetweentargets[i];
                    max_delay_between_targets = _maxdelaybetweentargets[i];
                    is_flickering_together = _isflickeringtogether[i];
                }
            }

            if (!istimeless)
            {
                if (initial_timer <= 0 && chosen_mode != GameMode.GeneralEval.ToString())
                {
                    GameEnded.Invoke();
                }

                else if (initial_timer <= 0 && chosen_mode == GameMode.GeneralEval.ToString())
                {
                    SequenceEnd.Invoke();
                }
            }
            else
            {
                if (scoremanager.lives <= 0)
                {
                    GameEnded.Invoke();
                }
            }

            yield return null;
        }
    }

    

    IEnumerator StartFlickering()
    {
        while (true)
        {
            
            if (!is_flickering_together)
            {
                isright = Random.value < 0.5f;
            }

            is_flickering = true;

            Coroutine flickerRoutine = StartCoroutine(Flicker());

            yield return new WaitForSeconds(flickering_time);

            StopCoroutine(flickerRoutine);

            is_flickering = false;

           
            left_central_point_renderer.enabled = true;
            right_central_point_renderer.enabled = true;

            yield return new WaitForSeconds(Random.Range(min_flickering_cooldown, max_flickering_cooldown));
        }
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            if (!is_flickering_together)
            {
             
                if (isright)
                {
                    right_central_point_renderer.enabled = !right_central_point_renderer.enabled;
                }
                else
                {
                    left_central_point_renderer.enabled = !left_central_point_renderer.enabled;
                }
            }
            else
            {
              
                bool state = !left_central_point_renderer.enabled;
                left_central_point_renderer.enabled = state;
                right_central_point_renderer.enabled = state;
            }

            yield return new WaitForSeconds(flicker_speed);
        }
    }


}
