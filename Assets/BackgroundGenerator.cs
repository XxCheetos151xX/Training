using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    [SerializeField] private GameObject background_tile;
    [SerializeField] private float tileSize = 1f;       // world units per cell
    [SerializeField, Range(0.1f, 1f)] private float fillPercent = 0.9f;
    

    private float minX, minY, maxX, maxY;

    private void Start()
    {
        SetupScreen();
        GenerateBackground();
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

    void GenerateBackground()
    {
        int cols = Mathf.CeilToInt((maxX - minX) / tileSize);
        int rows = Mathf.CeilToInt((maxY - minY) / tileSize);

        
        float actualTileSize = tileSize * fillPercent;
        Vector3 scale = new Vector3(actualTileSize, actualTileSize, 1f);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float xPos = minX + (x * tileSize) + tileSize / 2f;
                float yPos = minY + (y * tileSize) + tileSize / 2f;

                GameObject tile = Instantiate(background_tile, new Vector3(xPos, yPos, 0), Quaternion.identity);
                tile.transform.localScale = scale;
            }
        }
    }
}
