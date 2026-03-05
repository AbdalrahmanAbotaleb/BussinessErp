using System;
using System.Windows.Forms;
using BussinessErp.Helpers;
using BussinessErp.UI.Forms;
using BussinessErp.Properties;

namespace BussinessErp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize language/localization
            LanguageManager.Initialize();

            // Global exception handlers
            Application.ThreadException += (s, e) =>
            {
                AppLogger.Error("Unhandled UI exception", e.Exception);
                MessageBox.Show("An unexpected error occurred.\n" + e.Exception.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                AppLogger.Error("Unhandled domain exception", ex);
            };

            // Show splash screen — performs all async DB/service initialization internally.
            // It fades in, runs init in background, then fades out before closing.
            using (var splash = new frmSplashScreen())
            {
                Application.Run(splash);
            }

            // Onboarding - Shown only on first launch
            AppLogger.Info("Checking Onboarding status. IsFirstLaunch: " + Settings.Default.IsFirstLaunch);
            
            // NOTE: FORCING TO TRUE so you can see the onboarding screen now.
            // You can comment this out once you are satisfied with the result.
            Settings.Default.IsFirstLaunch = true; 

            if (Settings.Default.IsFirstLaunch)
            {
                AppLogger.Info("Starting Onboarding flow...");
                using (var onboarding = new frmOnboarding())
                {
                    if (onboarding.ShowDialog() != DialogResult.OK)
                    {
                        AppLogger.Info("Application closed from onboarding.");
                        return;
                    }
                }
            }

            // Login / main application loop
            while (true)
            {
                using (var loginForm = new frmLogin())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        AppLogger.Info("Application closed from login.");
                        return;
                    }
                }

                using (var mainForm = new frmMain())
                {
                    Application.Run(mainForm);

                    if (mainForm.DialogResult != DialogResult.Retry)
                    {
                        AppLogger.Info("Application closed.");
                        return;
                    }
                    // DialogResult.Retry = logout → loop back to login
                }
            }
        }
    }
}
