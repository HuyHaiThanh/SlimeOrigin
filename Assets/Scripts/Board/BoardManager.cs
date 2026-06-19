using UnityEngine;

/// <summary>
/// Nguồn dữ liệu duy nhất (single source of truth) cho board 7×7.
/// Giữ mảng logic SlimeTile[,], spawn grid, và quy đổi grid ↔ world.
/// Match/gravity/swap là các script riêng đọc/ghi vào board này (mục 3.1).
///
/// Quy ước toạ độ: row 0 = đáy, tăng dần lên trên → gravity kéo tile về row nhỏ.
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Kích thước")]
    [SerializeField] private int width = 7;
    [SerializeField] private int height = 7;
    [SerializeField] private float tileSize = 1.5f;   // units/tile, khớp CLAUDE.md mục 3.2

    [Header("Tile")]
    [SerializeField] private GameObject tilePrefab;    // prefab có SlimeTile + SpriteRenderer

    [Header("Element MVP")]
    // Board khởi tạo chỉ dùng 3 element này (mục 2.4 / 2.9). Thêm dần sau khi absorb.
    [SerializeField] private ElementType[] spawnPool = { ElementType.Pyro, ElementType.Hydro, ElementType.Cryo };

    [SerializeField] private float animDuration = 0.18f;   // thoi gian tween tile (giay)

    private SlimeTile[,] tiles;

    public int Width => width;
    public int Height => height;
    public SlimeTile GetTile(int row, int col) => tiles[row, col];
    public float AnimDuration => animDuration;

    void Start()
    {
        GenerateBoard();
    }

    /// <summary>Spawn toàn bộ grid, đảm bảo không có match-3 sẵn lúc khởi tạo.</summary>
    public void GenerateBoard()
    {
        tiles = new SlimeTile[height, width];

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                ElementType element = PickElementNoMatch(r, c);
                tiles[r, c] = SpawnTile(r, c, element);
            }
        }
    }

    /// <summary>Xóa toàn bộ tile cũ rồi spawn lại board mới — dùng khi reset màn.</summary>
    public void RegenerateBoard()
    {
        if (tiles != null)
        {
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    if (tiles[r, c] != null) Destroy(tiles[r, c].gameObject);
        }
        GenerateBoard();
    }


    /// <summary>
    /// Chọn element ngẫu nhiên từ spawnPool sao cho KHÔNG tạo match-3 với 2 ô đã đặt
    /// bên trái (ngang) và 2 ô bên dưới (dọc). Vì fill theo thứ tự tăng dần r,c nên
    /// chỉ cần tránh 2 neighbor đã có là đủ chặn match-3 ngay lúc spawn.
    /// </summary>
    private ElementType PickElementNoMatch(int r, int c)
    {
        // Giới hạn số lần thử để không kẹt vòng lặp nếu pool quá nhỏ.
        for (int attempt = 0; attempt < 20; attempt++)
        {
            ElementType candidate = spawnPool[Random.Range(0, spawnPool.Length)];

            bool horizontalMatch = c >= 2
                && tiles[r, c - 1].Element == candidate
                && tiles[r, c - 2].Element == candidate;

            bool verticalMatch = r >= 2
                && tiles[r - 1, c].Element == candidate
                && tiles[r - 2, c].Element == candidate;

            if (!horizontalMatch && !verticalMatch)
                return candidate;
        }
        // Fallback hiếm gặp: trả về phần tử đầu pool.
        return spawnPool[0];
    }

    /// <summary>Instantiate 1 tile, gán element + grid pos + snap world.</summary>
    private SlimeTile SpawnTile(int row, int col, ElementType element)
    {
        GameObject go = Instantiate(tilePrefab, transform);
        go.name = $"Tile_{row}_{col}";

        SlimeTile tile = go.GetComponent<SlimeTile>();
        tile.SetGridPosition(row, col);
        tile.SetElement(element);
        tile.SnapToWorld(GridToWorld(row, col));
        return tile;
    }

    /// <summary>Primitive: xóa tile tại (row,col) — destroy GameObject và set ô = null.</summary>
    public void RemoveTile(int row, int col)
    {
        if (tiles[row, col] != null)
        {
            Destroy(tiles[row, col].gameObject);
            tiles[row, col] = null;
        }
    }

    /// <summary>Primitive: dời tile trong array (from → to), cập nhật grid pos + snap world.</summary>
    public void MoveTile(int fromRow, int fromCol, int toRow, int toCol)
    {
        SlimeTile tile = tiles[fromRow, fromCol];
        tiles[toRow, toCol] = tile;
        tiles[fromRow, fromCol] = null;
        tile.SetGridPosition(toRow, toCol);
        tile.MoveTo(GridToWorld(toRow, toCol), animDuration);
    }

    /// <summary>
    /// Primitive: tạo tile element ngẫu nhiên từ pool tại (row,col), ghi vào array.
    /// Không ràng buộc no-match-3 (cho phép cascade khi refill).
    /// </summary>
    public SlimeTile CreateRandomTile(int row, int col)
    {
        ElementType element = spawnPool[Random.Range(0, spawnPool.Length)];
        SlimeTile tile = SpawnTile(row, col, element);
        tiles[row, col] = tile;
        // Spawn phia tren board roi roi xuong de co animation rot.
        Vector3 target = GridToWorld(row, col);
        tile.SnapToWorld(target + Vector3.up * (height * tileSize));
        tile.MoveTo(target, animDuration);
        return tile;
    }

    /// <summary>Nghịch đảo GridToWorld: từ world pos suy ra (row,col). Trả false nếu ngoài board.</summary>
    public bool WorldToGrid(Vector3 world, out int row, out int col)
    {
        Vector3 local = world - transform.position;
        col = Mathf.RoundToInt(local.x / tileSize + (width - 1) / 2f);
        row = Mathf.RoundToInt(local.y / tileSize + (height - 1) / 2f);
        return row >= 0 && row < height && col >= 0 && col < width;
    }

    /// <summary>Primitive: hoán đổi 2 tile trong array + cập nhật grid pos + snap world cả hai.</summary>
    public void SwapTiles(int r1, int c1, int r2, int c2)
    {
        SlimeTile a = tiles[r1, c1];
        SlimeTile b = tiles[r2, c2];
        tiles[r1, c1] = b;
        tiles[r2, c2] = a;
        a.SetGridPosition(r2, c2);
        a.MoveTo(GridToWorld(r2, c2), animDuration);
        b.SetGridPosition(r1, c1);
        b.MoveTo(GridToWorld(r1, c1), animDuration);
    }



    /// <summary>Quy đổi grid → world, board canh giữa quanh transform của BoardManager.</summary>
    public Vector3 GridToWorld(int row, int col)
    {
        float x = (col - (width - 1) / 2f) * tileSize;
        float y = (row - (height - 1) / 2f) * tileSize;
        return transform.position + new Vector3(x, y, 0f);
    }
}
