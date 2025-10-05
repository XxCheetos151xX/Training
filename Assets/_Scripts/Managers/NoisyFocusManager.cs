using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class NoisyFocusManager : AbstractGameManager
{
    [Header("Game Refrences")]
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private CircleCollider2D inverted_collider;
    [SerializeField] private ScoreManager scoremanager;
    [SerializeField] private FlickeringManager flickeringmanager;
    [SerializeField] private UIManager uimanager;
    [SerializeField] private BackgroundGenerator backgroundgenerator;

    [Header("Game Settings")]
    [SerializeField] private float score_tobe_added;
    [SerializeField] private float inverted_collider_pos;
    [SerializeField] private UnityEvent GameEnded;
    [SerializeField] private UnityEvent SequenceEnd;

    [HideInInspector] public float spawned_target_speed;

    private NoisyFocusSO activeNoisyFocusSO;
    private Camera cam;
    private GameObject spawned_target;
    private float minX, maxX, minY, maxY;
    private float delay;
    private float min_speed;
    private float max_speed;
    private float timer;
    private float score_ratio;
    private string chosen_mode;
    private List<float> _starttime = new List<float>();
    private List<float> _delay = new List<float>();
    private List<float> _minspeed = new List<float>();
    private List<float> _maxspeed = new List<float>(); 
    private List<float> _flickeringspeed = new List<float>();
    private List<float> _scoreratio = new List<float>();
    private List<bool> _isflickering = new List<bool>();
    private List<GameObject> active_targets = new List<GameObject>();

    void Start()
    {        
        ScreenSetup();
        backgroundgenerator.GenerateConstantBackGround(0.5f);
        GameSetup();
        StartCoroutine(uimanager.Timer());
        StartCoroutine(GameLoop());
        StartCoroutine(flickeringmanager.Flickering());
        StartCoroutine(SpawnTargets());
    }

    
    void ScreenSetup()
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


    void SpawnSingleTraget()
    {
        float posy = Random.Range(minY + 0.5f, maxY - 0.5f);
        float posx;
        spawned_target_speed = Random.Range(min_speed, max_speed);

        bool isleft = Random.value > 0.5f;
        
        if (isleft)
        {
            posx = minX + 0.5f;
            inverted_collider.offset = new Vector2(-inverted_collider_pos, 0);
        }

        else
        {
            posx = maxX - 0.5f;
            inverted_collider.offset = new Vector2(inverted_collider_pos, 0);
        }

        spawned_target = Instantiate(target_prefab, new Vector3(posx, posy, 0), Quaternion.identity);
        spawned_target.GetComponent<ClickableObject>()._Onclick.AddListener(TargetClicked);
        active_targets.Add(spawned_target);
        scoremanager.total_score += score_tobe_added;
    }



    public void TargetClicked()
    {
        if (activeNoisyFocusSO.noisyfocuslevels.Count > 1)
        {
            scoremanager.user_score += score_tobe_added * score_ratio;
        }
        else
            scoremanager.user_score += score_tobe_added;
    }

    void GameSetup()
    {
        timer = 0;

        chosen_mode = PlayerPrefs.GetString("GameMode");

        initial_timer = activeNoisyFocusSO.timer;

        for (int i = 0; i < activeNoisyFocusSO.noisyfocuslevels.Count; i++)
        {
            _starttime.Add(activeNoisyFocusSO.noisyfocuslevels[i].starttime);
            _delay.Add(activeNoisyFocusSO.noisyfocuslevels[i].delay);
            _minspeed.Add(activeNoisyFocusSO.noisyfocuslevels[i].minSpeed);
            _maxspeed.Add(activeNoisyFocusSO.noisyfocuslevels[i].maxSpeed);
            _flickeringspeed.Add(activeNoisyFocusSO.noisyfocuslevels[i].flickeringspeed);
            _isflickering.Add(activeNoisyFocusSO.noisyfocuslevels[i].isflickering);
            _scoreratio.Add(activeNoisyFocusSO.noisyfocuslevels[i].scoreratio);
        }
    }


    public void GameEnd()
    {
        StopAllCoroutines();
        foreach (var t in active_targets)
        {
            Destroy(t);
        }
    }

    public void ContinueSequence()
    {
        chosen_mode = PlayerPrefs.GetString("GameMode", "None");
        if (chosen_mode == GameMode.GeneralEval.ToString() && SequenceManager.Instance != null)
        {
            SequenceManager.Instance.LoadNextScene();
        }
    }


    public void SetActiveNoisyFocusSO(NoisyFocusSO val) => activeNoisyFocusSO = val;

    IEnumerator SpawnTargets()
    {
        while (true)
        {
            SpawnSingleTraget();
            yield return new WaitForSeconds(delay);
        }
    }

    //IEnumerator TargetBehaviour(GameObject target, float speed, bool isLeft)
    //{
    //    Vector3 destination;

    //    if (isLeft)
    //        destination = new Vector3(maxX, target.transform.position.y, 0); 
    //    else
    //        destination = new Vector3(minX, target.transform.position.y, 0);  

    //    while (target != null)
    //    {
    //        target.transform.position = Vector3.MoveTowards(
    //            target.transform.position,
    //            destination,
    //            speed * Time.deltaTime
    //        );

    //        float screenCenterX = cam.transform.position.x;

            

            
    //        if (isLeft && target.transform.position.x >= screenCenterX)
    //        {
    //            Destroy(target);
    //            scoremanager.misses++;
    //            active_targets.Remove(target);
    //            yield break;
    //        }

            
    //        if (!isLeft && target.transform.position.x <= screenCenterX)
    //        {
    //            Destroy(target);
    //            scoremanager.misses++;
    //            active_targets.Remove(target);
    //            yield break;
    //        }
    //        yield return null;
    //    }
    //}



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
                    delay = _delay[i];
                    min_speed = _minspeed[i];
                    max_speed = _maxspeed[i];
                    flickeringmanager.flickeringspeed = _flickeringspeed[i];
                    flickeringmanager.isflickering = _isflickering[i];
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
}
