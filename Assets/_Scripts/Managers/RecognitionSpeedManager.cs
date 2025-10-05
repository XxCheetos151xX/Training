using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public class RecognitionSpeedManager : AbstractGameManager
{
    [Header("Game Refrences")]
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private ScoreManager scoremanager;
    [SerializeField] private FlickeringManager flickermanager;
    [SerializeField] private UIManager uimanager;
    [SerializeField] private BackgroundGenerator backgroundgenerator;

    [Header("Game Settings")]
    [SerializeField] private float delay;
    [SerializeField] private float min_score;
    [SerializeField] private float max_score;
    [SerializeField] private Color normal_color;
    [SerializeField] private Color invisible_color;
    [SerializeField] private Color clicked_color;
    [SerializeField] private UnityEvent GameEnded;
    [SerializeField] private UnityEvent SequenceEnd;

    private RecognitionSpeedSO activeRecognitionSpeedSO;
    private Camera cam;
    private GameObject spawned_target;
    private SpriteRenderer spawned_target_renderer;
    private float minX, minY,  maxX, maxY;
    private float timer;
    private float life_span;
    private float speed;
    private float score_ratio;
    private int size;
    private bool all_targets_clicked;
    private string chosen_mode;
    private List<float> _starttime = new List<float>();
    private List<float> _lifespan = new List<float>();
    private List<float> _speed = new List<float>();
    private List<float> _flickerinspeed = new List<float>();
    private List<float> _scoreratio = new List<float>();
    private List<int> _size = new List<int>();
    private List<bool> _isflickering = new List<bool>();
    private List<GameObject> active_targets = new List<GameObject>();

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
        minY = cam.transform.position.y - halfHeight;
        minX = cam.transform.position.x - halfWidth;
        maxX = cam.transform.position.x + halfWidth;
        maxY = cam.transform.position.y + halfHeight;
    }


    void GameSetup()
    {
        timer = 0;

        chosen_mode = PlayerPrefs.GetString("GameMode");

        initial_timer = activeRecognitionSpeedSO.timer;

        all_targets_clicked = false;

        for (int i = 0; i < activeRecognitionSpeedSO.recognitionspeedlevels.Count; i++)
        {
            _starttime.Add(activeRecognitionSpeedSO.recognitionspeedlevels[i].starttime);
            _lifespan.Add(activeRecognitionSpeedSO.recognitionspeedlevels[i].lifespan);
            _speed.Add(activeRecognitionSpeedSO.recognitionspeedlevels[i].speed);
            _size.Add(activeRecognitionSpeedSO.recognitionspeedlevels[i].size);
            _flickerinspeed.Add(activeRecognitionSpeedSO.recognitionspeedlevels[i].flickeringspeed);
            _isflickering.Add(activeRecognitionSpeedSO.recognitionspeedlevels[i].isflickering);
            _scoreratio.Add(activeRecognitionSpeedSO.recognitionspeedlevels[i].scoreratio);
        }
    }


    void SpawnSingleTarget()
    {
        float posx = Random.Range(minX + 0.5f, maxX - 0.5f);
        float posy = Random.Range(minY + 0.5f, maxY - 0.5f);

        spawned_target = Instantiate(target_prefab, new Vector2(posx, posy), Quaternion.identity);
        spawned_target.GetComponent<CircleCollider2D>().enabled = false;
        spawned_target_renderer = spawned_target.GetComponent<SpriteRenderer>();
        spawned_target_renderer.color = normal_color;
        active_targets.Add(spawned_target);
        spawned_target.GetComponent<ClickableObject>().OnClick.AddListener(TargetClicked);
        
        scoremanager.total_score += max_score;
    }

    void DestroyAtciveTargets()
    {
        foreach (var t in active_targets)
        {
            Destroy(t);
        }
        active_targets.Clear();
    }

    public override void TargetClicked(GameObject t)
    {
        int pressed_targets = 0;

        t.GetComponent<CircleCollider2D>().enabled = false;
        
        Vector2 targetCenter = t.transform.position;

        Vector2 clickPos = ClickManager.LastClickWorldPos;

        float dist = Vector2.Distance(clickPos, targetCenter);

        GameObject circle = t.transform.GetChild(0).gameObject;

        circle.transform.position = clickPos;

        circle.SetActive(true);
         
        float tolerance = 0.1f; 

        if (activeRecognitionSpeedSO.recognitionspeedlevels.Count > 1)
        {
            if (dist <= tolerance)
            {
                scoremanager.user_score += max_score * score_ratio;
            }
            else
            {
                scoremanager.user_score += min_score * score_ratio;
            }
        }

        else
        {
            if (dist <= tolerance)
            {
                scoremanager.user_score += max_score;
            }
            else
            {
                scoremanager.user_score += min_score;
            }
        }



        t.GetComponent<SpriteRenderer>().color = clicked_color;
        foreach (var target in active_targets)
        {
            if (target.GetComponent<SpriteRenderer>().color == clicked_color)
            {
                pressed_targets++;
            }
        }
        if (pressed_targets == size)
        {
            all_targets_clicked = true;
            pressed_targets = 0;
        }
    }


    public void GameEnd()
    {
        StopAllCoroutines();
        DestroyAtciveTargets();
    }

    public void ContinueSequence()
    {
        chosen_mode = PlayerPrefs.GetString("GameMode", "None");
        if (chosen_mode == GameMode.GeneralEval.ToString() && SequenceManager.Instance != null)
        {
            SequenceManager.Instance.LoadNextScene();
        }
    }



    public void SetActiveRecognitionSpeedSO(RecognitionSpeedSO val) => activeRecognitionSpeedSO = val;


    IEnumerator GameInit()
    {
        yield return null;

        SetupScreen();
        backgroundgenerator.GenerateConstantBackGround(0.5f);
        GameSetup();
        StartCoroutine(uimanager.Timer());
        StartCoroutine(GameLoop());
        StartCoroutine(flickermanager.Flickering());
        StartCoroutine(SpawnTargets());
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
                    life_span = _lifespan[i];
                    speed = _speed[i];
                    size = _size[i];
                    flickermanager.flickeringspeed = _flickerinspeed[i];
                    flickermanager.isflickering = _isflickering[i]; 
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

    IEnumerator SpawnTargets()
    {
        while (true)
        {
            for (int i = 0; i < size; i++)
            {
                SpawnSingleTarget();
                yield return new WaitForSeconds(speed);
                spawned_target_renderer.color = invisible_color;
            }
            foreach (var t in active_targets)
            {
                t.GetComponent<CircleCollider2D>().enabled = true;
            }

            float elapsed = 0f;
            while (elapsed < life_span && !all_targets_clicked)
            {
                elapsed += Time.deltaTime;
                yield return null; 
            }

            if (all_targets_clicked)
            {
                yield return new WaitForSeconds(delay);
                all_targets_clicked = false;
            }
            else
            {
                scoremanager.LoseALife();
            }
            DestroyAtciveTargets();


        }
    }
}
