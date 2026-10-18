using System;
using System.Windows;
using ZenithOptimizer.Services;

namespace ZenithOptimizer
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Startup Error: {args.Exception.Message}\n\n{args.Exception.StackTrace}", "Zenith Optimizer Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            try
            {
                // Apply initial Cupertino Dark Theme with signature Zenith Purple
                ThemeManager.ApplyTheme(AppTheme.CupertinoDark, AccentColor.RoyalViolet);

                // Explicitly instantiate and show MainWindow
                var mainWindow = new MainWindow();
                this.MainWindow = mainWindow;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization Error: {ex.Message}\n\n{ex.StackTrace}", "Zenith Optimizer Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
