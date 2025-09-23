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

    [Header("Notch Settings")]
    [SerializeField] private float offset; 
    [SerializeField] private float notch_min_x;
    [SerializeField] private float notch_max_x;

    [SerializeField] private bool shuffle_grid = false;
    [SerializeField] private bool circular_tile = false;

    [SerializeField] private AbstractGameManager game_manager;
    [SerializeField] private BackgroundGenerator background_generator;

    [HideInInspector] public List<Vector2Int> all_positions = new List<Vector2Int>();
    [HideInInspector] public List<GameObject> active_tiles = new List<GameObject>();
    [HideInInspector] public List<GameObject> grid_lines = new List<GameObject>();

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
        foreach (var t in active_tiles)
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
        float totalHeight = maxY - minY;
        float cellWidth = totalWidth / col;
        float cellHeight = totalHeight / row;

        Vector3 scale = Vector3.zero;
        float circleDiameter = 0f;
        float offsetX = 0f;
        float offsetY = 0f;

        background_generator.GenerateBackground(row, col);

        // Generate all positions
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
            circleDiameter = Mathf.Min(cellWidth, cellHeight) * 0.98f;
            scale = new Vector3(circleDiameter, circleDiameter, 1f);

            float horizontalSpacing = (totalWidth - (col * circleDiameter)) / (col + 1);
            float verticalSpacing = (totalHeight - (row * circleDiameter)) / (row + 1);

            offsetX = horizontalSpacing;
            offsetY = verticalSpacing;
        }
        else
        {
            scale = new Vector3(cellWidth * 0.9f, cellHeight * 0.9f, 1f);
        }

        // === LINES ===
        if (has_lines)
        {
            // Vertical lines (split if they cross the notch)
            for (int x = 1; x < col; x++)
            {
                float xPos = minX + (x * cellWidth);

                if (xPos > notch_min_x && xPos < notch_max_x)
                {
                    // Segment 1: from minY up to notch bottom
                    float segment1Height = (maxY - offset) - minY;
                    float segment1CenterY = minY + segment1Height / 2f;

                    GameObject vLine1 = Instantiate(line_prefab, new Vector3(xPos, segment1CenterY, 0), Quaternion.identity);
                    vLine1.transform.localScale = new Vector3(line_thickness, segment1Height, 1f);
                    grid_lines.Add(vLine1);

                    // Segment 2 (above notch) is skipped because notch is at the very top
                }
                else
                {
                    // Full vertical line
                    float lineHeight = totalHeight;
                    float centerY = minY + lineHeight / 2f;

                    GameObject vLine = Instantiate(line_prefab, new Vector3(xPos, centerY, 0), Quaternion.identity);
                    vLine.transform.localScale = new Vector3(line_thickness, lineHeight, 1f);
                    grid_lines.Add(vLine);
                }

            }

            // Horizontal lines (always full)
            for (int y = 1; y < row; y++)
            {
                float yPos = minY + (y * cellHeight);
                GameObject hLine = Instantiate(line_prefab, new Vector3(0, yPos, 0), Quaternion.identity);
                hLine.transform.localScale = new Vector3(totalWidth, line_thickness, 1f);
                grid_lines.Add(hLine);
            }
        }

        // === TILES ===
        foreach (Vector2Int pos in all_positions)
        {
            float xPos, yPos;

            if (circular_tile)
            {
                xPos = minX + offsetX + (pos.x * (circleDiameter + offsetX)) + (circleDiameter / 2f);
                yPos = minY + offsetY + (pos.y * (circleDiameter + offsetY)) + (circleDiameter / 2f);
            }
            else
            {
                xPos = minX + (pos.x * cellWidth) + (cellWidth / 2f);
                yPos = minY + (pos.y * cellHeight) + (cellHeight / 2f);
            }

            bool insideNotch = (xPos >= notch_min_x && xPos <= notch_max_x) &&
                    (yPos >= maxY - offset && yPos <= maxY);



            if (!insideNotch)
            {
                GameObject tile;
                if (has_diff_tile)
                {
                    if (active_tiles.Count < number_of_diff_tiles)
                    {
                        tile = Instantiate(diff_tile_prefab, new Vector2(xPos, yPos), Quaternion.identity);
                        tile.GetComponent<ClickableObject>().OnClick.AddListener(game_manager.TargetClicked);
                    }
                    else
                    {
                        tile = Instantiate(normal_tile_prefab, new Vector2(xPos, yPos), Quaternion.identity);
                    }
                }
                else
                {
                    tile = Instantiate(normal_tile_prefab, new Vector2(xPos, yPos), Quaternion.identity);
                    tile.GetComponent<ClickableObject>().OnClick.AddListener(game_manager.TargetClicked);
                }
                tile.transform.localScale = scale;
                active_tiles.Add(tile);
            }
        }
    }
}
