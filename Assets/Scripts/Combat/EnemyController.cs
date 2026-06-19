using System;
using UnityEngine;

/// <summary>
/// Quản lý 1 quái trong combat: HP hiện tại, nhận damage, cung cấp sát thương mỗi lượt.
/// Khởi tạo từ EnemyData. Dùng event Action để decouple UI/Combat (nguyên tắc 3.3 conventions).
/// </summary>
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    public int CurrentHP { get; private set; }
    public int MaxHP => data != null ? data.maxHP : 0;
    public ElementType Element => data != null ? data.element : ElementType.White;
    public int AttackPower => data != null ? data.attackPerTurn : 0;
    public bool IsDead => CurrentHP <= 0;
    public EnemyData Data => data;

    public event Action<int, int> OnHPChanged;   // (current, max) — cho UI HP bar
    public event Action OnDeath;                  // báo Combat chuyển sang AbsorbChoice

    void Start()
    {
        if (data != null) Init(data);
    }

    /// <summary>Khởi tạo (hoặc reset) quái từ data — full HP.</summary>
    public void Init(EnemyData enemyData)
    {
        data = enemyData;
        CurrentHP = data.maxHP;
        OnHPChanged?.Invoke(CurrentHP, data.maxHP);
    }

    /// <summary>Nhận damage, clamp về [0, max], bắn event, kiểm tra chết.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        OnHPChanged?.Invoke(CurrentHP, MaxHP);
        if (CurrentHP == 0) OnDeath?.Invoke();
    }

    /// <summary>Hồi HP (cho boss có Regenerate, hoặc test). Không vượt max.</summary>
    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    /// <summary>Sát thương quái gây cho player trong lượt enemy.</summary>
    public int GetAttackDamage()
    {
        return AttackPower;
    }
}
