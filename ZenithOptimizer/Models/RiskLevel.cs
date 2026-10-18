namespace ZenithOptimizer.Models
{
    public enum RiskLevel
    {
        Safe,       // Recommended for all users; 100% reversible, zero side effects
        Caution,    // 50/50: Trade-offs exist (e.g. disables secondary convenience feature like GameDVR/Search Indexing)
        Risky       // Advanced/High-impact: Competitive gaming or extreme system stripping
    }

    public enum OsTarget
    {
        Universal,  // Runs on both Windows 10 and 11
        Windows10,  // Windows 10 only (hidden automatically on Windows 11)
        Windows11   // Windows 11 only (hidden automatically on Windows 10)
    }

    public enum DeviceTarget
    {
        Universal,  // Both Laptops and Desktops
        LaptopOnly, // Laptop power & battery specific
        DesktopOnly // Desktop high-power / gaming specific
    }

    public enum OptimizationCategory
    {
        SystemPerformance,
        PrivacyTelemetry,
        GamingLatency,
        LaptopBattery,
        InterfaceErgonomics
    }
}
