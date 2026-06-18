using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điều phối vòng lượt combat (mục 3.6). Lắng nghe swap từ SwapHandler:
/// validate → resolve cascade (tính damage + feed skill counter) → đánh enemy →
/// lượt enemy đánh lại. Giữ HP player. Effect skill áp dụng qua SkillSystem.OnSkillActivated.
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BoardManager board;
    [SerializeField] private SwapHandler swapHandler;
    [SerializeField] private SkillSystem skillSystem;
    [SerializeField] private EnemyController enemy;

    [Header("Player (mục 2.9)")]
    [SerializeField] private int playerMaxHP = 100;

    [Header("Damage config (mục 2.9)")]
    [SerializeField] private int baseMatchDamage = 5;     // damage mỗi match-3
    [SerializeField] private int bonusPerExtraTile = 2;   // +mỗi tile vượt 3

    public int PlayerHP { get; private set; }
    public int PlayerMaxHP => playerMaxHP;

    private int enemyFreezeTurns = 0;

    public event Action<int, int> OnPlayerHPChanged;   // (cur,max) — cho UI
    public event Action OnEnemyDefeated;               // → AbsorbChoice (GameManager xử lý)
    public event Action OnGameOver;

    void Start()
    {
        PlayerHP = playerMaxHP;
        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);

        if (skillSystem != null) skillSystem.OnSkillActivated += ApplySkill;
        if (swapHandler != null) swapHandler.OnSwapRequested += HandleSwap;
    }

    void OnDestroy()
    {
        if (skillSystem != null) skillSystem.OnSkillActivated -= ApplySkill;
        if (swapHandler != null) swapHandler.OnSwapRequested -= HandleSwap;
    }

    /// <summary>1 lượt player: swap → nếu không tạo match thì hoàn tác (không tốn lượt).</summary>
    private void HandleSwap(int r1, int c1, int r2, int c2)
    {
        board.SwapTiles(r1, c1, r2, c2);
        if (!MatchDetector.HasAnyMatch(board))
        {
            board.SwapTiles(r1, c1, r2, c2); // không match → trả lại, không qua lượt enemy
            return;
        }

        int damage = ResolveCascades();
        if (enemy != null) enemy.TakeDamage(damage);

        if (enemy != null && enemy.IsDead)
        {
            if (swapHandler != null) swapHandler.InputEnabled = false;
            OnEnemyDefeated?.Invoke();
            return;
        }

        EnemyTurn();
    }

    /// <summary>
    /// Lặp: tìm match → cộng damage (5 + (n-3)×2 mỗi đợt) + feed skill counter theo element
    /// → gravity/refill → đến khi hết match. Trả tổng damage gây cho enemy.
    /// </summary>
    private int ResolveCascades()
    {
        int totalDamage = 0;
        HashSet<SlimeTile> matches = MatchDetector.FindMatches(board);

        while (matches.Count > 0)
        {
            int n = matches.Count;
            totalDamage += baseMatchDamage + (n - 3) * bonusPerExtraTile;

            // Đếm tile theo element TRƯỚC khi gravity xóa chúng, rồi feed skill.
            if (skillSystem != null)
            {
                var perElement = new Dictionary<ElementType, int>();
                foreach (SlimeTile tile in matches)
                {
                    ElementType e = tile.Element;
                    perElement[e] = (perElement.ContainsKey(e) ? perElement[e] : 0) + 1;
                }
                foreach (var kv in perElement)
                    skillSystem.AddMatchedTiles(kv.Key, kv.Value);
            }

            GravitySystem.ApplyGravityAndRefill(board, matches);
            matches = MatchDetector.FindMatches(board);
        }

        return totalDamage;
    }

    /// <summary>Lượt enemy: nếu đang freeze thì bỏ lượt, ngược lại đánh player.</summary>
    private void EnemyTurn()
    {
        if (enemy == null) return;

        if (enemyFreezeTurns > 0)
        {
            enemyFreezeTurns--;
            return;
        }

        int dmg = enemy.GetAttackDamage();
        PlayerHP = Mathf.Max(0, PlayerHP - dmg);
        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);

        if (PlayerHP == 0)
        {
            if (swapHandler != null) swapHandler.InputEnabled = false;
            OnGameOver?.Invoke();
        }
    }

    /// <summary>Áp dụng hiệu ứng 1 skill vừa kích hoạt (gọi từ SkillSystem.OnSkillActivated).</summary>
    private void ApplySkill(SkillData skill)
    {
        switch (skill.effectType)
        {
            case SkillEffectType.DamageAll:
                if (enemy != null) enemy.TakeDamage(skill.effectValue);
                break;

            case SkillEffectType.Heal:
                PlayerHP = Mathf.Min(playerMaxHP, PlayerHP + skill.effectValue);
                OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);
                break;

            case SkillEffectType.Freeze:
                enemyFreezeTurns += skill.effectValue;
                break;
        }
    }
}
