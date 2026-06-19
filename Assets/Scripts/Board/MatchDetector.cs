using System.Collections.Generic;

/// <summary>
/// Tìm tất cả match 3+ (cùng element, liên tiếp theo hàng hoặc cột) trên board.
/// Thuần logic, không giữ state — chỉ đọc grid từ BoardManager. Gọi sau khi board đầy.
/// </summary>
public static class MatchDetector
{
    /// <summary>
    /// Trả về tập hợp tile thuộc bất kỳ match 3+ nào. Dùng HashSet để tile giao nhau
    /// giữa match ngang và dọc (hình L, T) không bị tính trùng.
    /// </summary>
    public static HashSet<SlimeTile> FindMatches(BoardManager board)
    {
        var matched = new HashSet<SlimeTile>();
        int height = board.Height;
        int width = board.Width;

        // Quét từng hàng: gom các "run" liên tiếp cùng element, run dài >= 3 thì đánh dấu.
        for (int r = 0; r < height; r++)
        {
            int runStart = 0;
            for (int c = 1; c <= width; c++)
            {
                bool sameAsRun = c < width
                    && board.GetTile(r, c).Element == board.GetTile(r, runStart).Element;
                if (!sameAsRun)
                {
                    if (c - runStart >= 3)
                        for (int k = runStart; k < c; k++) matched.Add(board.GetTile(r, k));
                    runStart = c;
                }
            }
        }

        // Quét từng cột: tương tự theo chiều dọc.
        for (int c = 0; c < width; c++)
        {
            int runStart = 0;
            for (int r = 1; r <= height; r++)
            {
                bool sameAsRun = r < height
                    && board.GetTile(r, c).Element == board.GetTile(runStart, c).Element;
                if (!sameAsRun)
                {
                    if (r - runStart >= 3)
                        for (int k = runStart; k < r; k++) matched.Add(board.GetTile(k, c));
                    runStart = r;
                }
            }
        }

        return matched;
    }

    /// <summary>Board có ít nhất 1 match không — tiện để validate swap có hợp lệ không.</summary>
    public static bool HasAnyMatch(BoardManager board)
    {
        return FindMatches(board).Count > 0;
    }
}
