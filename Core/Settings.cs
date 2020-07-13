using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace TimeClock.Core
{
    public static class Settings
    {
        /// <summary>
        /// Name of the application.
        /// </summary>
        public const string APPLICATION_NAME = "Time Clock";

        public const string APPLICATION_NAME_SHORT = "TIMECLOCK";

        /// <summary>
        /// Name of the process.
        /// </summary>
        public const string APPLICATION_PROCESS_NAME = "TimeClock.exe";

        /// <summary>
        /// Registry key used by the application.
        /// </summary>
        public const string REGISTRY_KEY = "Software\\Memos\\eWay Outlook Client";

        /// <summary>
        /// Registry key used by the application.
        /// </summary>
        public const string TIMECLOCK_REGISTRY_KEY = "Software\\TimeClock";

        static Settings()
        {
            if (!Directory.Exists(ApplicationDataPath))
            {
                Directory.CreateDirectory(ApplicationDataPath);
            }

            UpdateInternetExplorerSettings();
        }

        private static void UpdateInternetExplorerSettings()
        {
            const string path = "Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_96DPI_PIXEL";
            var registry = Registry.CurrentUser.OpenSubKey(path, true);
            try
            {
                if (registry == null)
                {
                    registry = Registry.CurrentUser.CreateSubKey(path);
                }

                const string keyName = "TimeClock.exe";
                if (Convert.ToInt32(registry.GetValue(keyName)) == 1)
                    return;

                registry.SetValue(keyName, 1);
            }
            finally
            {
                if (registry != null)
                {
                    registry.Dispose();
                }
            }
        }

        /// <summary>
        /// Version of the client.
        /// </summary>
        public static string ClientVersion
        {
            get
            {
                return string.Format(
                    "{0}-{1}",
                    APPLICATION_NAME_SHORT,
                    System.Windows.Forms.Application.ProductVersion
                    );
            }
        }

        /// <summary>
        /// Application data path.
        /// </summary>
        public static string ApplicationDataPath
        {
            get
            {
                return System.IO.Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TimeClock"
                    );
            }
        }

        /// <summary>
        /// Indicates whether timer is started automatically when the application starts.
        /// </summary>
        public static bool AutoStart
        {
            get { return Convert.ToBoolean(GetTimeClockUserSetting("AutoStart", true)); }
            set { SaveTimeClockUserSetting("AutoStart", value); }
        }

        /// <summary>
        /// Indicates whether TimeClock uses eWay-CRM web service settings.
        /// </summary>
        public static bool ForceOwnWebService
        {
            get { return Convert.ToBoolean(GetTimeClockUserSetting("ForceOwnWebService", false)); }
            set { SaveTimeClockUserSetting("ForceOwnWebService", value); }
        }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public static string AccessToken
        {
            get { return (string)GetTimeClockUserSetting("AccessToken", null); }
            set { SaveTimeClockUserSetting("AccessToken", value); }
        }

        /// <summary>
        /// Get a setting of the user.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="defaultValue">Value used when the key does not exist.</param>
        /// <returns></returns>
        public static object GetUserSetting(string settingName, object defaultValue)
        {
            return GetRegistrySetting(Registry.CurrentUser, REGISTRY_KEY, settingName, defaultValue);
        }

        /// <summary>
        /// Gets a TimeClock user setting from registry.
        /// </summary>
        public static object GetTimeClockUserSetting(string settingName, object defaultValue)
        {
            return GetRegistrySetting(Registry.CurrentUser, TIMECLOCK_REGISTRY_KEY, settingName, defaultValue);
        }

        public static object TryGetUserSetting(string settingName, object defaultValue)
        {
            var timeClockWebServiceAddress = GetTimeClockUserSetting(settingName, defaultValue);
            if (Convert.ToBoolean(GetTimeClockUserSetting("ForceOwnWebService", false)))
                return timeClockWebServiceAddress;

            return GetUserSetting(settingName, null) ?? timeClockWebServiceAddress;
        }

        /// <summary>
        /// Get a setting of the user.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="path">Relative path to the key.</param>
        /// <param name="defaultValue">Value used when the key does not exist.</param>
        /// <returns></returns>
        public static object GetUserSetting(string settingName, string path, object defaultValue)
        {
            return GetRegistrySetting(Registry.CurrentUser, path, settingName, defaultValue);
        }

        /// <summary>
        /// Save a setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="value">Value to save.</param>
        /// <returns></returns>
        public static void SaveUserSetting(string settingName, object value)
        {
            SaveRegistrySetting(Registry.CurrentUser, REGISTRY_KEY, settingName, value);
        }

        /// <summary>
        /// Saves TimeClock user setting into the registry.
        /// </summary>
        public static void SaveTimeClockUserSetting(string settingName, object value)
        {
            SaveRegistrySetting(Registry.CurrentUser, TIMECLOCK_REGISTRY_KEY, settingName, value);
        }

        /// <summary>
        /// Save a setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="path">Relative path to the key.</param>
        /// <param name="value">Value to save.</param>
        /// <returns></returns>
        public static void SaveUserSetting(string settingName, string path, object value)
        {
            SaveRegistrySetting(Registry.CurrentUser, path, settingName, value);
        }

        /// <summary>
        /// Reads settings from registry.
        /// </summary>
        /// <param name="globalKey">Global key used when reading from registry (HKLM, HKCU, ...).</param>
        /// <param name="path">Relative path to the key.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="defaultValue">Default value, if the key does not exist.</param>
        /// <returns></returns>
        private static object GetRegistrySetting(RegistryKey globalKey, string path, string settingName,
            object defaultValue)
        {
            RegistryKey key = null;

            try
            {
                key = globalKey.OpenSubKey(path, false);

                if (key == null)
                    return defaultValue;

                object settingValue = key.GetValue(settingName);

                if (settingValue == null)
                    settingValue = defaultValue;

                return settingValue;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Cannot read the setting {0} from the registry", settingName), ex);
            }
            finally
            {
                if (key != null)
                    key.Close();
            }
        }

        /// <summary>
        /// Saves setting to registry based on the globalKey.
        /// </summary>
        /// <param name="globalKey">Global key used when reading from registry (HKLM, HKCU, ...).</param>
        /// <param name="path">Relative path to the key.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">Value, which will be stored in registry.</param>
        private static void SaveRegistrySetting(RegistryKey globalKey, string path, string settingName,
            object settingValue)
        {
            RegistryKey key = null;

            try
            {
                key = globalKey.OpenSubKey(path, true);

                // If the key does not exist, create it.
                if (key == null)
                    key = globalKey.CreateSubKey(path);

                if (settingValue != null)
                {
                    key.SetValue(settingName, settingValue);
                }
                else
                {
                    key.DeleteValue(settingName, false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Cannot save the setting '{0}' to the registry", settingName), ex);
            }
            finally
            {
                if (key != null)
                    key.Close();
            }
        }
    }
}
