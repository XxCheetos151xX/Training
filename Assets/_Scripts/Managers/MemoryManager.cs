using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;



public class MemoryManager : AbstractGameManager
{
    [Header("Game References")]
    [SerializeField] private FlickeringManager flickering_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private GridManager grid_manager;
    [SerializeField] private UIManager ui_manager;

    [Header("Game Settings")]
    [SerializeField] private float delay;
    [SerializeField] private float score_tobe_added;
    [SerializeField] private Color original_color;
    [SerializeField] private Color pattern_color;
    [SerializeField] private Color right_color;
    [SerializeField] private Color wrong_color;
    [SerializeField] private UnityEvent GameEnd;

    private MemorySO activeMemorySO;
    private float timer;
    private float user_time_window;
    private float pattern_speed;
    private float score_ratio;
    private int row, col, pattern_size;
    private int current_stage;
    private int streak;
    private bool stopPattern = false;
    private Coroutine patternCoroutine;
    private List<float> _flickerstarttime = new List<float>();
    private List<float> _paternspeed = new List<float>();
    private List<float> _usertimewindow = new List<float>();
    private List<float> _flickeringtime = new List<float>();
    private List<float> _scoreratio = new List<float>();            
    private List<int> _rows = new List<int>();
    private List<int> _columns = new List<int>();
    private List<int> _patternsize = new List<int>();
    private List<bool> _isflickering = new List<bool>();
    private List<GameObject> pattern = new List<GameObject>();
    private List<GameObject> pressed_tiles = new List<GameObject>();


    private void Start()
    {
        GameSetup();
        StartCoroutine(GameInit());
    }



    private void GameSetup()
    {
        timer = 0;
        initial_timer = activeMemorySO.timer;

        current_stage = 0;

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
            _scoreratio.Add(activeMemorySO.memorylevels[i].scoreratio);
        }

        pattern_speed = _paternspeed[current_stage];
        user_time_window = _usertimewindow[current_stage];
        pattern_size = _patternsize[current_stage];
        row = _rows[current_stage];
        col = _columns[current_stage];
        flickering_manager.flickeringspeed = _flickeringtime[current_stage];
        flickering_manager.isflickering = _isflickering[current_stage];
    }



    public override void TargetClicked(GameObject t)
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
            score_manager.user_score += score_tobe_added * score_ratio;
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


    IEnumerator GameInit()
    {
        yield return null;
        grid_manager.GenerateGrid(row, col);
        StartCoroutine(ui_manager.Timer());
        StartCoroutine(GameLoop());
        patternCoroutine = StartCoroutine(GeneratePattern());
        StartCoroutine(flickering_manager.Flickering());
    }


    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            initial_timer -= Time.deltaTime;

            for (int i = 0; i < _flickerstarttime.Count; i++)
            {
                if (timer >= _flickerstarttime[i])
                {
                    flickering_manager.isflickering = _isflickering[i];
                    flickering_manager.flickeringspeed = _flickeringtime[i];
                    score_ratio = _scoreratio[i];
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
            score_manager.total_score += score_tobe_added * score_ratio;

            
            foreach (var t in grid_manager.active_tiles)
                if (t != null && t.TryGetComponent(out BoxCollider2D col))
                    col.enabled = false;

            pattern.Clear();
            pressed_tiles.Clear();

            GameObject lastTile = null;
            SpriteRenderer lastRend = null;

            for (int i = 0; i < pattern_size; i++)
            {
                if (stopPattern) yield break;

                GameObject tile = grid_manager.active_tiles[Random.Range(0, grid_manager.active_tiles.Count)];

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

            
            foreach (var t in grid_manager.active_tiles)
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
        pattern.Clear();
        grid_manager.GenerateGrid(row, col);
    }

}
