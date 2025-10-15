using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;

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

    public ComplexFunctionsSO activeComplexFunctionsSO;
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
    private List<GameObject> active_left_targets = new List<GameObject>();
    private List<GameObject> active_right_targets = new List<GameObject>();
    private Queue<GameObject> pooling = new Queue<GameObject>();

    void Start()
    {
        StartCoroutine(GameInit());
    }

    void SetupScreen()
    {

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

        chosen_mode = PlayerPrefs.GetString("GameMode");

        timer = 0;

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


    void InitializePool()
    {
        for (int i = 0; i < 40; i++)
        {
            GameObject obj = Instantiate(target_prefab);
            obj.SetActive(false);
            obj.GetComponent<ClickableObject>().OnClick.AddListener(TargetClicked);
            pooling.Enqueue(obj);
        }
    } 


    void ClearTargets()
    {
        foreach (var t in pooling)
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

        // Left side
        int tries = 0;
        do
        {
            leftpos = new Vector2(Random.Range(minX, midX - 0.5f), Random.Range(minY, maxY));
            Vector2 leftDir = (leftpos - (Vector2)left_central_point.transform.position).normalized;
            float leftDist = Vector2.Distance(leftpos, left_central_point.transform.position);
            if (leftDist < leftRadius)
                leftpos = (Vector2)left_central_point.transform.position + leftDir * leftRadius;

            tries++;
            if (tries > 15) break; // safety stop
        } while (IsOverlapping(leftpos, false));

        // Right side
        tries = 0;
        do
        {
            rightpos = new Vector2(Random.Range(midX + 0.5f, maxX), Random.Range(minY, maxY));
            Vector2 rightDir = (rightpos - (Vector2)right_central_point.transform.position).normalized;
            float rightDist = Vector2.Distance(rightpos, right_central_point.transform.position);
            if (rightDist < rightRadius)
                rightpos = (Vector2)right_central_point.transform.position + rightDir * rightRadius;

            tries++;
            if (tries > 15) break; // safety stop
        } while (IsOverlapping(rightpos, true));
    }



    void SpawnRightTarget()
    {
        GeneratePositions();
        right_spawned_target = pooling.Dequeue();
        right_spawned_target.transform.position = rightpos;

        var t = right_spawned_target.GetComponent<ComplexTargetValidity>();
        t.isvalid = is_flickering && (!is_flickering_together ? isright : true);
        t.isright = true;

        right_spawned_target.SetActive(true);

        active_right_targets.Add(right_spawned_target);


        if (is_flickering)
        {
            if (is_flickering_together || (!is_flickering_together && isright))
            {
                scoremanager.total_score += score_tobe_added;
            }
        }
        StartCoroutine(HandleTarget(right_spawned_target));
    }



    void SpawnLeftTarget()
    {
        GeneratePositions();
        left_spawned_target = pooling.Dequeue();
        left_spawned_target.transform.position = leftpos;

        var t = left_spawned_target.GetComponent<ComplexTargetValidity>();
        t.isvalid = is_flickering && (!is_flickering_together ? !isright : true);
        t.isright = false;

        left_spawned_target.SetActive(true);

        active_left_targets.Add(left_spawned_target);


        if (is_flickering)
        {
            if (is_flickering_together || (!is_flickering_together && !isright))
            {
                scoremanager.total_score += score_tobe_added;
            }
        }

        StartCoroutine(HandleTarget(left_spawned_target));
    }


    public override void TargetClicked(GameObject clickedtarget)
    {
        if (activeComplexFunctionsSO.complexfunctionslevels.Count > 1)
        {
            if (is_flickering_together && is_flickering)
            {
                scoremanager.user_score += score_tobe_added * score_ratio;
            }
            else if (!is_flickering_together && is_flickering && !isright)
            {
                if (!clickedtarget.GetComponent<ComplexTargetValidity>().isright)
                {
                    scoremanager.user_score += score_tobe_added * score_ratio;
                }
                else
                {
                    scoremanager.user_score -= score_tobe_added * score_ratio;
                    scoremanager.LoseALife();
                }
                    
            }
            else if (!is_flickering_together && is_flickering && isright)
            {
                if (clickedtarget.GetComponent<ComplexTargetValidity>().isright)
                {
                    scoremanager.user_score += score_tobe_added * score_ratio;
                }
                else
                {
                    scoremanager.user_score -= score_tobe_added * score_ratio;
                    scoremanager.LoseALife();
                }
                    
            }
            else
            {
                scoremanager.user_score -= score_tobe_added * score_ratio;
                scoremanager.LoseALife();
            }
        }
        else
        {
            if (is_flickering_together && is_flickering)
            {
                scoremanager.user_score += score_tobe_added;
            }
            else if (!is_flickering_together && is_flickering && !isright)
            {
                if (!clickedtarget.GetComponent<ComplexTargetValidity>().isright)
                {
                    scoremanager.user_score += score_tobe_added;
                }
                else
                {
                    scoremanager.user_score -= score_tobe_added;
                    scoremanager.LoseALife();
                }
                    
            }
            else if (!is_flickering_together && is_flickering && isright)
            {
                if (clickedtarget.GetComponent<ComplexTargetValidity>().isright)
                {
                    scoremanager.user_score += score_tobe_added;
                }
                else
                {
                    scoremanager.user_score -= score_tobe_added;
                    scoremanager.LoseALife();
                }
                    
            }
            else
            {
                scoremanager.user_score -= score_tobe_added;
                scoremanager.LoseALife();
            }
        }

        if (clickedtarget.GetComponent<ComplexTargetValidity>().isright)
        {
            active_right_targets.Remove(clickedtarget);
        }
        else
            active_left_targets.Remove(clickedtarget);

        clickedtarget.SetActive(false);
        pooling.Enqueue(clickedtarget);
    }



    public void SetActiveComplexFunctionsSO(ComplexFunctionsSO val) => activeComplexFunctionsSO = val;


    bool IsOverlapping(Vector2 newPos, bool isRight)
    {
        float minDistance = 1.5f; // adjust based on your target size

        // choose which side's list to compare against
        List<GameObject> list = isRight ? active_right_targets : active_left_targets;

        foreach (var t in list)
        {
            if (t == null || !t.activeSelf) continue;
            float distance = Vector2.Distance(newPos, t.transform.position);
            if (distance < minDistance)
                return true; // too close, overlapping
        }

        return false;
    }



    IEnumerator GameInit()
    {
        yield return null;

        SetupScreen();
        GameSetup();
        backgroundgenerator.GenerateConstantBackGround(0.5f);
        InitializePool();
        if (chosen_mode == GameMode.Timeless.ToString())
        {
            StartCoroutine(uimanager.Lives());
        }
        else
        {
            StartCoroutine(uimanager.Timer());
        }
        StartCoroutine(GameLoop());
        StartCoroutine(StartFlickering());
        StartCoroutine(SpawnTargets());
    }


    IEnumerator SpawnTargets()
    {
        while (true)
        {
            GeneratePositions();
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
            target.SetActive(false);

            if (target.GetComponent<ComplexTargetValidity>().isright)
            {
                active_right_targets.Remove(target);
            }
            else
                active_left_targets.Remove(target);

            pooling.Enqueue(target);
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
                    score_ratio = _scoreratio[i];
                }
            }

            if (chosen_mode != GameMode.Timeless.ToString())
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
                isright = Random.value < 0.5f;


            Coroutine flickerRoutine = StartCoroutine(Flicker());

            if (isright)
            {
                scoremanager.total_score += active_right_targets.Count;
            }
            else if (!isright)
            {
                scoremanager.total_score += active_left_targets.Count;
            }

            is_flickering = true;
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
