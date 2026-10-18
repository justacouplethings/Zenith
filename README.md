# Zenith Optimizer

> **High-Precision Windows 10/11 Performance Suite**  
> Heavily based on Chris Titus Tech's WinUtil, redesigned into an intelligent, Apple-inspired native desktop application with hardware-driven algorithmic optimizations.

---

## ⚡ What's New in This Version

- **Automatic OS Detection & Unified Tweaks**: Zenith automatically identifies whether you are running Windows 10 or Windows 11. Incompatible tweaks are hidden dynamically—giving you one streamlined **Tweaks** tab tailored specifically to your operating system.
- **Zenith Smart Hardware Audit Engine**: An algorithmic audit engine that inspects your CPU core topology, RAM capacity, NVMe/SSD storage, and device chassis to calculate and select the ideal optimizations automatically.
- **Laptop & Desktop Hardware Profiling**:
  - `💻 Laptop Profile`: Modern Standby sleep battery drain fix, WiFi power droop prevention, energy-efficient CPU throttling, and background telemetry suspension.
  - `🖥️ Desktop Profile`: High-throughput gaming latency tuning, MMCSS packet prioritization, and 1:1 raw mouse tracking.
- **Friendly App Debloater**: Completely redesigned with human-readable categories (Pre-installed Games & Ads, News & Weather, Feedback & Help, Video Tools, Partner Promo Stubs, Xbox Gaming Apps). Every app can be reinstalled from the Microsoft Store at any time.
- **In-App Safety Guardrails & Checklist**:
  - Direct Restore Point capture with live status verification.
  - Granular Risk Badges (🟢 Safe, 🟡 50/50 Caution, 🔴 Risky).
  - Safety confirmation prompts before applying any high-impact or 50/50 tweaks.
- **Clean Native Windows Typography**: Standardized on clean, crisp `Segoe UI` and `Segoe UI Variable Text` typography—eliminating artificial letter-spacing and gimmicks.
- **Signature Zenith Purple Branding**: Custom logo auto-loader tailored for the purple `logo.png` brandmark.

---

## 🚀 Quick Start

1. Close any older instance and launch **`ZenithOptimizer.exe`** as Administrator.
2. Check the **System Safety Shield** on the Dashboard and click **Create Restore Point**.
3. Select your hardware profile (`💻 Laptop` vs `🖥️ Desktop`) or click **⚡ Run Smart Audit & Select Best Tweaks** to let the algorithm analyze your machine and choose the optimal configuration.
4. Review selected items under the **Tweaks** tab or **Debloat Apps** tab.
5. Click **Apply Selected** (or **Uninstall Selected Apps**) to commit changes.
6. Click **Restart Explorer** to refresh the taskbar and context menus.

---

## 📂 Project Architecture

```
Optimizer/
├── ZenithOptimizer.exe          # Standalone portable application (Ready to run)
├── logo.png                     # Custom purple Zenith brand logo
├── README.md                    # Documentation
└── ZenithOptimizer/             # Full source code
    ├── ZenithOptimizer.csproj   # .NET 10 WPF project definition (Single-file configured)
    ├── app.manifest             # Administrator elevation & PerMonitorV2 DPI manifest
    ├── App.xaml / .cs           # Theme engine & window startup dispatch
    ├── MainWindow.xaml / .cs    # Responsive Apple-style dashboard & unified tweaks
    ├── Models/                  # Data models (OptimizationItem, SystemMetrics, RiskLevel)
    ├── Converters/              # Value converters
    ├── Services/
    │   ├── SmartAuditEngine.cs  # Algorithmic hardware analyzer
    │   ├── DebloatService.cs    # Friendly UWP bloatware remover
    │   ├── HardwareMonitor.cs   # Win32 P/Invoke telemetry engine
    │   ├── OptimizationDB.cs    # Comprehensive Windows 10/11 & Laptop database
    │   ├── RegistryHelper.cs    # Safe registry modification engine
    │   ├── ServiceHelper.cs     # Windows service configuration
    │   └── ThemeManager.cs      # Cupertino Dark/Light & Zenith Purple palettes
    └── ViewModels/              # MainViewModel (MVVM bindings, audit, presets)
```
