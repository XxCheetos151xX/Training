using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private GameObject normal_tile_prefab;
    [SerializeField] private GameObject diff_tile_prefab;
    [SerializeField] private bool has_diff_tile = false;
    [SerializeField] private int number_of_diff_tiles = 0;

    [Header("Line Settings")]
    [SerializeField] private GameObject line_prefab;
    [SerializeField] private float line_thickness;
    [SerializeField] private bool has_lines;

    // If needed to shuffle the positions each iteration
    [SerializeField] private bool shuffle_grid = false;

    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private AbstractGameManager game_manager;

    // Lists where all the grid data is stored 
    [HideInInspector] public List<Vector2Int> all_positions = new List<Vector2Int>();
    [HideInInspector] public List<GameObject> active_tiles = new List<GameObject>();
    [HideInInspector] public List<GameObject> grid_lines = new List<GameObject>();  

    // Screen Dimensions
    private float minX, minY, maxX, maxY;


    private void Start()
    {
        SetupScreen();
    }


    void SetupScreen()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = Camera.main.transform.position.y - halfHeight;
        minX = Camera.main.transform.position.x - halfWidth;
        maxX = Camera.main.transform.position.x + halfWidth;
        maxY = Camera.main.transform.position.y + halfHeight;
    }


    public void ClearGrid()
    {
        foreach(var t in active_tiles)
        {
            if (t) Destroy(t);
        }
        active_tiles.Clear();

        foreach (var l in grid_lines)
        {
            if (l) Destroy(l);
        }
        grid_lines.Clear();
    }


    public void GenerateGrid(int row , int col)
    {
        ClearGrid();
        all_positions.Clear();

        float totalWidth = maxX - minX;
        float totalHeight = maxY - minY - 1;
        float cellWidth = totalWidth / col;
        float cellHeight = totalHeight / row;

        float verticalExtension = 1f;
        float adjustedMinY = minY - verticalExtension;
        float adjustedTotalHeight = totalHeight + verticalExtension;

        if (has_lines)
        {
            for (int x = 1; x < col; x++)
            {
                float xPos = minX + (x * cellWidth);
                float yCenter = adjustedMinY + adjustedTotalHeight / 2f;
                GameObject vLine = Instantiate(line_prefab, new Vector3(xPos, yCenter, 0), Quaternion.identity);
                vLine.transform.localScale = new Vector3(line_thickness, adjustedTotalHeight, 1f);
                grid_lines.Add(vLine);
            }

            for (int y = 1; y < row; y++)
            {
                float yPos = minY + (y * cellHeight);
                GameObject hLine = Instantiate(line_prefab, new Vector3(0, yPos, 0), Quaternion.identity);
                hLine.transform.localScale = new Vector3(totalWidth, line_thickness, 1f);
                grid_lines.Add(hLine);
            }
        }

        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                all_positions.Add(new Vector2Int(x, y));
            }
        }

        if (shuffle_grid)
        {
            for (int i = 0; i < all_positions.Count; i++)
            {
                int randomIndex = Random.Range(i, all_positions.Count);
                Vector2Int temp = all_positions[i];
                all_positions[i] = all_positions[randomIndex];
                all_positions[randomIndex] = temp;
            }
        }

        if (has_diff_tile)
        {
            for (int i = 0; i < all_positions.Count; i++)
            {
                Vector2Int pos = all_positions[i];
                float xPos = minX + (pos.x * cellWidth) + (cellWidth / 2f);
                float yPos = minY + (pos.y * cellHeight) + (cellHeight / 2f);
                Vector3 scale = new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 1f);

                GameObject tile;
                if (i < number_of_diff_tiles)
                {
                    tile = Instantiate(diff_tile_prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                    tile.GetComponent<ClickableObject>().OnClick.AddListener(game_manager.TargetClicked);
                    score_manager.total_score++;
                }
                else
                {
                    tile = Instantiate(normal_tile_prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                }

                tile.transform.localScale = scale;
                active_tiles.Add(tile);
            }
        }

        else if (!has_diff_tile)
        {
            foreach (Vector2Int pos in all_positions)
            {
                float xPos = minX + (pos.x * cellWidth) + (cellWidth / 2f);
                float yPos = minY + (pos.y * cellHeight) + (cellHeight / 2f);
                Vector3 scale = new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 1f);

                GameObject tile = Instantiate(normal_tile_prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                tile.transform.localScale = scale;
                active_tiles.Add(tile);
                tile.GetComponent<ClickableObject>().OnClick.AddListener(game_manager.TargetClicked);
            }
        }

    }
}
