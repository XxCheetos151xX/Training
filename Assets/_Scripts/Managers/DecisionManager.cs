using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class DecisionManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private GameObject false_target_prefab;
    [SerializeField] private GameObject left_hand;
    [SerializeField] private GameObject right_hand;
    [SerializeField] private GameObject end_screen;
    [SerializeField] private FlickeringManager flickering_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private UIManager ui_manager;
    [SerializeField] private BackgroundGenerator background_generator;

    [Header("Game Settings")]
    [SerializeField] private float delay;
    [SerializeField] private float button_size = 1;
    [SerializeField] private float target_size = 1;
    [SerializeField] private List<Color> colors = new List<Color>();
    [SerializeField] private UnityEvent GameEnd;

    private DecisionSO activeDecisionSO;
    private Camera cam;
    private float timer;
    private float colorchangetime;
    private float minY, minX, maxX, maxY;
    private float not_todo_prob;
    private int index1;
    private int index2;
    private bool first_switch;
    private bool lefthandpressed = false;
    private bool righthandpressed = false;
    private bool gamestarted = false;
    private bool switch_colors;
    private GameObject spawned_target;
    private GameObject spawned_false_target;
    private ClickableObject target_clickableobject;
    private CircleCollider2D target_collider;
    private SpriteRenderer lefthand_renderer;
    private SpriteRenderer righthand_renderer;
    private SpriteRenderer spawnedTargetRenderer;
    private SpriteRenderer targetFollowHand;
    private List<float> start_time = new List<float>();
    private List<float> color_change_time = new List<float>();
    private List<float> flickering_speed = new List<float>();
    private List<float> _not_todo_prob = new List<float>();
    private List<bool> _isflickering = new List<bool>();
    private List<bool> _switch_colors = new List<bool>();

    private void Start()
    {
        GameSetup();
        background_generator.GenerateConstantBackGround(0.5f);

    }

    void GameSetup()
    {
        initial_timer = activeDecisionSO.timer;
        cam = Camera.main;
        timer = 0;
        first_switch = true;

        lefthand_renderer = left_hand.GetComponent<SpriteRenderer>();
        righthand_renderer = right_hand.GetComponent<SpriteRenderer>();

        left_hand.transform.localScale = new Vector3(button_size, button_size, button_size);
        right_hand.transform.localScale = new Vector3(button_size, button_size, button_size);

        do
        {
            index1 = Random.Range(0, colors.Count);
            index2 = Random.Range(0, colors.Count);
        } while (index1 == index2);

        lefthand_renderer.color = colors[index1];
        righthand_renderer.color = colors[index2];


        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = cam.transform.position.y - halfHeight;
        minX = cam.transform.position.x - halfWidth;
        maxX = cam.transform.position.x + halfWidth;
        maxY = cam.transform.position.y + halfHeight;



        for (int i = 0; i < activeDecisionSO.decisionlevels.Count; i++)
        {
            start_time.Add(activeDecisionSO.decisionlevels[i].starttime);
            color_change_time.Add(activeDecisionSO.decisionlevels[i].color_changetimer);
            _switch_colors.Add(activeDecisionSO.decisionlevels[i].switchcolors);
            _not_todo_prob.Add(activeDecisionSO.decisionlevels[i].nottodo_prob);
            flickering_speed.Add(activeDecisionSO.decisionlevels[i].flickerspeed);
            _isflickering.Add(activeDecisionSO.decisionlevels[i].isflickering);
        }
    }


    public void SwitchColor()
    {
        if (switch_colors || first_switch)
        {
            do
            {
                index1 = Random.Range(0, colors.Count);
                index2 = Random.Range(0, colors.Count);
            } while (index1 == index2);

            lefthand_renderer.color = colors[index1];
            righthand_renderer.color = colors[index2];

            first_switch = false;
        }
    }


    public void LeftHandPressed()
    {
        lefthandpressed = true;
        CheckGameStarted();
    }

    public void LeftHandReleasd()
    {
        lefthandpressed = false;
    }

    public void RightHandPressed()
    {
        righthandpressed = true;
        CheckGameStarted();
    }

    public void RightHandReleased()
    {
        righthandpressed = false;
    }


    void CheckGameStarted()
    {
        if (!gamestarted && righthandpressed && lefthandpressed)
        {
           
            StartCoroutine(flickering_manager.Flickering());
            SwitchColor();
            StartCoroutine(ui_manager.Timer());
            StartCoroutine(GameLoop());
            StartCoroutine(SpawnTargets());
            gamestarted = true;
        }
    }


    void TargetCaptured()
    {
        score_manager.user_score++;
        SwitchColor();
    }

    void FalseTargetCaptured()
    {
        score_manager.misses++;
    }


    public void GameEnded()
    {
        StopAllCoroutines();

        if (spawned_target != null)
        {
            Destroy(spawned_target);
        }
        if (spawned_false_target != null)
        {
            Destroy(spawned_false_target);
        }
    }

    public void SetActiveDecisionSO(DecisionSO val) => activeDecisionSO = val;


    IEnumerator SpawnTargets()
    {
        while (true)
        {
        StartNextSpawn:

            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY + 3.5f, maxY - 2.5f);
            bool spawn_wrong_target = Random.value > ((11 - not_todo_prob) / 10);

            if (spawn_wrong_target)
            {
                spawned_false_target = Instantiate(false_target_prefab, new Vector3(x, y, 0), Quaternion.identity);
                spawned_false_target.GetComponent<ClickableObject>()._Onclick.AddListener(FalseTargetCaptured);

                float elapsed = 0f;
                while (elapsed < colorchangetime)
                {
                    if (spawned_false_target == null)
                        goto StartNextSpawn;

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (spawned_false_target != null)
                {
                    Destroy(spawned_false_target);
                    score_manager.misses++;
                    SwitchColor();
                }

                continue;
            }
            else
            {
                bool useLeft = Random.value > 0.5f;
                targetFollowHand = useLeft ? lefthand_renderer : righthand_renderer;

                spawned_target = Instantiate(target_prefab, new Vector3(x, y, 0), Quaternion.identity);
                spawned_target.transform.localScale = new Vector3(target_size, target_size, target_size);
                spawnedTargetRenderer = spawned_target.GetComponent<SpriteRenderer>();
                target_clickableobject = spawned_target.GetComponent<ClickableObject>();
                target_collider = spawned_target.GetComponent<CircleCollider2D>();
                target_clickableobject._Onclick.AddListener(TargetCaptured);
                spawnedTargetRenderer.color = targetFollowHand.color;
                score_manager.total_score++;

                float elapsed = 0f;
                while (elapsed < colorchangetime)
                {
                    if (spawned_target == null)
                        goto StartNextSpawn;

                    if (targetFollowHand != null)
                        spawnedTargetRenderer.color = targetFollowHand.color;

                    if (spawned_target != null)
                    {
                        bool exactlyOneHandPressed = lefthandpressed ^ righthandpressed;
                        bool correctHandPressed = (useLeft && righthandpressed) || (!useLeft && lefthandpressed);

                        if (target_collider != null)
                            target_collider.enabled = exactlyOneHandPressed && correctHandPressed;
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (spawned_target != null)
                {
                    Destroy(spawned_target);
                    score_manager.misses++;
                    SwitchColor();
                }

                continue;
            }
        }
    }





    IEnumerator GameLoop()
    {
        while (timer != activeDecisionSO.timer)
        {
            timer += Time.deltaTime;

            initial_timer -= Time.deltaTime;

            for (int i = 0; i < start_time.Count; i++)
            {
                if (timer >= start_time[i])
                {
                    colorchangetime = color_change_time[i];
                    flickering_manager.isflickering = _isflickering[i];
                    switch_colors = _switch_colors[i];
                    flickering_manager.flickeringspeed = flickering_speed[i];
                    not_todo_prob = _not_todo_prob[i];
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


