using UnityEngine;
using System.Collections;

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
    private Coroutine moveRoutine;

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

    /// <summary>Tween mượt tới world position trong 'duration' giây. Logic đã cập nhật từ trước, đây chỉ là visual bắt kịp.</summary>
    public void MoveTo(Vector3 target, float duration)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        if (duration <= 0f || !gameObject.activeInHierarchy) { transform.position = target; return; }
        moveRoutine = StartCoroutine(MoveRoutine(target, duration));
    }

    private IEnumerator MoveRoutine(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            transform.position = Vector3.Lerp(start, target, k);
            yield return null;
        }
        transform.position = target;
        moveRoutine = null;
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
