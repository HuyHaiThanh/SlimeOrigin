/// <summary>
/// 7 nguyên tố trong game + White (Slime trắng mặc định, chưa absorb element nào).
/// White luôn = 0 để board khởi tạo và absorb logic dễ phân biệt "chưa có element".
/// Thứ tự khớp bảng element trong CLAUDE.md mục 2.4.
/// </summary>
public enum ElementType
{
    White = 0,  // Slime trắng mặc định, không element
    Pyro,       // Đỏ cam  — damage chính
    Hydro,      // Xanh dương — heal + setup
    Cryo,       // Trắng xanh — freeze / slow
    Electro,    // Tím vàng — chain damage
    Dendro,     // Xanh lá — trap / grow
    Anemo,      // Xám bạc — spread element
    Geo         // Nâu vàng — shield / block
}
