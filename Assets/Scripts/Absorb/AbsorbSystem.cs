using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Xử lý absorb sau khi quái chết (mục 2.7): đổi 3 tile ngẫu nhiên sang element của quái
/// (board phản ánh thứ đã ăn), mở khóa skill thưởng. Popup Yes/No do GameManager/UI điều phối.
/// </summary>
public class AbsorbSystem : MonoBehaviour
{
    [SerializeField] private BoardManager board;
    [SerializeField] private SkillSystem skillSystem;
    [SerializeField] private int tilesToConvert = 3;

    // Các element đã absorb (để biết lần đầu gặp element mới).
    private readonly HashSet<ElementType> absorbedElements = new HashSet<ElementType>();

    // (enemy, isNewElement) — cho GameManager/Dialogue dùng.
    public event Action<EnemyData, bool> OnAbsorbed;

    /// <summary>Thực hiện absorb 1 quái: đổi tile + unlock skill + báo event.</summary>
    public void Absorb(EnemyData enemy)
    {
        if (enemy == null) return;

        ConvertRandomTiles(enemy.element, tilesToConvert);
        bool isNewElement = absorbedElements.Add(enemy.element);   // true nếu lần đầu absorb element này

        if (enemy.rewardSkill != null && skillSystem != null)
            skillSystem.UnlockSkill(enemy.rewardSkill);

        OnAbsorbed?.Invoke(enemy, isNewElement);
    }

    /// <summary>Đổi 'count' tile ngẫu nhiên trên board sang element mới.</summary>
    private void ConvertRandomTiles(ElementType element, int count)
    {
        if (board == null) return;
        int w = board.Width, h = board.Height, total = w * h;
        count = Mathf.Min(count, total);

        var chosen = new HashSet<int>();
        int guard = 0;
        while (chosen.Count < count && guard++ < 1000)
        {
            int idx = UnityEngine.Random.Range(0, total);
            if (chosen.Add(idx))
            {
                SlimeTile tile = board.GetTile(idx / w, idx % w);
                if (tile != null) tile.SetElement(element);
            }
        }
    }

    /// <summary>Reset lịch sử absorb (chơi lại Chapter).</summary>
    public void ResetAbsorbed()
    {
        absorbedElements.Clear();
    }
}
