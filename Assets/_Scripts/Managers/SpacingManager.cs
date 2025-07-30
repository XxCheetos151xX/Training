using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpacingManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private GameObject bad_tile_prefab;
    [SerializeField] private GameObject good_tile_prefab;
    [SerializeField] private GameObject end_panel;
    [SerializeField] private GameObject grid_line_prefab;  
    [SerializeField] private Image blackscreen;
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private TextMeshProUGUI timer_txt;
    [SerializeField] private float lineThickness;
    [SerializeField] private float grid_delay;
    [SerializeField] UnityEvent GameEnded;


    private SpacingSO activeSpacingSO;
    private float minY, minX, maxX, maxY;
    private float timer;
    private float initial_timer;
    private float lifespan;
    private float score;
    private float total_targets;
    private float captured_targets;
    private float nextGridSpawnTime;
    private int row;
    private int column;
    private int missed_targets;
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
        StartCoroutine(Flickering());
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
        float verticalExtension = 1.0f; // Adjust this value to control how much lower the lines go
        float adjustedMinY = minY - verticalExtension;
        float adjustedTotalHeight = totalHeight + verticalExtension;

        // Draw vertical lines (extending below original grid)
        for (int x = 1; x < column; x++)
        {
            float xPos = minX + (x * cellWidth);
            // Position at center of extended height
            float yCenter = adjustedMinY + adjustedTotalHeight / 2f;
            GameObject vLine = Instantiate(grid_line_prefab,
                new Vector3(xPos, yCenter, 0),
                Quaternion.identity);
            // Scale to full extended height
            vLine.transform.localScale = new Vector3(lineThickness, adjustedTotalHeight, 1f);
            grid_lines.Add(vLine);
        }

        // Draw horizontal lines (normal positioning)
        for (int y = 1; y < row; y++)
        {
            float yPos = minY + (y * cellHeight);
            GameObject hLine = Instantiate(grid_line_prefab,
                new Vector3(0, yPos, 0),
                Quaternion.identity);
            hLine.transform.localScale = new Vector3(totalWidth, lineThickness, 1f);
            grid_lines.Add(hLine);
        }

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
                tile.GetComponent<ClickableObject>()._Onclick.AddListener(TargetClicked);
                total_targets++;
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


    public void TargetClicked()
    {
        captured_targets += 1;
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
        end_panel.SetActive(true);
        timer_txt.enabled = false;
        score = (captured_targets / total_targets) * 100;
        missed_targets = Mathf.FloorToInt(total_targets - captured_targets);
        score_txt.text = score.ToString("F2") + "%";
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
        while (timer <= activeSpacingSO.timer)
        {
            timer += Time.deltaTime;
            initial_timer -= Time.deltaTime;

            // Update timer display
            int minutes = Mathf.FloorToInt(initial_timer / 60f);
            int seconds = Mathf.FloorToInt(initial_timer % 60f);
            timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Check for level changes
            for (int i = 0; i < starttime.Count; i++)
            {
                if (timer >= starttime[i])
                {
                    lifespan = life_span[i];
                    activeLevelIndex=i;
                        row = rows[i];
                    column = columns[i];
                }
            }

            if (initial_timer <= 0)
            {
                GameEnded.Invoke();
            }

            yield return null;
        }
    }

    IEnumerator Flickering()
    {
        float flickerTimer = 0f;
        float currentFlickerSpeed = 0f;
        bool isFlickering = false;

        while (true)
        {
            for (int i = starttime.Count - 1; i >= 0; i--)
            {
                if (timer >= starttime[i])
                {
                    isFlickering = isflickering[i];
                    currentFlickerSpeed = flickeringspeed[i];
                    break;
                }
            }

            if (isFlickering)
            {
                flickerTimer += Time.deltaTime;
                if (flickerTimer >= currentFlickerSpeed)
                {
                    blackscreen.enabled = !blackscreen.enabled;
                    flickerTimer = 0f;
                }
            }
            else
            {
                if (blackscreen.enabled)
                    blackscreen.enabled = false;
            }

            yield return null;
        }
    }

}