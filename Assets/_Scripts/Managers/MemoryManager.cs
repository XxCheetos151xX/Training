using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;


public class MemoryManager : MonoBehaviour
{
    
    [SerializeField] private GameObject tile_prefab;
    [SerializeField] private GameObject grid_line_prefab;
    [SerializeField] private float line_thickness;
    [SerializeField] Color patteren_color;
    [SerializeField] Color right_color;
    [SerializeField] Color wrong_color;
    [SerializeField] private TextMeshProUGUI timer_txt;


    private MemorySO activeMemorySO;
    private float timer;
    private float initial_timer;
    private float user_time_window;
    private float flickering_speed;
    private float not_todo_prob;
    private float minX, maxX, minY, maxY;
    private int patteren_size;
    private int row;
    private int col;
    private int index;
    private bool is_flickering;
    private GameObject tile;
    private Color original_color;
    private List<float> _starttime = new List<float>();
    private List<float> _usertimewindow = new List<float>();
    private List<float> _fliceringspeed = new List<float>();
    private List<float> _nottodoprob = new List<float>();
    private List<int> _patterensize = new List<int>();
    private List<int> _rows = new List<int>();
    private List<int> _cols = new List<int>();
    private List<bool> _isflickering = new List<bool>();
    private List<GameObject> _gridlines = new List<GameObject>();
    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> patteren = new List<GameObject>();




    private void Start()
    {
        GameSetup();
        StartCoroutine(GameLoop());
        GenerateGridNow();
        StartCoroutine(GeneratePatteren());
    }


    void GameSetup()
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
            _patterensize.Add(activeMemorySO.memorylevels[i].patternsize);
            _usertimewindow.Add(activeMemorySO.memorylevels[i].usertimewindow);
            _fliceringspeed.Add(activeMemorySO.memorylevels[i].flickeringspeed);
            _nottodoprob.Add(activeMemorySO.memorylevels[i].nottodoprob);
            _rows.Add(activeMemorySO.memorylevels[i].rows);
            _cols.Add(activeMemorySO.memorylevels[i].cols);
            _isflickering.Add(activeMemorySO.memorylevels[i].isflickering);
        }
    }



    void GenerateGridNow()
    {
        float totalWidth = maxX - minX;
        float totalHeight = maxY - minY - 1; 

        
        float cellWidth = totalWidth / col;
        float cellHeight = totalHeight / row;

       
        float verticalExtension = 1.0f; 
        float adjustedMinY = minY - verticalExtension;
        float adjustedTotalHeight = totalHeight + verticalExtension;

        
        for (int x = 1; x < col; x++)
        {
            float xPos = minX + (x * cellWidth);
           
            float yCenter = adjustedMinY + adjustedTotalHeight / 2f;
            GameObject vLine = Instantiate(grid_line_prefab,
                new Vector3(xPos, yCenter, 0),
                Quaternion.identity);
           
            vLine.transform.localScale = new Vector3(line_thickness, adjustedTotalHeight, 1f);
            _gridlines.Add(vLine);
        }

       
        for (int y = 1; y < row; y++)
        {
            float yPos = minY + (y * cellHeight);
            GameObject hLine = Instantiate(grid_line_prefab,
                new Vector3(0, yPos, 0),
                Quaternion.identity);
            hLine.transform.localScale = new Vector3(totalWidth, line_thickness, 1f);
            _gridlines.Add(hLine);
        }

        for (int x = 0; x < col; x++)
        {
            for (int y = 0; y < row; y++)
            {
                float xPos = minX + (x * cellWidth) + (cellWidth / 2f);
                float yPos = minY + (y * cellHeight) + (cellHeight / 2f);
                Vector3 position = new Vector3(xPos, yPos, 0);
                Vector3 scale = new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 1f);
                tile = Instantiate(tile_prefab, position, Quaternion.identity);
                tiles.Add(tile);
                tile.transform.localScale = scale;
            }
        }
    }



    public void SetActiveMemorySO(MemorySO val) => activeMemorySO = val;



    IEnumerator GameLoop()
    {
        while (timer != activeMemorySO.timer)
        {
            timer += Time.deltaTime;
            initial_timer -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(initial_timer / 60);
            int seconds = Mathf.FloorToInt(initial_timer % 60);

            timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            for (int i = 0; i < _starttime.Count; i++)
            {
                if (timer >= _starttime[i])
                {
                    patteren_size = _patterensize[i];
                    user_time_window = _usertimewindow[i];
                    flickering_speed = _fliceringspeed[i];
                    not_todo_prob = _nottodoprob[i];
                    row = _rows[i];
                    col = _cols[i];
                    is_flickering = _isflickering[i];
                }
            }

            yield return null;
        }
    }


    IEnumerator GeneratePatteren()
    {
        while (true)
        {
            for (int i = 0; i < patteren_size; i++)
            {
                index = Random.Range(0, tiles.Count);
                tiles[index].GetComponent<SpriteRenderer>().color = patteren_color;
                patteren.Add(tiles[index]);
                yield return new WaitForSeconds(2);
                tiles[index].GetComponent<SpriteRenderer>().color = original_color;
            }
            yield return new WaitForSeconds(user_time_window);
        }
    }
}
