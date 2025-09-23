using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;


public class SpacingManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private FlickeringManager flickering_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private GridManager grid_manager;
    [SerializeField] private UIManager ui_manager;

    [Header("Game Settings")]
    [SerializeField] private float grid_delay;
    [SerializeField] private float score_tobe_added;
    [SerializeField] private Color clicked_color;
    [SerializeField] UnityEvent GameEnded;


    private SpacingSO activeSpacingSO;
    private float timer;
    private float lifespan;
    private float nextGridSpawnTime;
    private float score_ratio;
    private int row;
    private int column;
    private int streak;
    private int activeLevelIndex = 0;
    private bool isWaitingAfterClick = false;
    private List<float> starttime = new List<float>();
    private List<float> life_span = new List<float>();
    private List<float> flickeringspeed = new List<float>();
    private List<float> _scoreratio = new List<float>();
    private List<bool> isflickering = new List<bool>();
    private List<int> rows = new List<int>();
    private List<int> columns = new List<int>();

    private void Start()
    {
        GameSetup();
        StartCoroutine(ui_manager.Timer());
        StartCoroutine(GameLoop());
        grid_manager.GenerateGrid(row, column);
        StartCoroutine(GenerateGrid());
        StartCoroutine(flickering_manager.Flickering());
    }

    void GameSetup()
    {
        initial_timer = activeSpacingSO.timer;

       
       
        for (int i = 0; i < activeSpacingSO.spacinglevels.Count; i++)
        {
            starttime.Add(activeSpacingSO.spacinglevels[i].starttime);
            life_span.Add(activeSpacingSO.spacinglevels[i].speed);
            rows.Add(activeSpacingSO.spacinglevels[i].rows);
            columns.Add(activeSpacingSO.spacinglevels[i].columns);
            isflickering.Add(activeSpacingSO.spacinglevels[i].isflickering);
            flickeringspeed.Add(activeSpacingSO.spacinglevels[i].flickeringspeed);
            _scoreratio.Add(activeSpacingSO.spacinglevels[i].scoreratio);
        }

        lifespan = life_span[activeLevelIndex];

       
        if (life_span.Count > 0) lifespan = life_span[0];
        if (rows.Count > 0) row = rows[0];
        if (columns.Count > 0) column = columns[0];
    }



   


    public override void TargetClicked(GameObject good_tile)
    {
        good_tile.GetComponent<SpriteRenderer>().color = clicked_color;
        good_tile.GetComponent<CircleCollider2D>().enabled = false;
        score_manager.user_score += score_tobe_added * score_ratio;
        streak++;
        if (streak >= 2)
        {
            streak = 0;
            StartCoroutine(WaitBeforeNewGrid());
        }
    }




    public void GameEnd()
    {
        grid_manager.ClearGrid();
        StopAllCoroutines();
    }


    public void SetActiveSpacingSO(SpacingSO val) => activeSpacingSO = val;

    private IEnumerator WaitBeforeNewGrid()
    {
        isWaitingAfterClick = true;
        yield return new WaitForSeconds(0.3f); 
        nextGridSpawnTime = Time.time; 
        isWaitingAfterClick = false;
    }


    IEnumerator GenerateGrid()
    {
       
        nextGridSpawnTime = Time.time ;

        while (true)
        {
            if (Time.time >= nextGridSpawnTime && !isWaitingAfterClick)
            {
                grid_manager.ClearGrid();

                if (grid_delay > 0)
                {
                    yield return new WaitForSeconds(grid_delay);
                }

                grid_manager.GenerateGrid(row, column);
                score_manager.total_score += (score_tobe_added * 2) * score_ratio;
                print(score_manager.total_score);
                streak = 0;
                nextGridSpawnTime = Time.time + lifespan;
            }

            yield return null;
        }
    }






    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            initial_timer -= Time.deltaTime;
           
            for (int i = 0; i < starttime.Count; i++)
            {
                if (timer >= starttime[i])
                {
                    lifespan = life_span[i];
                    activeLevelIndex = i;
                    row = rows[i];
                    column = columns[i];
                    flickering_manager.flickeringspeed = flickeringspeed[i];
                    flickering_manager.isflickering = isflickering[i];
                    score_ratio = _scoreratio[i];
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