using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lớp hiển thị HUD: HP bar enemy + player, counter element. Thuần view — chỉ subscribe
/// event của CombatManager/EnemyController/SkillSystem rồi cập nhật UI, không chứa logic game.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CombatManager combat;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private SkillSystem skills;

    [Header("Enemy UI")]
    [SerializeField] private Image enemyHPFill;
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private TMP_Text enemyNameText;

    [Header("Player UI")]
    [SerializeField] private Image playerHPFill;
    [SerializeField] private TMP_Text playerHPText;

    [Header("Counter UI")]
    [SerializeField] private TMP_Text counterText;

    private readonly Dictionary<ElementType, int> counters = new Dictionary<ElementType, int>();

    void Start()
    {
        if (enemy != null)
        {
            enemy.OnHPChanged += UpdateEnemyHP;
            if (enemyNameText != null && enemy.Data != null) enemyNameText.text = enemy.Data.enemyName;
            UpdateEnemyHP(enemy.CurrentHP, enemy.MaxHP);
        }
        if (combat != null)
        {
            combat.OnPlayerHPChanged += UpdatePlayerHP;
            UpdatePlayerHP(combat.PlayerHP, combat.PlayerMaxHP);
        }
        if (skills != null)
            skills.OnCounterChanged += UpdateCounter;

        RefreshCounterText();
    }

    void OnDestroy()
    {
        if (enemy != null) enemy.OnHPChanged -= UpdateEnemyHP;
        if (combat != null) combat.OnPlayerHPChanged -= UpdatePlayerHP;
        if (skills != null) skills.OnCounterChanged -= UpdateCounter;
    }

    private void UpdateEnemyHP(int cur, int max)
    {
        if (enemyHPFill != null) enemyHPFill.fillAmount = max > 0 ? (float)cur / max : 0f;
        if (enemyHPText != null) enemyHPText.text = cur + " / " + max;
    }

    private void UpdatePlayerHP(int cur, int max)
    {
        if (playerHPFill != null) playerHPFill.fillAmount = max > 0 ? (float)cur / max : 0f;
        if (playerHPText != null) playerHPText.text = cur + " / " + max;
    }

    private void UpdateCounter(ElementType element, int value)
    {
        counters[element] = value;
        RefreshCounterText();
    }

    private void RefreshCounterText()
    {
        if (counterText == null) return;
        counterText.text = "Pyro " + Get(ElementType.Pyro)
            + "    Hydro " + Get(ElementType.Hydro)
            + "    Cryo " + Get(ElementType.Cryo);
    }

    private int Get(ElementType e)
    {
        return counters.ContainsKey(e) ? counters[e] : 0;
    }
}
