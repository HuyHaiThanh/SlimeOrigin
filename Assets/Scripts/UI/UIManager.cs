using System.Collections;
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
            combat.OnEnemyDamaged += ShowEnemyDamage;
            combat.OnReaction += ShowReaction;
            UpdatePlayerHP(combat.PlayerHP, combat.PlayerMaxHP);
        }
        if (skills != null)
            skills.OnCounterChanged += UpdateCounter;

        RefreshCounterText();
    }

    void OnDestroy()
    {
        if (enemy != null) enemy.OnHPChanged -= UpdateEnemyHP;
        if (combat != null)
        {
            combat.OnPlayerHPChanged -= UpdatePlayerHP;
            combat.OnEnemyDamaged -= ShowEnemyDamage;
            combat.OnReaction -= ShowReaction;
        }
        if (skills != null) skills.OnCounterChanged -= UpdateCounter;
    }

    private void UpdateEnemyHP(int cur, int max)
    {
        if (enemyHPFill != null) enemyHPFill.fillAmount = max > 0 ? (float)cur / max : 0f;
        if (enemyHPText != null) enemyHPText.text = cur + " / " + max;
        // Cập nhật tên quái mỗi lần (đổi màn init enemy mới -> fire OnHPChanged).
        if (enemyNameText != null && enemy != null && enemy.Data != null)
            enemyNameText.text = enemy.Data.enemyName;
    }

    /// <summary>Hien so damage bay len + mo dan tren khu vuc enemy.</summary>
    public void ShowEnemyDamage(int amount)
    {
        StartCoroutine(DamagePopupRoutine(amount));
    }

    private IEnumerator DamagePopupRoutine(int amount)
    {
        var go = new GameObject("DamagePopup", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = "-" + amount;
        t.fontSize = 96;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(1f, 0.9f, 0.15f, 1f);
        t.enableWordWrapping = false;
        if (enemyHPText != null && enemyHPText.font != null) t.font = enemyHPText.font;
        go.transform.SetAsLastSibling();   // ve tren cung

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(400, 130);
        float x = Random.Range(-120f, 120f);
        float startY = -250f;

        float dur = 1.4f, tt = 0f;
        while (tt < dur)
        {
            tt += Time.deltaTime;
            float k = tt / dur;
            rt.anchoredPosition = new Vector2(x, startY + 180f * k);   // bay len
            t.color = new Color(1f, 0.9f, 0.15f, 1f - k * k);          // giu sang lau roi mo nhanh
            yield return null;
        }
        Destroy(go);
    }

    /// <summary>Hien ten reaction (Vaporize/Frozen) giua man, bay len + mo dan.</summary>
    public void ShowReaction(string reactionName)
    {
        StartCoroutine(ReactionPopupRoutine(reactionName));
    }

    private IEnumerator ReactionPopupRoutine(string reactionName)
    {
        var go = new GameObject("ReactionPopup", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = reactionName;
        t.fontSize = 84;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.5f, 0.95f, 1f, 1f);
        t.enableWordWrapping = false;
        if (enemyHPText != null && enemyHPText.font != null) t.font = enemyHPText.font;
        go.transform.SetAsLastSibling();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(700, 140);
        float startY = 120f;

        float dur = 1.3f, tt = 0f;
        while (tt < dur)
        {
            tt += Time.deltaTime;
            float k = tt / dur;
            rt.anchoredPosition = new Vector2(0f, startY + 120f * k);
            t.color = new Color(0.5f, 0.95f, 1f, 1f - k * k);
            yield return null;
        }
        Destroy(go);
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
