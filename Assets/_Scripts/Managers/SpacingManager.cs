using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpacingManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private GameObject bad_tile_prefab;
    [SerializeField] private GameObject good_tile_prefab;
    [SerializeField] private float grid_delay;
    [SerializeField] private FlickeringManager flickering_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] UnityEvent GameEnded;


    private SpacingSO activeSpacingSO;
    private float minY, minX, maxX, maxY;
    private float timer;
    private float lifespan;
    private float nextGridSpawnTime;
    private int row;
    private int column;
    private int streak;
    private int activeLevelIndex = 0;
    private bool isWaitingAfterClick = false;
    private List<float> starttime = new List<float>();
    private List<float> life_span = new List<float>();
    private List<float> flickeringspeed = new List<float>();
    private List<bool> isflickering = new List<bool>();
    private List<int> rows = new List<int>();
    private List<int> columns = new List<int>();
    private List<Vector2Int> allPositions = new List<Vector2Int>();
    private List<GameObject> active_tiles = new List<GameObject>();
    private List<GameObject> grid_lines = new List<GameObject>();
    
    private void Start()
    {
        GameSetup();
        StartCoroutine(GameLoop());
        StartCoroutine(GenerateGrid());
        StartCoroutine(flickering_manager.Flickering());
        GenerateGridNow();
    }

    void GameSetup()
    {
        initial_timer = activeSpacingSO.timer;

        // Calculate screen bounds
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = Camera.main.transform.position.y - halfHeight;
        minX = Camera.main.transform.position.x - halfWidth;
        maxX = Camera.main.transform.position.x + halfWidth;
        maxY = Camera.main.transform.position.y + halfHeight;

        // Load level data
        for (int i = 0; i < activeSpacingSO.spacinglevels.Count; i++)
        {
            starttime.Add(activeSpacingSO.spacinglevels[i].starttime);
            life_span.Add(activeSpacingSO.spacinglevels[i].speed);
            rows.Add(activeSpacingSO.spacinglevels[i].rows);
            columns.Add(activeSpacingSO.spacinglevels[i].columns);
            isflickering.Add(activeSpacingSO.spacinglevels[i].isflickering);
            flickeringspeed.Add(activeSpacingSO.spacinglevels[i].flickeringspeed);
        }

        lifespan = life_span[activeLevelIndex];

        // Set initial values
        if (life_span.Count > 0) lifespan = life_span[0];
        if (rows.Count > 0) row = rows[0];
        if (columns.Count > 0) column = columns[0];
    }



    void GenerateGridNow()
    {
        ClearGrid();
        lifespan = life_span[activeLevelIndex];
        allPositions.Clear();

        // Calculate grid dimensions with adjusted height
        float totalWidth = maxX - minX;
        float totalHeight = maxY - minY - 1; // This is your existing height reduction

        // Calculate cell sizes
        float cellWidth = totalWidth / column;
        float cellHeight = totalHeight / row;

        // Calculate vertical offset to make lines extend lower
       

 

        // Rest of your grid generation code (tile creation) remains the same...
        // Create all grid positions
        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < column; x++)
            {
                allPositions.Add(new Vector2Int(x, y));
            }
        }

        // Shuffle positions
        for (int i = 0; i < allPositions.Count; i++)
        {
            int randomIndex = Random.Range(i, allPositions.Count);
            Vector2Int temp = allPositions[i];
            allPositions[i] = allPositions[randomIndex];
            allPositions[randomIndex] = temp;
        }

        // Create tiles (using original positions without vertical extension)
        for (int i = 0; i < allPositions.Count; i++)
        {
            Vector2Int pos = allPositions[i];
            float xPos = minX + (pos.x * cellWidth) + (cellWidth / 2f);
            float yPos = minY + (pos.y * cellHeight) + (cellHeight / 2f);
            Vector3 scale = new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 1f);

            GameObject tile;
            if (i < 2)
            {
                tile = Instantiate(good_tile_prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                tile.GetComponent<ClickableObject>().OnClick.AddListener(TargetClicked);
                score_manager.total_score++;
            }
            else
            {
                tile = Instantiate(bad_tile_prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
            }

            tile.transform.localScale = scale;
            active_tiles.Add(tile);
        }
    }




    void ClearGrid()
    {
        foreach (var tile in active_tiles)
            Destroy(tile);
        active_tiles.Clear();

        foreach (var line in grid_lines)
            Destroy(line);
        grid_lines.Clear();
    }


    public void TargetClicked(GameObject good_tile)
    {
        good_tile.GetComponent<SpriteRenderer>().color = Color.green;
        score_manager.user_score++;
        streak++;
        if (streak >= 2)
        {
            streak = 0;
            StartCoroutine(WaitBeforeNewGrid());
        }
    }




    public void GameEnd()
    {
        ClearGrid();
        StopAllCoroutines();
    }


    public void SetActiveSpacingSO(SpacingSO val) => activeSpacingSO = val;

    private IEnumerator WaitBeforeNewGrid()
    {
        isWaitingAfterClick = true;
        yield return new WaitForSeconds(0.3f); // Wait for 0.3 seconds
        nextGridSpawnTime = Time.time; // Force immediate regeneration
        isWaitingAfterClick = false;
    }


    IEnumerator GenerateGrid()
    {
        // Initial grid spawn (no delay for first grid)
        nextGridSpawnTime = Time.time + lifespan;

        while (true)
        {
            if (Time.time >= nextGridSpawnTime && !isWaitingAfterClick)
            {
                ClearGrid();

                if (grid_delay > 0)
                {
                    yield return new WaitForSeconds(grid_delay);
                }

                GenerateGridNow();
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
            // Check for level changes
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