using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using BussinessErp.Properties;

namespace BussinessErp.Helpers
{
    public static class LanguageManager
    {
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static string _currentLang = "en";

        public static bool IsArabic => _currentLang == "ar";
        public static string CurrentLanguage => _currentLang;

        public static event Action LanguageChanged;

        public static void Initialize()
        {
            string saved = Settings.Default.Language;
            if (string.IsNullOrEmpty(saved)) saved = "en";
            SetLanguage(saved, false);
        }

        public static void SetLanguage(string lang, bool save = true)
        {
            _currentLang = lang;
            LoadTranslations(lang);
            if (save)
            {
                Settings.Default.Language = lang;
                Settings.Default.Save();
            }
            LanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            if (_translations.ContainsKey(key))
                return _translations[key];
            return key; // Fallback to key itself
        }

        public static void ApplyRTL(Form form)
        {
            if (IsArabic)
            {
                form.RightToLeft = RightToLeft.Yes;
                form.RightToLeftLayout = true;
            }
            else
            {
                form.RightToLeft = RightToLeft.No;
                form.RightToLeftLayout = false;
            }
        }

        public static void ApplyRTL(Control control)
        {
            if (IsArabic)
                control.RightToLeft = RightToLeft.Yes;
            else
                control.RightToLeft = RightToLeft.No;
        }

        private static void LoadTranslations(string lang)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(baseDir, "Lang", lang + ".json");
                
                if (!File.Exists(filePath))
                {
                    // Try relative to project directory
                    filePath = Path.Combine(baseDir, "..", "..", "Lang", lang + ".json");
                }

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var serializer = new JavaScriptSerializer();
                    _translations = serializer.Deserialize<Dictionary<string, string>>(json);
                }
                else
                {
                    _translations = new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Failed to load language file: " + lang, ex);
                _translations = new Dictionary<string, string>();
            }
        }
    }
}
