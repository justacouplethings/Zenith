# Zenith Optimizer

> **High-Precision Windows 10/11 Performance Suite**  
> Heavily inspired by Chris Titus Tech's Windows Utility (WinUtil), refactored into a high-performance, Apple-designed standalone desktop application.

---

## ⚡ Key Highlights

- **Zero-Dependency Portable EXE**: Shipped as a self-contained single-file binary (`ZenithOptimizer.exe`). Runs out of the box on Windows 10 and Windows 11 with no prerequisite runtimes.
- **Apple Human Interface Design**: Minimalist typography, subtle card elevation, Cupertino Dark, Cupertino Light, Midnight Slate, and Graphite Minimal themes with dynamic accent color customization.
- **Live Hardware Telemetry Engine**: Continuous non-blocking hardware monitoring for CPU load, RAM allocation, disk active time, process count, system uptime, and laptop battery/power status.
- **Granular Risk Classification**:
  - 🟢 **SAFE**: Recommended for daily use. Completely reversible with zero negative side effects on standard user workflows.
  - 🟡 **50/50 (CAUTION)**: High-performance tweaks that involve trade-offs (e.g. disabling GameDVR, Xbox services, or search indexing).
  - 🔴 **RISKY**: High-impact modifications for competitive gaming or extreme stripping (Ultimate Performance power plan, kernel locked in RAM, large system cache).
- **Dedicated OS Architecture Tabs**: Distinct optimization suites for Windows 11 and Windows 10, plus Universal privacy, system, and gaming tweaks.
- **One-Click Presets**:
  - `Safe Productivity`: Instant privacy and responsiveness tuning for daily drivers.
  - `Pro Gaming`: Latency reduction, MMCSS tuning, GameDVR removal, and 1:1 raw mouse input.
  - `Maximum Strip`: Unlocks the hidden Ultimate Performance ACPI power scheme and strips non-essential services.
  - `Revert All`: Resets selected tweaks back to Microsoft default configurations.
- **System Restore Point Integration**: Automated Windows restore point creation with verification.
- **UWP Bloatware & Cache Cleaner**: One-click purge for pre-installed consumer apps (Candy Crush, Xbox, Bing News, Spotify, Disney+) and temporary cache/prefetch scrubber.

---

## 🚀 Quick Start

1. Double-click **`ZenithOptimizer.exe`** (requires Administrator elevation to apply system registry and service changes).
2. Click **Create Restore Point** on the top header if you wish to capture an initial system snapshot.
3. Select a preset (e.g. **Safe Productivity** or **Pro Gaming**) or toggle custom optimizations under the **Windows 11**, **Windows 10**, or **Universal** tabs.
4. Click **Apply Selected** to commit changes.
5. Click **Restart Explorer** to refresh the shell for taskbar and context menu changes.

---

## 🎨 Custom Branding & Logo

To replace the default placeholder logo with your own brand mark:
- Place your image in the same directory as `ZenithOptimizer.exe` named **`logo.png`** or **`logo.ico`**.
- Zenith Optimizer will automatically load and display your custom logo upon launch.

---

## 📂 Project Structure

```
Optimizer/
├── ZenithOptimizer.exe          # Standalone portable application (Ready to run)
├── logo.png                     # Brand logo placeholder (Replace with your custom logo)
├── README.md                    # Documentation
└── ZenithOptimizer/             # Full source code
    ├── ZenithOptimizer.csproj   # .NET 10 WPF project definition (Single-file configured)
    ├── app.manifest             # Administrator elevation & PerMonitorV2 DPI manifest
    ├── App.xaml / .cs           # Apple design system brushes & theme engine
    ├── MainWindow.xaml / .cs    # Responsive Apple-style dashboard & segmented controls
    ├── Models/                  # Data models (OptimizationItem, SystemMetrics, RiskLevel)
    ├── Converters/              # XAML UI value converters
    ├── Services/                # Core engines (HardwareMonitor, Registry, Services, Debloat)
    └── ViewModels/              # MainViewModel (MVVM bindings, presets, logs)
```
