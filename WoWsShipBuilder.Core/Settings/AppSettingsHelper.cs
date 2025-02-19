using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Settings
{
    [SuppressMessage("System.IO.Abstractions", "IO0002", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public static class AppSettingsHelper
    {
        private static readonly string SettingFile = Path.Combine(AppDataHelper.Instance.DefaultAppDataDirectory, "settings.json");

        public static void SaveSettings()
        {
            string settingString = JsonConvert.SerializeObject(AppData.Settings);
            File.WriteAllText(SettingFile, settingString);
        }

        public static void LoadSettings()
        {
            if (File.Exists(SettingFile))
            {
                Logging.Logger.Info("Trying to load settings from settings file...");
                string jsonSettings = File.ReadAllText(SettingFile);
                var settings = JsonConvert.DeserializeObject<AppSettings>(jsonSettings);
                if (settings == null)
                {
                    Logging.Logger.Error("Unable to parse local settings file. Creating empty settings instance.");
                    settings = new AppSettings();
                }

                AppData.Settings = settings;
            }
            else
            {
                Logging.Logger.Info("No settings file found, creating new settings...");
                AppData.Settings = new AppSettings();
            }

            AppData.IsInitialized = true;
            Logging.Logger.Info("Settings initialized.");
        }
    }
}
