using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class MemoryManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private GameObject tile_prefab;
    [SerializeField] private GameObject line_prefab;
    [SerializeField] private GameObject end_panel;
    [SerializeField] private Image black_screen;
    [SerializeField] private TextMeshProUGUI timer_txt;
    [SerializeField] private TextMeshProUGUI score_txt;


    [Header("Game Settings")]
    [SerializeField] private float line_thckness;
    [SerializeField] private float delay;
    [SerializeField] private Color pattern_color;
    [SerializeField] private Color right_color;
    [SerializeField] private Color wrong_color;
    [SerializeField] private UnityEvent GameEnd;

    private MemorySO activeMemorySO;
    private float timer;
    private float initial_timer;
    private float total_patterns;
    private float correct_patterns;
    private float missed_patterns;
    private float score;
    private float minX, maxX, minY, maxY;
    private float start_time;
    private float user_time_window;
    private float flickering_time;
    private int row, col, pattern_size;
    private int lastActivatedLevel = -1;
    private bool is_flickering;
    private bool stopPattern = false;
    private Color original_color;
    private Coroutine patternCoroutine;
    private List<float> _starttime = new List<float>();
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
        StartCoroutine(GameLoop());
        StartCoroutine(Flickering());
    }

    private void GameSetup()
    {
        timer = 0;
        initial_timer = activeMemorySO.timer;
        original_color = tile_prefab.GetComponent<SpriteRenderer>().color;

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
            _starttime.Add(activeMemorySO.memorylevels[i].starttime);
            _usertimewindow.Add(activeMemorySO.memorylevels[i].usertimewindow);
            _patternsize.Add(activeMemorySO.memorylevels[i].patternsize);
            _rows.Add(activeMemorySO.memorylevels[i].rows);
            _columns.Add(activeMemorySO.memorylevels[i].cols);
            _isflickering.Add(activeMemorySO.memorylevels[i].isflickering);
            _flickeringtime.Add(activeMemorySO.memorylevels[i].flickeringspeed);
        }
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
            missed_patterns++;

            foreach (var tile in pressed_tiles)
            {
                if (tile != null && tile.TryGetComponent(out SpriteRenderer r))
                    r.color = wrong_color;
            }

            StartCoroutine(DelayedResetAndGenerate());
        }

        if (!wrong_tile && pressed_tiles.Count == pattern.Count)
        {
            correct_patterns++;

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
        score = (correct_patterns / total_patterns) * 100;
        score_txt.text = score.ToString("F2") + "%";
        end_panel.SetActive(true);
        timer_txt.enabled = false;
    }

    public void SetActiveMemorySO(MemorySO val) => activeMemorySO = val;

    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;
            initial_timer -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(initial_timer / 60f);
            int seconds = Mathf.FloorToInt(initial_timer % 60f);
            timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            for (int i = 0; i < _starttime.Count; i++)
            {
                if (timer >= _starttime[i] && i > lastActivatedLevel)
                {
                    lastActivatedLevel = i;

                    stopPattern = true;
                    if (patternCoroutine != null)
                    {
                        StopCoroutine(patternCoroutine);
                        patternCoroutine = null;
                    }

                    start_time = _starttime[i];
                    user_time_window = _usertimewindow[i];
                    pattern_size = _patternsize[i];
                    row = _rows[i];
                    col = _columns[i];
                    is_flickering = _isflickering[i];
                    flickering_time = _flickeringtime[i];

                    pattern.Clear();

                    GenerateGrid();
                    patternCoroutine = StartCoroutine(GeneratePattern());
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
        stopPattern = false;

        total_patterns++;

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
            if (stopPattern) break;

            GameObject tile = active_tiles[Random.Range(0, active_tiles.Count)];
            if (tile != null && tile.TryGetComponent(out SpriteRenderer rend))
            {
                pattern.Add(tile);

                rend.color = pattern_color;
                lastTile = tile;
                lastRend = rend;

                float elapsed = 0f;
                while (elapsed < 1f)
                {
                    if (stopPattern) break;
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (!stopPattern && tile != null && rend != null)
                    rend.color = original_color;

                float pause = 0f;
                while (pause < 0.2f)
                {
                    if (stopPattern) break;
                    pause += Time.deltaTime;
                    yield return null;
                }
            }
        }

        // Safety: reset color if stopped mid-flash
        if (stopPattern && lastTile != null && lastRend != null)
            lastRend.color = original_color;

        // Enable interaction if not cancelled
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

        if (pressed_tiles.Count < pattern.Count)
        {
            missed_patterns++;
            StartCoroutine(DelayedResetAndGenerate());
        }
    }

    IEnumerator DelayedResetAndGenerate()
    {
        yield return new WaitForSeconds(delay);

        foreach (var tile in pressed_tiles)
        {
            if (tile != null && tile.TryGetComponent(out SpriteRenderer rend))
                rend.color = original_color;
        }

        pressed_tiles.Clear();

        if (patternCoroutine != null)
        {
            stopPattern = true;
            StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }

        patternCoroutine = StartCoroutine(GeneratePattern());
    }


    IEnumerator Flickering()
    {
        float flickerTimer = 0f;
        float currentFlickerSpeed = 0f;
        bool isFlickering = false;

        while (true)
        {
            for (int i = _starttime.Count - 1; i >= 0; i--)
            {
                if (timer >= _starttime[i])
                {
                    isFlickering = is_flickering;
                    currentFlickerSpeed = flickering_time;
                    break;
                }
            }

            if (isFlickering)
            {
                flickerTimer += Time.deltaTime;
                if (flickerTimer >= currentFlickerSpeed)
                {
                    black_screen.enabled = !black_screen.enabled;
                    flickerTimer = 0f;
                }
            }
            else
            {
                if (black_screen.enabled)
                    black_screen.enabled = false;
            }

            yield return null;
        }
    }

}
