using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class FlickeringGameManager : AbstractGameManager
{
    [Header("Game Refrences")]
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private Transform left_goal;
    [SerializeField] private Transform right_goal;
    [SerializeField] private FlickeringManager flickeringmanager;
    [SerializeField] private ScoreManager scoremanager;

    [Header("Game Settings")]
    [SerializeField] private float score_tobe_added;
    [SerializeField] private float target_scale;
    [SerializeField] private float spawning_area_width;
    [SerializeField] private UnityEvent GameEnded;

    private FlickeringSO activeFlickeringSO;
    private Camera cam;
    private GameObject spawned_target;
    private float minX, maxX, minY, maxY;
    private float timer;
    private float min_speed;
    private float max_speed;
    private float delay;
    private float right_spawning_area_border;
    private float left_spawning_area_border;
    private List<float> _starttime = new List<float>();
    private List<float> _delay = new List<float>();
    private List<float> _minspeed = new List<float>();
    private List<float> _maxspeed = new List<float>();
    private List<float> _flickeringspeed = new List<float>();
    private List<bool> _isflickering = new List<bool>();
    private List<GameObject> active_targets = new List<GameObject>();

    void Start()
    {
        SetupScreen();
        GameSetup();
        StartCoroutine(GameLoop());
        StartCoroutine(flickeringmanager.Flickering());
        StartCoroutine(SpawnTargets());
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

        initial_timer = activeFlickeringSO.timer;

        right_goal.position = new Vector3(maxX - 1f, 0, 0);
        left_goal.position = new Vector3(minX + 1f, 0, 0);

        right_spawning_area_border = spawning_area_width / 2;
        left_spawning_area_border = -spawning_area_width / 2;

        for (int i = 0; i < activeFlickeringSO.flickeringlevels.Count; i++)
        {
            _starttime.Add(activeFlickeringSO.flickeringlevels[i].starttime);
            _delay.Add(activeFlickeringSO.flickeringlevels[i].delay);
            _minspeed.Add(activeFlickeringSO.flickeringlevels[i].minspeed);
            _maxspeed.Add(activeFlickeringSO.flickeringlevels[i].maxspeed);
            _flickeringspeed.Add(activeFlickeringSO.flickeringlevels[i].flickeringspeed);
            _isflickering.Add(activeFlickeringSO.flickeringlevels[i].isflickeing);
        }
    }

    void SpawnSingleTarget()
    {
        float posx = Random.Range(left_spawning_area_border, right_spawning_area_border);
        float posy = Random.Range(minY + 0.5f, maxY - 0.5f);

        bool isleft = Random.value > 0.5f;

        float speed = Random.Range(min_speed, max_speed);


        spawned_target = Instantiate(target_prefab, new Vector2(posx, posy), Quaternion.identity);
        spawned_target.transform.localScale = new Vector3(target_scale, target_scale, target_scale);
        spawned_target.GetComponent<ClickableObject>()._Onclick.AddListener(TargetClicked);

        scoremanager.total_score += score_tobe_added;

        active_targets.Add(spawned_target);

        StartCoroutine(TargetBehaviour(spawned_target, speed, isleft));
    }


    void TargetClicked()
    {
        scoremanager.user_score += score_tobe_added;
    }

    void ClearActiveTargets()
    {
        foreach (var target in active_targets)
        {
            Destroy(target);
        }
    }

    public void GameEnd()
    {
        StopAllCoroutines();
        ClearActiveTargets();
    }

    public void SetActiveFlickeringSO(FlickeringSO val) => activeFlickeringSO = val;

    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < _starttime.Count; i++)
            {
                if (timer >= _starttime[i])
                {
                    delay = _delay[i];
                    min_speed = _minspeed[i];
                    max_speed = _maxspeed[i];
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


    IEnumerator SpawnTargets()
    {
        while (true)
        {
            SpawnSingleTarget();
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator TargetBehaviour(GameObject target, float speed, bool isleft)
    {
        Vector3 destination;

        if (isleft)
            destination = left_goal.transform.position;
        else
            destination = right_goal.transform.position;

        while (target != null)
        {
            target.transform.position = Vector3.MoveTowards(target.transform.position, destination, speed * Time.deltaTime);

            if (Vector3.Distance(target.transform.position, destination) <= 0.01)
            {
                Destroy(target);
                scoremanager.misses++;
                yield break;
            }

            yield return null;
        }
    }
}
