namespace ZenithOptimizer.Models
{
    public enum RiskLevel
    {
        Safe,       // Recommended for all users; 100% reversible, zero side effects
        Caution,    // 50/50: Advanced tweak with trade-offs (e.g. disables a secondary feature)
        Risky       // Advanced/High impact: Hardcore gamer / power user tweaks only
    }

    public enum OptimizationCategory
    {
        Windows11,
        Windows10,
        UniversalPrivacy,
        UniversalSystem,
        UniversalGaming,
        Bloatware,
        Maintenance
    }
}
