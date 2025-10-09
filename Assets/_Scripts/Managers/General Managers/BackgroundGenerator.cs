using UnityEngine;
using System.Collections.Generic;


public class BackgroundGenerator : MonoBehaviour
{
    [SerializeField] private GameObject background_tile;
    [SerializeField, Range(0.1f, 1f)] private float fillPercent = 0.9f;
    [SerializeField, Range(1, 5)] private int dotDensity = 1;
    [SerializeField] private bool circularTile = false;
        

    private float minX, minY, maxX, maxY;
    private List<GameObject> active_tiles = new List<GameObject>();

    private void Awake()
    {
        Application.targetFrameRate = -1;
        SetupScreen();
    }

    void ClearBackGround()
    {
        if (active_tiles.Count > 0)
        {
            foreach (var tile in active_tiles)
            {
                Destroy(tile);
            }
            active_tiles.Clear();
        }
    }

    /// <summary>
    /// Generates a constant background using the screen bounds, not rows/cols.
    /// Uses dotDensity and fillPercent, same as GenerateBackground.
    /// </summary>
    public void GenerateConstantBackGround(float tileSize)
    {
        int cols = Mathf.CeilToInt((maxX - minX) / tileSize);
        int rows = Mathf.CeilToInt((maxY - minY) / tileSize);

        ClearBackGround();

        float actualTileSize = tileSize * fillPercent / dotDensity;
        Vector3 scale = new Vector3(actualTileSize, actualTileSize, 1f);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Base tile center
                float baseX = minX + (x * tileSize) + tileSize / 2f;
                float baseY = minY + (y * tileSize) + tileSize / 2f;

                // Subdivide into smaller dots
                for (int dy = 0; dy < dotDensity; dy++)
                {
                    for (int dx = 0; dx < dotDensity; dx++)
                    {
                        float offsetX = (dx - (dotDensity - 1) / 2f) * (tileSize / dotDensity);
                        float offsetY = (dy - (dotDensity - 1) / 2f) * (tileSize / dotDensity);

                        GameObject dot = Instantiate(background_tile, new Vector3(baseX + offsetX, baseY + offsetY, 0), Quaternion.identity);
                        dot.transform.localScale = scale;
                        active_tiles.Add(dot);
                    }
                }
            }
        }
    }

    public void GenerateBackground(int rows, int cols)
    {
        float totalWidth = maxX - minX;
        float totalHeight = maxY - minY;
        float cellWidth = totalWidth / cols;
        float cellHeight = totalHeight / rows;

        ClearBackGround();

    


        if (circularTile)
        {
            float circleDiameter = Mathf.Min(cellWidth, cellHeight) * 0.98f;
            float horizontalSpacing = (totalWidth - (cols * circleDiameter)) / (cols + 1);
            float verticalSpacing = (totalHeight - (rows * circleDiameter)) / (rows + 1);

            float dotSize = circleDiameter * fillPercent / dotDensity;
            Vector3 scale = new Vector3(dotSize, dotSize, 1f);


            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    // Base tile center
                    float baseX = minX + horizontalSpacing + (x * (circleDiameter + horizontalSpacing)) + (circleDiameter / 2f);
                    float baseY = minY + verticalSpacing + (y * (circleDiameter + verticalSpacing)) + (circleDiameter / 2f);

                    // Subdivide into a grid inside each circle
                    for (int dy = 0; dy < dotDensity; dy++)
                    {
                        for (int dx = 0; dx < dotDensity; dx++)
                        {
                            float offsetX = (dx - (dotDensity - 1) / 2f) * (circleDiameter / dotDensity);
                            float offsetY = (dy - (dotDensity - 1) / 2f) * (circleDiameter / dotDensity);

                            GameObject dot = Instantiate(background_tile, new Vector3(baseX + offsetX, baseY + offsetY, 0), Quaternion.identity);
                            dot.transform.localScale = scale;
                            active_tiles.Add(dot);
                        }
                    }
                }
            }
        }
        else
        {
            // === Normal square alignment ===
            float actualTileSize = Mathf.Min(cellWidth, cellHeight) * fillPercent / dotDensity;
            Vector3 scale = new Vector3(actualTileSize, actualTileSize, 1f);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    // Base tile center
                    float baseX = minX + (x * cellWidth) + (cellWidth / 2f);
                    float baseY = minY + (y * cellHeight) + (cellHeight / 2f);

                    // Subdivide into smaller dots inside each cell
                    for (int dy = 0; dy < dotDensity; dy++)
                    {
                        for (int dx = 0; dx < dotDensity; dx++)
                        {
                            float offsetX = (dx - (dotDensity - 1) / 2f) * (cellWidth / dotDensity);
                            float offsetY = (dy - (dotDensity - 1) / 2f) * (cellHeight / dotDensity);

                            GameObject dot = Instantiate(background_tile, new Vector3(baseX + offsetX, baseY + offsetY, 0), Quaternion.identity);
                            dot.transform.localScale = scale;
                            active_tiles.Add(dot);
                        }
                    }
                }
            }
        }
        
    }

    private void SetupScreen()
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
}
