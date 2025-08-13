using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.VisualScripting;

public class MemoryManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private GameObject tile_prefab;
    [SerializeField] private GameObject line_prefab;
    [SerializeField] private FlickeringManager flickering_manager;
    [SerializeField] private ScoreManager score_manager;

    [Header("Game Settings")]
    [SerializeField] private float line_thckness;
    [SerializeField] private float delay;
    [SerializeField] private Color pattern_color;
    [SerializeField] private Color right_color;
    [SerializeField] private Color wrong_color;
    [SerializeField] private UnityEvent GameEnd;

    private MemorySO activeMemorySO;
    private float timer;
    private float minX, maxX, minY, maxY;
    private float user_time_window;
    private float pattern_speed;
    private int row, col, pattern_size;
    private int current_stage;
    private int streak;
    private bool stopPattern = false;
    private Color original_color;
    private Coroutine patternCoroutine;
    private List<float> _flickerstarttime = new List<float>();
    private List<float> _paternspeed = new List<float>();
    private List<float> _usertimewindow = new List<float>();
    private List<float> _flickeringtime = new List<float>();
    private List<int> _rows = new List<int>();
    private List<int> _columns = new List<int>();
    private List<int> _patternsize = new List<int>();
    private List<bool> _isflickering = new List<bool>();
    private List<Vector2Int> allpositions = new List<Vector2Int>();
    private List<GameObject> grid_lines = new List<GameObject>();
    private List<GameObject> active_tiles = new List<GameObject>();
    private List<GameObject> pattern = new List<GameObject>();
    private List<GameObject> pressed_tiles = new List<GameObject>();

    private void Start()
    {
        GameSetup();
        GenerateGrid();
        StartCoroutine(GameLoop());
        patternCoroutine = StartCoroutine(GeneratePattern());
        StartCoroutine(flickering_manager.Flickering());
    }

    private void GameSetup()
    {
        timer = 0;
        initial_timer = activeMemorySO.timer;
        original_color = tile_prefab.GetComponent<SpriteRenderer>().color;

        current_stage = 0;

        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = Camera.main.transform.position.y - halfHeight;
        minX = Camera.main.transform.position.x - halfWidth;
        maxX = Camera.main.transform.position.x + halfWidth;
        maxY = Camera.main.transform.position.y + halfHeight;

        for (int i = 0; i < activeMemorySO.memorylevels.Count; i++)
        {
            _paternspeed.Add(activeMemorySO.memorylevels[i].patternspeed);
            _flickerstarttime.Add(activeMemorySO.memorylevels[i].flickerstarttime);
            _usertimewindow.Add(activeMemorySO.memorylevels[i].usertimewindow);
            _patternsize.Add(activeMemorySO.memorylevels[i].patternsize);
            _rows.Add(activeMemorySO.memorylevels[i].rows);
            _columns.Add(activeMemorySO.memorylevels[i].cols);
            _isflickering.Add(activeMemorySO.memorylevels[i].isflickering);
            _flickeringtime.Add(activeMemorySO.memorylevels[i].flickeringspeed);
        }

        pattern_speed = _paternspeed[current_stage];
        user_time_window = _usertimewindow[current_stage];
        pattern_size = _patternsize[current_stage];
        row = _rows[current_stage];
        col = _columns[current_stage];
        flickering_manager.flickeringspeed = _flickeringtime[current_stage];
        flickering_manager.isflickering = _isflickering[current_stage];
    }

    void GenerateGrid()
    {
        ClearGrid();
        pattern.Clear();
        allpositions.Clear();

        float totalWidth = maxX - minX;
        float totalHeight = maxY - minY - 1;
        float cellWidth = totalWidth / col;
        float cellHeight = totalHeight / row;

        float verticalExtension = 1f;
        float adjustedMinY = minY - verticalExtension;
        float adjustedTotalHeight = totalHeight + verticalExtension;

        for (int x = 1; x < col; x++)
        {
            float xPos = minX + (x * cellWidth);
            float yCenter = adjustedMinY + adjustedTotalHeight / 2f;
            GameObject vLine = Instantiate(line_prefab, new Vector3(xPos, yCenter, 0), Quaternion.identity);
            vLine.transform.localScale = new Vector3(line_thckness, adjustedTotalHeight, 1f);
            grid_lines.Add(vLine);
        }

        for (int y = 1; y < row; y++)
        {
            float yPos = minY + (y * cellHeight);
            GameObject hLine = Instantiate(line_prefab, new Vector3(0, yPos, 0), Quaternion.identity);
            hLine.transform.localScale = new Vector3(totalWidth, line_thckness, 1f);
            grid_lines.Add(hLine);
        }

        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                allpositions.Add(new Vector2Int(x, y));
            }
        }

        foreach (Vector2Int pos in allpositions)
        {
            float xPos = minX + (pos.x * cellWidth) + (cellWidth / 2f);
            float yPos = minY + (pos.y * cellHeight) + (cellHeight / 2f);
            Vector3 scale = new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 1f);

            GameObject tile = Instantiate(tile_prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
            tile.transform.localScale = scale;
            active_tiles.Add(tile);
            tile.GetComponent<ClickableObject>().OnClick.AddListener(TileClicked);
        }
    }

    void ClearGrid()
    {
        foreach (var tile in active_tiles)
            if (tile) Destroy(tile);
        active_tiles.Clear();

        foreach (var line in grid_lines)
            if (line) Destroy(line);
        grid_lines.Clear();
    }

    void TileClicked(GameObject t)
    {

        if (t == null) return;

        if (stopPattern) return;

        if (t.TryGetComponent(out SpriteRenderer rend))
            rend.color = pattern_color;

        pressed_tiles.Add(t);

        bool wrong_tile = false;

        int checkCount = Mathf.Min(pressed_tiles.Count, pattern.Count);
        for (int i = 0; i < checkCount; i++)
        {
            if (pressed_tiles[i] != pattern[i])
            {
                wrong_tile = true;
            }
        }

        if (wrong_tile)
        {
            score_manager.misses++;

            foreach (var tile in pressed_tiles)
            {
                if (tile != null && tile.TryGetComponent(out SpriteRenderer r))
                    r.color = wrong_color;
            }

            StartCoroutine(DelayedResetAndGenerate());
        }

        if (!wrong_tile && pressed_tiles.Count == pattern.Count)
        {
            score_manager.user_score++;
            streak++;

            if (streak >= 3 && current_stage + 1 < _usertimewindow.Count)
            {
                current_stage++;
                pattern_speed = _paternspeed[current_stage];
                user_time_window = _usertimewindow[current_stage];
                pattern_size = _patternsize[current_stage];
                row = _rows[current_stage];
                col = _columns[current_stage];
                flickering_manager.isflickering = _isflickering[current_stage];
                flickering_manager.flickeringspeed = _flickeringtime[current_stage];
                StartCoroutine(DelayOnly());
                streak = 0;
            }

            foreach (var tile in pressed_tiles)
            {
                if (tile != null && tile.TryGetComponent(out SpriteRenderer r))
                    r.color = right_color;
            }

            StartCoroutine(DelayedResetAndGenerate());
        }
    }

    public void GameEnded()
    {
        StopAllCoroutines();
    }

    public void SetActiveMemorySO(MemorySO val) => activeMemorySO = val;

    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < _flickerstarttime.Count; i++)
            {
                if (timer >= _flickerstarttime[i])
                {
                    flickering_manager.isflickering = _isflickering[i];
                    flickering_manager.flickeringspeed = _flickeringtime[i];
                }
            }


            if (initial_timer <= 0)
            {
                GameEnd.Invoke();
            }

            yield return null;
        }
    }

    IEnumerator GeneratePattern()
    {
        while (true)
        {
            stopPattern = false;
            score_manager.total_score++;

            // Disable interaction
            foreach (var t in active_tiles)
                if (t != null && t.TryGetComponent(out BoxCollider2D col))
                    col.enabled = false;

            pattern.Clear();
            pressed_tiles.Clear();

            GameObject lastTile = null;
            SpriteRenderer lastRend = null;

            for (int i = 0; i < pattern_size; i++)
            {
                if (stopPattern) yield break;

                GameObject tile = active_tiles[Random.Range(0, active_tiles.Count)];

                if (tile != null && tile.TryGetComponent(out SpriteRenderer rend))
                {
                    pattern.Add(tile);

                    rend.color = pattern_color;
                    lastTile = tile;
                    lastRend = rend;

                    float elapsed = 0f;
                    while (elapsed < pattern_speed)
                    {
                        if (stopPattern)
                        {
                            rend.color = original_color;
                            yield break;
                        }
                        elapsed += Time.deltaTime;
                        yield return null;
                    }

                    if (!stopPattern && rend != null)
                        rend.color = original_color;

                    float pause = 0f;
                    while (pause < 0.2f)
                    {
                        if (stopPattern) yield break;
                        pause += Time.deltaTime;
                        yield return null;
                    }
                }
            }

            // Enable interaction
            foreach (var t in active_tiles)
                if (t != null && t.TryGetComponent(out BoxCollider2D col))
                    col.enabled = true;

            if (stopPattern) yield break;

            float timePassed = 0f;
            while (timePassed < user_time_window)
            {
                if (stopPattern) yield break;

                if (pressed_tiles.Count == pattern.Count)
                    yield break;

                timePassed += Time.deltaTime;
                yield return null;
            }

            if (stopPattern) yield break;

            if (pressed_tiles.Count < pattern.Count)
            {
                score_manager.misses++;
                StartCoroutine(DelayedResetAndGenerate());
                yield break;
            }
        }
    }


    IEnumerator DelayedResetAndGenerate()
    {
        if (patternCoroutine != null)
        {
            stopPattern = true;
            StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }


        yield return new WaitForSeconds(delay);

        foreach (var tile in pressed_tiles)
        {
            if (tile != null && tile.TryGetComponent(out SpriteRenderer rend))
                rend.color = original_color;
        }

        pressed_tiles.Clear();

        patternCoroutine = StartCoroutine(GeneratePattern());
    }


    IEnumerator DelayOnly()
    {
        yield return new WaitForSeconds(delay);
        GenerateGrid();
    }

}
