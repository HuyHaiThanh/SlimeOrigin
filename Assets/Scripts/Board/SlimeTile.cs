using UnityEngine;

/// <summary>
/// View của 1 ô trên board. KHÔNG chứa logic match/gravity — chỉ hiển thị element
/// và snap tới vị trí. Toàn bộ logic board do BoardManager điều phối (mục 3.1 CLAUDE.md).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SlimeTile : MonoBehaviour
{
    // Toạ độ grid hiện tại — tiện cho input swap (raycast trúng tile là biết ô nào).
    public int Row { get; private set; }
    public int Col { get; private set; }
    public ElementType Element { get; private set; }

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>Gán element và đổi màu sprite placeholder tương ứng.</summary>
    public void SetElement(ElementType element)
    {
        Element = element;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetColor(element);
    }

    /// <summary>Cập nhật toạ độ grid (logic). Không tự dời world position.</summary>
    public void SetGridPosition(int row, int col)
    {
        Row = row;
        Col = col;
    }

    /// <summary>Snap tile tới world position. Chưa animation ở giai đoạn này (tween ở tuần 4).</summary>
    public void SnapToWorld(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    /// <summary>
    /// Màu placeholder theo element (CLAUDE.md mục 3.2). Pyro/Hydro/Cryo/White đúng spec;
    /// 4 element còn lại tạm suy từ mô tả mục 2.4, thay bằng sprite thật ở tuần 4.
    /// </summary>
    public static Color GetColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Pyro:    return new Color32(0xFF, 0x44, 0x00, 0xFF);
            case ElementType.Hydro:   return new Color32(0x00, 0x88, 0xFF, 0xFF);
            case ElementType.Cryo:    return new Color32(0x88, 0xDD, 0xFF, 0xFF);
            case ElementType.Electro: return new Color32(0xAA, 0x44, 0xFF, 0xFF);
            case ElementType.Dendro:  return new Color32(0x44, 0xCC, 0x44, 0xFF);
            case ElementType.Anemo:   return new Color32(0xBB, 0xCC, 0xCC, 0xFF);
            case ElementType.Geo:     return new Color32(0xCC, 0x99, 0x33, 0xFF);
            case ElementType.White:
            default:                  return new Color32(0xDD, 0xDD, 0xDD, 0xFF);
        }
    }
}
