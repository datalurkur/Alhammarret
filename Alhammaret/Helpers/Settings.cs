using System.Collections.Generic;
using Windows.Storage;

namespace Alhammaret
{
    public class Settings
    {
        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                }
                return instance;
            }
        }

        public Settings() { }

        public int Get(string name)
        {
            if (ApplicationData.Current.RoamingSettings.Containers.ContainsKey("alhammaretsettings"))
            {
                object val = ApplicationData.Current.RoamingSettings.Containers["alhammaretsettings"].Values[name];
                return val == null ? DefaultValue(name) : (int)val;
            }
            else
            {
                return DefaultValue(name);
            }
        }

        public string GetString(string name)
        {
            if (ApplicationData.Current.RoamingSettings.Containers.ContainsKey("alhammaretsettings"))
            {
                object val = ApplicationData.Current.RoamingSettings.Containers["alhammaretsettings"].Values[name];
                return val == null ? null : (string)val;
            }
            else
            {
                return null;
            }
        }

        public void Set(string name, int value)
        {
            ApplicationDataContainer settings = ApplicationData.Current.RoamingSettings.CreateContainer("alhammaretsettings", ApplicationDataCreateDisposition.Always);
            settings.Values[name] = value;
        }

        public void SetString(string name, string value)
        {
            ApplicationDataContainer settings = ApplicationData.Current.RoamingSettings.CreateContainer("alhammaretsettings", ApplicationDataCreateDisposition.Always);
            settings.Values[name] = value;
        }

        private int DefaultValue(string name)
        {
            switch (name)
            {
                case "Canny Lower": return 10;
                case "Canny Upper": return 20;
                case "Canny Kernel": return 3;
                case "Canny Blur": return 3;
                case "Min Contour": return 1000;
                case "Max Contour": return 1500;
                case "Min Area": return 70000;
                case "Max Area": return 85000;
                case "Rotations": return 0;
                default: return 0;
            }
        }
    }
}