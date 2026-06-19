using UnityEngine;

/// <summary>
/// Quản lý tiến trình 5 màn Chapter 1. Mỗi màn = 1 EnemyData (stats + introText + rewardSkill).
/// Data-driven qua Inspector (ScriptableObject) — gọn và an toàn hơn JSON cho MVP.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField] private EnemyData[] levels;

    public int CurrentIndex { get; private set; }
    public int LevelCount => levels != null ? levels.Length : 0;
    public EnemyData CurrentEnemy =>
        (levels != null && CurrentIndex >= 0 && CurrentIndex < levels.Length) ? levels[CurrentIndex] : null;
    public bool HasNextLevel => levels != null && CurrentIndex < levels.Length - 1;
    public bool IsLastLevel => levels != null && CurrentIndex >= levels.Length - 1;

    /// <summary>Sang màn kế. Trả false nếu đã hết màn (thắng Chapter).</summary>
    public bool AdvanceLevel()
    {
        if (HasNextLevel) { CurrentIndex++; return true; }
        return false;
    }

    /// <summary>Về màn đầu — chơi lại Chapter.</summary>
    public void ResetToFirst()
    {
        CurrentIndex = 0;
    }
}
