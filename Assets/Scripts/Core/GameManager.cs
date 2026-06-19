using UnityEngine;

/// <summary>
/// State machine điều phối Chapter 1 (mục 3.5): LevelIntro → Combat → AbsorbChoice
/// → LevelComplete → (màn kế / ChapterComplete) ; GameOver khi player chết.
/// Player HP + board GIỮ NGUYÊN giữa các màn (mục 2.9); skill counter reset mỗi trận.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { LevelIntro, Combat, AbsorbChoice, LevelComplete, GameOver, ChapterComplete }

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private CombatManager combat;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private SkillSystem skillSystem;
    [SerializeField] private AbsorbSystem absorbSystem;
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private SwapHandler swapHandler;
    [SerializeField] private GamePanelUI panel;

    public GameState State { get; private set; }

    void Start()
    {
        if (combat != null)
        {
            combat.OnEnemyDefeated += HandleEnemyDefeated;
            combat.OnGameOver += HandleGameOver;
        }
        StartLevel();
    }

    void OnDestroy()
    {
        if (combat != null)
        {
            combat.OnEnemyDefeated -= HandleEnemyDefeated;
            combat.OnGameOver -= HandleGameOver;
        }
    }

    /// <summary>Bắt đầu màn hiện tại: nạp quái, reset counter, hiện intro.</summary>
    private void StartLevel()
    {
        State = GameState.LevelIntro;
        if (swapHandler != null) swapHandler.InputEnabled = false;

        EnemyData data = levelManager != null ? levelManager.CurrentEnemy : null;
        if (data == null) { ChapterComplete(); return; }

        if (enemy != null) enemy.Init(data);
        if (skillSystem != null) skillSystem.ResetCounters();   // counter theo từng trận

        string intro = "Màn " + (levelManager.CurrentIndex + 1) + "/" + levelManager.LevelCount
            + "\n\n" + data.introText;
        if (panel != null) panel.ShowMessage(intro, "Bắt đầu", BeginCombat);
        else BeginCombat();
    }

    private void BeginCombat()
    {
        State = GameState.Combat;
        if (panel != null) panel.Hide();
        if (swapHandler != null) swapHandler.InputEnabled = true;
    }

    private void HandleEnemyDefeated()
    {
        State = GameState.AbsorbChoice;
        if (swapHandler != null) swapHandler.InputEnabled = false;
        string nm = (enemy != null && enemy.Data != null) ? enemy.Data.enemyName : "quái";
        if (panel != null)
            panel.ShowMessage("Đã đánh bại " + nm + "!\nAbsorb để học element + skill?",
                "Absorb", AbsorbYes, "Bỏ qua", AbsorbNo);
        else AbsorbNo();
    }

    private void AbsorbYes()
    {
        EnemyData data = enemy != null ? enemy.Data : null;
        if (absorbSystem != null && data != null) absorbSystem.Absorb(data);

        if (panel != null) panel.ShowMessage("...", "", null);   // màn chờ trong lúc gọi dialogue
        if (dialogue != null && data != null)
        {
            dialogue.RequestDialogue(data.enemyName, data.element, line =>
            {
                if (panel != null) panel.ShowMessage("“" + line + "”", "Tiếp tục", AfterAbsorb);
                else AfterAbsorb();
            });
        }
        else AfterAbsorb();
    }

    private void AbsorbNo()
    {
        AfterAbsorb();
    }

    private void AfterAbsorb()
    {
        State = GameState.LevelComplete;
        if (levelManager != null && levelManager.HasNextLevel)
        {
            levelManager.AdvanceLevel();
            StartLevel();
        }
        else ChapterComplete();
    }

    private void ChapterComplete()
    {
        State = GameState.ChapterComplete;
        if (swapHandler != null) swapHandler.InputEnabled = false;
        if (panel != null)
            panel.ShowMessage("HOÀN THÀNH CHAPTER 1!\nSlime của bạn đã tiến hóa.", "Chơi lại", RestartChapter);
    }

    private void HandleGameOver()
    {
        State = GameState.GameOver;
        if (swapHandler != null) swapHandler.InputEnabled = false;
        if (panel != null)
            panel.ShowMessage("Bạn đã ngã xuống...\nThử lại nhé?", "Chơi lại", RestartChapter);
    }

    private void RestartChapter()
    {
        if (levelManager != null) levelManager.ResetToFirst();
        if (absorbSystem != null) absorbSystem.ResetAbsorbed();
        if (combat != null) combat.ResetCombat();   // reset player HP + board + counter
        StartLevel();
    }
}
