using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AbstractGameManager), typeof(ScoreManager))]
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

    // If the tiles used are circular shaped
    [SerializeField] private bool circular_tile = false;

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


    public void GenerateGrid(int row, int col)
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

        Vector3 scale = Vector3.zero;
        float circleDiameter = 0f;
        float offsetX = 0f;
        float offsetY = 0f;

        // Generate all positions first
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

        if (circular_tile)
        {
            // For circular tiles, we want them to fill the available space
            // Use the full cell dimensions but maintain aspect ratio
            circleDiameter = Mathf.Min(cellWidth, cellHeight);

            // Reduce spacing to minimize gaps (use almost the full cell size)
            float spacingFactor = 0.98f;
            circleDiameter *= spacingFactor;

            scale = new Vector3(circleDiameter, circleDiameter, 1f);

            // Calculate spacing between circles
            float horizontalSpacing = (totalWidth - (col * circleDiameter)) / (col + 1);
            float verticalSpacing = (totalHeight - (row * circleDiameter)) / (row + 1);

            // Use equal spacing on all sides
            offsetX = horizontalSpacing;
            offsetY = verticalSpacing;
        }
        else
        {
            scale = new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 1f);
        }

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

        if (has_diff_tile)
        {
            for (int i = 0; i < all_positions.Count; i++)
            {
                Vector2Int pos = all_positions[i];
                float xPos, yPos;

                if (circular_tile)
                {
                    // Use circular grid positioning with equal spacing
                    xPos = minX + offsetX + (pos.x * (circleDiameter + offsetX)) + (circleDiameter / 2f);
                    yPos = minY + offsetY + (pos.y * (circleDiameter + offsetY)) + (circleDiameter / 2f);
                }
                else
                {
                    // Use rectangular grid positioning
                    xPos = minX + (pos.x * cellWidth) + (cellWidth / 2f);
                    yPos = minY + (pos.y * cellHeight) + (cellHeight / 2f);
                }

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
        else
        {
            foreach (Vector2Int pos in all_positions)
            {
                float xPos, yPos;

                if (circular_tile)
                {
                    // Use circular grid positioning with equal spacing
                    xPos = minX + offsetX + (pos.x * (circleDiameter + offsetX)) + (circleDiameter / 2f);
                    yPos = minY + offsetY + (pos.y * (circleDiameter + offsetY)) + (circleDiameter / 2f);
                }
                else
                {
                    // Use rectangular grid positioning
                    xPos = minX + (pos.x * cellWidth) + (cellWidth / 2f);
                    yPos = minY + (pos.y * cellHeight) + (cellHeight / 2f);
                }

                GameObject tile = Instantiate(normal_tile_prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                tile.transform.localScale = scale;
                active_tiles.Add(tile);
                tile.GetComponent<ClickableObject>().OnClick.AddListener(game_manager.TargetClicked);
            }
        }
    }
}
