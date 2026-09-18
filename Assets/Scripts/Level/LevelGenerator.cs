using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Existing manual level")]
    [SerializeField] private GameObject manualLevel;

    [Header("Level piece prefabs")]
    [SerializeField] private GameObject outsideCornerPrefab;
    [SerializeField] private GameObject outsideWallPrefab;
    [SerializeField] private GameObject insideCornerPrefab;
    [SerializeField] private GameObject insideWallPrefab;
    [SerializeField] private GameObject standardPelletPrefab;
    [SerializeField] private GameObject powerPelletPrefab;
    [SerializeField] private GameObject tJunctionPrefab;
    [SerializeField] private GameObject ghostExitWallPrefab;

    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

    private int[,] fullMap;

    private void Start()
    {
        if (manualLevel != null)
        {
            manualLevel.SetActive(false);
            Destroy(manualLevel);
        }

        fullMap = CreateMirroredMap();
        GenerateLevel();
        FitCameraToLevel();
    }

    private int[,] CreateMirroredMap()
    {
        int sourceRows = levelMap.GetLength(0);
        int sourceColumns = levelMap.GetLength(1);
        int generatedRows = sourceRows * 2 - 1;
        int generatedColumns = sourceColumns * 2;
        int[,] mirroredMap = new int[generatedRows, generatedColumns];

        for (int row = 0; row < generatedRows; row++)
        {
            int sourceRow = row < sourceRows ? row : generatedRows - 1 - row;

            for (int column = 0; column < generatedColumns; column++)
            {
                int sourceColumn = column < sourceColumns
                    ? column
                    : generatedColumns - 1 - column;

                mirroredMap[row, column] = levelMap[sourceRow, sourceColumn];
            }
        }

        return mirroredMap;
    }

    private void GenerateLevel()
    {
        Transform generatedRoot = new GameObject("GeneratedLevel01").transform;
        Transform topLeft = CreateQuadrant("TopLeft", generatedRoot, new Vector3(1f, 1f, 1f));
        Transform topRight = CreateQuadrant("TopRight", generatedRoot, new Vector3(-1f, 1f, 1f));
        Transform bottomLeft = CreateQuadrant("BottomLeft", generatedRoot, new Vector3(1f, -1f, 1f));
        Transform bottomRight = CreateQuadrant("BottomRight", generatedRoot, new Vector3(-1f, -1f, 1f));

        int sourceRows = levelMap.GetLength(0);
        BuildQuadrant(topLeft, sourceRows);
        BuildQuadrant(topRight, sourceRows);
        BuildQuadrant(bottomLeft, sourceRows - 1);
        BuildQuadrant(bottomRight, sourceRows - 1);
    }

    private static Transform CreateQuadrant(string quadrantName, Transform parent, Vector3 scale)
    {
        Transform quadrant = new GameObject(quadrantName).transform;
        quadrant.SetParent(parent);
        quadrant.localPosition = Vector3.zero;
        quadrant.localRotation = Quaternion.identity;
        quadrant.localScale = scale;
        return quadrant;
    }

    private void BuildQuadrant(Transform parent, int rowCount)
    {
        int sourceRows = levelMap.GetLength(0);
        int sourceColumns = levelMap.GetLength(1);

        float left = -sourceColumns + 0.5f;
        float top = sourceRows - 1f;

        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < sourceColumns; column++)
            {
                int tileType = levelMap[row, column];
                GameObject prefab = GetPrefab(tileType);
                if (prefab == null)
                {
                    continue;
                }

                Vector3 position = new Vector3(left + column, top - row, 0f);
                Quaternion rotation = Quaternion.Euler(0f, 0f, GetRotation(row, column, tileType));
                GameObject piece = Instantiate(prefab, parent);
                piece.transform.localPosition = position;
                piece.transform.localRotation = rotation;
                piece.transform.localScale = Vector3.one;
                piece.name = $"R{row:00}_C{column:00}_Type{tileType}";
            }
        }
    }

    private GameObject GetPrefab(int tileType)
    {
        switch (tileType)
        {
            case 1: return outsideCornerPrefab;
            case 2: return outsideWallPrefab;
            case 3: return insideCornerPrefab;
            case 4: return insideWallPrefab;
            case 5: return standardPelletPrefab;
            case 6: return powerPelletPrefab;
            case 7: return tJunctionPrefab;
            case 8: return ghostExitWallPrefab;
            default: return null;
        }
    }

    private float GetRotation(int row, int column, int tileType)
    {
        if (tileType == 1 || tileType == 3)
        {
            return GetCornerRotation(row, column);
        }

        if (tileType == 2 || tileType == 4 || tileType == 8)
        {
            return IsStraightWallVertical(row, column) ? 90f : 0f;
        }

        if (tileType == 7)
        {
            GetConnections(row, column, out bool up, out bool right, out bool down, out bool left);
            int connectionCount = CountTrue(up, right, down, left);

            if (column == levelMap.GetLength(1) - 1 && connectionCount < 3)
            {
                right = true;
            }

            if (row == levelMap.GetLength(0) - 1 && CountTrue(up, right, down, left) < 3)
            {
                down = true;
            }

            return GetTJunctionRotation(up, right, down, left);
        }

        return 0f;
    }

    private float GetCornerRotation(int row, int column)
    {
        GetConnections(row, column, out bool up, out bool right, out bool down, out bool left);

        bool cornerUp = IsCorner(GetSourceTile(row - 1, column));
        bool cornerRight = IsCorner(GetSourceTile(row, column + 1));
        bool cornerDown = IsCorner(GetSourceTile(row + 1, column));
        bool cornerLeft = IsCorner(GetSourceTile(row, column - 1));

        if (cornerUp)
        {
            down = false;
        }
        else if (cornerRight)
        {
            left = false;
        }
        else if (cornerDown)
        {
            up = false;
        }
        else if (cornerLeft)
        {
            right = false;
        }

        if (CountTrue(up, right, down, left) < 2 && column == levelMap.GetLength(1) - 1)
        {
            right = true;
        }

        if (CountTrue(up, right, down, left) < 2 && row == levelMap.GetLength(0) - 1)
        {
            down = true;
        }

        if (right && down) return 0f;
        if (up && right) return 90f;
        if (up && left) return 180f;
        if (left && down) return -90f;
        return 0f;
    }

    private static float GetTJunctionRotation(bool up, bool right, bool down, bool left)
    {
        if (!up && right && down && left) return 0f;
        if (up && !right && down && left) return -90f;
        if (up && right && !down && left) return 180f;
        if (up && right && down && !left) return 90f;
        return 0f;
    }

    private void GetConnections(
        int row,
        int column,
        out bool up,
        out bool right,
        out bool down,
        out bool left)
    {
        up = NeighborConnects(row - 1, column, true);
        right = NeighborConnects(row, column + 1, false);
        down = NeighborConnects(row + 1, column, true);
        left = NeighborConnects(row, column - 1, false);
    }

    private bool NeighborConnects(int row, int column, bool verticalConnection)
    {
        int tileType = GetSourceTile(row, column);

        if (tileType == 1 || tileType == 3 || tileType == 7)
        {
            return true;
        }

        if (tileType == 2 || tileType == 4 || tileType == 8)
        {
            return IsStraightWallVertical(row, column) == verticalConnection;
        }

        return false;
    }

    private bool IsStraightWallVertical(int row, int column)
    {
        int verticalNeighbors = 0;
        int horizontalNeighbors = 0;

        if (IsSourceWall(row - 1, column)) verticalNeighbors++;
        if (IsSourceWall(row + 1, column)) verticalNeighbors++;
        if (IsSourceWall(row, column - 1)) horizontalNeighbors++;
        if (IsSourceWall(row, column + 1)) horizontalNeighbors++;

        return verticalNeighbors > horizontalNeighbors;
    }

    private bool IsSourceWall(int row, int column)
    {
        int tileType = GetSourceTile(row, column);
        return tileType == 1 || tileType == 2 || tileType == 3 ||
               tileType == 4 || tileType == 7 || tileType == 8;
    }

    private int GetSourceTile(int row, int column)
    {
        if (row < 0 || row >= levelMap.GetLength(0) ||
            column < 0 || column >= levelMap.GetLength(1))
        {
            return 0;
        }

        return levelMap[row, column];
    }

    private static bool IsCorner(int tileType)
    {
        return tileType == 1 || tileType == 3;
    }

    private static int CountTrue(bool first, bool second, bool third, bool fourth)
    {
        int count = 0;
        if (first) count++;
        if (second) count++;
        if (third) count++;
        if (fourth) count++;
        return count;
    }

    private void FitCameraToLevel()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null)
        {
            return;
        }

        int rows = fullMap.GetLength(0);
        int columns = fullMap.GetLength(1);
        float heightRequired = rows * 0.5f + 1f;
        float widthRequired = (columns * 0.5f + 1f) / gameCamera.aspect;

        gameCamera.orthographic = true;
        gameCamera.orthographicSize = Mathf.Max(heightRequired, widthRequired);
        gameCamera.transform.position = new Vector3(0f, 0f, -10f);
    }
}
