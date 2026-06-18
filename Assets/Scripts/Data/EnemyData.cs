using UnityEngine;

/// <summary>
/// Dữ liệu tĩnh của 1 loại quái (mục 2.8). Tạo asset qua menu
/// Create > SlimeOrigin > Enemy Data. Không hardcode số trong script (nguyên tắc 5).
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "SlimeOrigin/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Goblin";
    public ElementType element = ElementType.Pyro;
    public int maxHP = 30;
    public int attackPerTurn = 8;

    [Tooltip("Tên skill quái này dạy khi absorb — tuần 3 dùng.")]
    public string skillTaught = "Flare";

    [Tooltip("Sprite hiển thị quái — gán ở tuần 4, để trống cũng chạy được.")]
    public Sprite sprite;
}
