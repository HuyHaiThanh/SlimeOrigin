using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Nhận input swipe (chuột trong Editor / chạm trên mobile qua Pointer.current) để swap
/// 2 tile kề nhau. Nếu swap tạo match → resolve cascade (match → gravity → refill → lặp lại).
/// Nếu không tạo match → swap trả về như cũ.
/// </summary>
public class SwapHandler : MonoBehaviour
{
    [SerializeField] private BoardManager board;
    [SerializeField] private Camera cam;
    [SerializeField] private float swipeThreshold = 20f; // pixel tối thiểu để tính là swipe

    public bool InputEnabled = true;
    public event Action<int, int, int, int> OnSwapRequested;  // (r1,c1,r2,c2) -> CombatManager xu ly

    private bool dragging;
    private int startRow, startCol;
    private Vector2 startScreenPos;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (!InputEnabled) return;

        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
            OnPress(pointer.position.ReadValue());
        else if (pointer.press.wasReleasedThisFrame && dragging)
            OnRelease(pointer.position.ReadValue());
    }

    private void OnPress(Vector2 screenPos)
    {
        Vector3 world = ScreenToBoardWorld(screenPos);
        if (board.WorldToGrid(world, out startRow, out startCol))
        {
            dragging = true;
            startScreenPos = screenPos;
        }
    }

    private void OnRelease(Vector2 screenPos)
    {
        dragging = false;
        Vector2 delta = screenPos - startScreenPos;
        if (delta.magnitude < swipeThreshold) return;

        // Xac dinh huong swipe chiem uu the -> o ke can swap.
        int dRow = 0, dCol = 0;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            dCol = delta.x > 0 ? 1 : -1;
        else
            dRow = delta.y > 0 ? 1 : -1;   // man hinh y len = row tang (row 0 o day)

        int targetRow = startRow + dRow;
        int targetCol = startCol + dCol;
        if (targetRow < 0 || targetRow >= board.Height || targetCol < 0 || targetCol >= board.Width)
            return;

        // Khong tu resolve nua — bao CombatManager dieu phoi luot.
        OnSwapRequested?.Invoke(startRow, startCol, targetRow, targetCol);
    }

    /// <summary>Swap thử; giữ nếu tạo match, ngược lại hoàn tác.</summary>


    /// <summary>Lặp xóa match → gravity → refill cho đến khi board không còn match.</summary>


    /// <summary>Đổi screen pos → world pos trên mặt phẳng board (z = 0).</summary>
    private Vector3 ScreenToBoardWorld(Vector2 screenPos)
    {
        Vector3 sp = new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(sp);
        world.z = 0f;
        return world;
    }
}
