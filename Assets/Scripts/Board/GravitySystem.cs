using System.Collections.Generic;

/// <summary>
/// Sau khi match bị xóa: dồn tile còn lại rơi xuống đáy mỗi cột, rồi spawn tile mới
/// lấp các ô trống phía trên. Thuần logic, dùng primitive của BoardManager.
/// Quy ước: row 0 = đáy → "rơi xuống" nghĩa là dồn về row nhỏ.
/// </summary>
public static class GravitySystem
{
    /// <summary>Xóa tập tile đã match, dồn cột, refill. Gọi sau MatchDetector.FindMatches.</summary>
    public static void ApplyGravityAndRefill(BoardManager board, HashSet<SlimeTile> matched)
    {
        // 1) Xóa toàn bộ tile đã match.
        foreach (SlimeTile tile in matched)
            board.RemoveTile(tile.Row, tile.Col);

        // 2) Mỗi cột: dồn tile còn lại xuống đáy, lấp phần trên bằng tile mới.
        for (int col = 0; col < board.Width; col++)
        {
            // writeRow = ô đáy thấp nhất chưa được lấp.
            int writeRow = 0;
            for (int readRow = 0; readRow < board.Height; readRow++)
            {
                if (board.GetTile(readRow, col) != null)
                {
                    if (readRow != writeRow)
                        board.MoveTile(readRow, col, writeRow, col);
                    writeRow++;
                }
            }

            // Các ô trống còn lại phía trên → tile mới rơi vào.
            for (int row = writeRow; row < board.Height; row++)
                board.CreateRandomTile(row, col);
        }
    }
}
