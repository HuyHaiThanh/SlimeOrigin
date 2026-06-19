using UnityEngine;

/// <summary>Loại hiệu ứng skill (MVP). Mở rộng dần ở tuần sau (reaction, AoE...).</summary>
public enum SkillEffectType
{
    DamageAll,   // gây damage cho enemy
    Heal,        // hồi HP player
    Freeze       // freeze enemy N lượt
}

/// <summary>
/// Dữ liệu 1 skill: kích hoạt khi tích lũy đủ tile của 1 element trong trận (mục 2.5).
/// Tạo asset qua Create > SlimeOrigin > Skill Data. Không hardcode số (nguyên tắc 5).
/// </summary>
[CreateAssetMenu(fileName = "SkillData", menuName = "SlimeOrigin/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName = "Flare";
    public ElementType requiredElement = ElementType.Pyro;

    [Tooltip("Số tile element này cần tích lũy để kích hoạt.")]
    public int requiredCount = 10;

    public SkillEffectType effectType = SkillEffectType.DamageAll;

    [Tooltip("Damage / HP hồi / số lượt freeze — tùy effectType.")]
    public int effectValue = 20;
}
