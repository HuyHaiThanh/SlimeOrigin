using System.Collections.Generic;

/// <summary>
/// Phát hiện elemental reaction từ tập element vừa match trong 1 đợt cascade (mục 2.6).
/// MVP: Vaporize (Pyro + Hydro) và Frozen (Hydro + Cryo). Thuần logic, CombatManager áp dụng effect.
/// </summary>
public static class ReactionSystem
{
    /// <summary>Vaporize: cùng đợt có cả Pyro và Hydro → damage nhân đôi.</summary>
    public static bool HasVaporize(Dictionary<ElementType, int> matched)
    {
        return matched != null
            && matched.ContainsKey(ElementType.Pyro)
            && matched.ContainsKey(ElementType.Hydro);
    }

    /// <summary>Frozen: cùng đợt có cả Hydro và Cryo → freeze enemy.</summary>
    public static bool HasFrozen(Dictionary<ElementType, int> matched)
    {
        return matched != null
            && matched.ContainsKey(ElementType.Hydro)
            && matched.ContainsKey(ElementType.Cryo);
    }
}
