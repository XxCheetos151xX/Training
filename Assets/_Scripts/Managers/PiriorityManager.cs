using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class PiriorityManager : AbstractGameManager
{
    [Header("Game Refrences")]
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private ScoreManager scoremanager;
    [SerializeField] private FlickeringManager flickeringmanager;
    [SerializeField] private UIManager uimanager;
    [SerializeField] private BackgroundGenerator backgroundgenerator;

    [Header("Game Settings")]
    [SerializeField] private Color first_color; 
    [SerializeField] private Color second_color; 
    [SerializeField] private Color third_color; 
    [SerializeField] private Color fourth_color;
    [SerializeField] private float first_color_score;
    [SerializeField] private float second_color_score;
    [SerializeField] private float third_color_score;
    [SerializeField] private float fourth_color_score;
    [SerializeField] private float score_tobe_added_each_spawn;
    [SerializeField] private float minimum_distance_between_targets;
    [SerializeField] private UnityEvent GameEnded;

    private PirioritySO activePirioritySO;
    private Camera cam;
    private GameObject spawned_target;
    private SpriteRenderer spawned_target_renderer;
    private float minX, maxX, minY, maxY;
    private float timer;
    private float life_span;
    private float delay;
    private bool valid_pos;
    private List<float> _starttime = new List<float>();
    private List<float> _lifespan = new List<float>();
    private List<float> _delay = new List<float>();
    private List<float> _flickeringspeed = new List<float>();
    private List<bool> _isflickering = new List<bool>();
    private List<Vector3> active_pos = new List<Vector3>();
    private List<GameObject> active_targets = new List<GameObject>();

    void Start()
    {
        SetupScreen();
        backgroundgenerator.GenerateConstantBackGround(0.5f);
        GameSetup();
        StartCoroutine(uimanager.Timer());
        StartCoroutine(GameLoop());
        StartCoroutine(SpawnTargets());
        StartCoroutine(flickeringmanager.Flickering());
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

        initial_timer = activePirioritySO.timer;

        if (activePirioritySO != null)
        {
            for (int i = 0; i < activePirioritySO.pirioritylevels.Count; i++)
            {
                _starttime.Add(activePirioritySO.pirioritylevels[i].starttime);
                _lifespan.Add(activePirioritySO.pirioritylevels[i].targetlifetime);
                _delay.Add(activePirioritySO.pirioritylevels[i].delaybetweentargets);
                _flickeringspeed.Add(activePirioritySO.pirioritylevels[i].flickeringspeed);
                _isflickering.Add(activePirioritySO.pirioritylevels[i].isflickering);
            }
        }
    }

    void SpawnSingleTarget()
    {
        int attempts = 30;
        Vector3 pos = Vector3.zero;


        for (int i = 0; i < attempts; i++)
        {
            float posx = Random.Range(minX + 1, maxX - 1);
            float posy = Random.Range(minY + 1, maxY - 1);
            pos = new Vector3(posx, posy, 0);

            valid_pos = true;

            foreach (var p in active_pos)
            {
                if (Vector3.Distance(pos, p) < minimum_distance_between_targets)
                {
                    valid_pos = false;
                    break;
                }
            }

            if (valid_pos) break; 
        }

        if (valid_pos)
        {
            spawned_target = Instantiate(target_prefab, pos, Quaternion.identity);
            spawned_target_renderer = spawned_target.GetComponent<SpriteRenderer>();
            spawned_target.GetComponent<ClickableObject>().OnClick.AddListener(TargetClicked);

            scoremanager.total_score += score_tobe_added_each_spawn;
            active_pos.Add(pos);
            active_targets.Add(spawned_target);

            StartCoroutine(HandleTargetLifeCycle(spawned_target, spawned_target_renderer, life_span));
        }
    }



    public override void TargetClicked(GameObject t)
    {
        if (t.GetComponent<SpriteRenderer>().color == first_color)
        {
            scoremanager.user_score += first_color_score;
        } 
        
        else if (t.GetComponent<SpriteRenderer>().color == second_color)
        {
            scoremanager.user_score += second_color_score;
        } 
        
        else if (t.GetComponent<SpriteRenderer>().color == third_color)
        {
            scoremanager.user_score += third_color_score;
        }
        
        else if (t.GetComponent<SpriteRenderer>().color == fourth_color)
        {
            scoremanager.user_score += fourth_color_score;
        }

        active_pos.Remove(t.transform.position);
        active_targets.Remove(t);
    }

    public void GameEnd()
    {
        StopAllCoroutines();
        foreach (var target in active_targets)
        {
            Destroy(target);
        }
    }

    public void SetActivePirioritySO(PirioritySO val) => activePirioritySO = val;


    IEnumerator SpawnTargets()
    {
        while (true)
        {
            SpawnSingleTarget();
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator HandleTargetLifeCycle(GameObject target, SpriteRenderer target_renderer, float lifespan)
    {
        float elapsed = 0;
        float quarter = lifespan / 4;

        while (elapsed < lifespan)
        { 
            if (target_renderer != null)
            {
                if (elapsed < quarter)
                    target_renderer.color = first_color;
                else if (elapsed < quarter * 2)
                    target_renderer.color = second_color;
                else if (elapsed < quarter * 3)
                    target_renderer.color = third_color;
                else
                    target_renderer.color = fourth_color;

            }


            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target != null)
        {
            Destroy(target);
            active_pos.Remove(target.transform.position);
            active_targets.Remove(target);
            scoremanager.misses++;
        } 
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < _starttime.Count; i++)
            {
                if (timer >= _starttime[i])
                {
                    life_span = _lifespan[i];
                    delay = _delay[i];
                    flickeringmanager.flickeringspeed = _flickeringspeed[i];
                    flickeringmanager.isflickering = _isflickering[i];
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
