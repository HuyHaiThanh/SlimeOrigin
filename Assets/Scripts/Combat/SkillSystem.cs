using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Đếm số tile mỗi element player đã match trong trận; kích hoạt skill khi đạt ngưỡng.
/// Không cooldown (mục 2.5). Effect cụ thể do CombatManager xử lý qua event OnSkillActivated
/// để giữ tách bạch (SkillSystem không biết về Enemy/Player).
/// </summary>
public class SkillSystem : MonoBehaviour
{
    [SerializeField] private List<SkillData> unlockedSkills = new List<SkillData>();

    // Đếm tile tích lũy theo element trong trận hiện tại.
    private readonly Dictionary<ElementType, int> counters = new Dictionary<ElementType, int>();

    public event Action<SkillData> OnSkillActivated;
    public event Action<ElementType, int> OnCounterChanged;  // (element, giá trị mới) — cho UI

    /// <summary>Cộng số tile vừa match của 1 element rồi kiểm tra skill.</summary>
    public void AddMatchedTiles(ElementType element, int count)
    {
        if (count <= 0) return;
        if (!counters.ContainsKey(element)) counters[element] = 0;
        counters[element] += count;
        OnCounterChanged?.Invoke(element, counters[element]);
        CheckSkills();
    }

    /// <summary>Mở khóa skill mới — gọi khi absorb (tuần 3).</summary>
    public void UnlockSkill(SkillData skill)
    {
        if (skill != null && !unlockedSkills.Contains(skill))
            unlockedSkills.Add(skill);
    }

    /// <summary>Reset toàn bộ counter — đầu mỗi trận.</summary>
    /// <summary>Reset toàn bộ counter — đầu mỗi trận. Bắn event 0 để UI cập nhật.</summary>
    public void ResetCounters()
    {
        var keys = new List<ElementType>(counters.Keys);
        counters.Clear();
        foreach (var e in keys) OnCounterChanged?.Invoke(e, 0);
    }

    public int GetCounter(ElementType element)
    {
        return counters.ContainsKey(element) ? counters[element] : 0;
    }

    /// <summary>
    /// Skill nào đủ ngưỡng thì kích hoạt và trừ đi đúng ngưỡng (cho phép tái kích hoạt).
    /// Dùng while để xử lý trường hợp 1 lần match vượt nhiều lần ngưỡng.
    /// </summary>
    private void CheckSkills()
    {
        foreach (SkillData skill in unlockedSkills)
        {
            if (skill == null || skill.requiredCount <= 0) continue;
            while (GetCounter(skill.requiredElement) >= skill.requiredCount)
            {
                counters[skill.requiredElement] -= skill.requiredCount;
                OnCounterChanged?.Invoke(skill.requiredElement, counters[skill.requiredElement]);
                OnSkillActivated?.Invoke(skill);
            }
        }
    }
}
