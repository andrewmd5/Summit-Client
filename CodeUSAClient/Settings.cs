using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeUSAClient
{
    public class Settings
    {
        private static List<Setting> settings = new List<Setting>();
        private static List<Setting> themeSettings;

        static Settings()
        {
            settings.Add(Setting.Construct("Hiscores.AutoComplete", true));
            settings.Add(Setting.Construct("Client.AutoUpdate", true));
            settings.Add(Setting.Construct("Client.EnableDebug", true));
            settings.Add(Setting.Construct("Login.RememberLoginDetails", true));
            settings.Add(Setting.Construct("Login.Username", "", true));
            settings.Add(Setting.Construct("Login.Password", "", true));
            settings.Add(Setting.Construct("Screenshot.ImageDirectory", "screenshots"));
            settings.Add(Setting.Construct("Screenshot.PreviewBeforeUpload", true));
            settings.Add(Setting.Construct("Screenshot.DontSave", false));
            settings.Add(Setting.Construct("Screenshot.Font", "Tahoma"));
            settings.Add(Setting.Construct("Screenshot.FontSize", 10f));
            settings.Add(Setting.Construct("MusicPlayer.PlayFolder", "music"));
            settings.Add(Setting.Construct("MusicPlayer.StreamURL", "http://"));
            settings.Add(Setting.Construct("Resolution.Width", 0x4a6));
        }

        public static string[] GetHeads()
        {
            var list = new List<string>();
            foreach (var setting in settings)
            {
                var item = setting.name.Substring(0, setting.name.IndexOf("."));
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        public static Setting GetSetting(string name)
        {
            foreach (var setting in settings)
            {
                if (setting.name.ToLower() == name.ToLower())
                {
                    return setting;
                }
            }
            return null;
        }

        public static List<Setting> GetSettings()
        {
            return settings;
        }

        public static List<Setting> GetSettings(string header)
        {
            var list = new List<Setting>();
            foreach (var setting in settings)
            {
                if (setting.name.ToLower().StartsWith(header.ToLower()))
                {
                    list.Add(setting);
                }
            }
            return list;
        }

        public static Setting[] GetSettingsForHead(string head)
        {
            var list = new List<Setting>();
            foreach (var setting in GetSettings())
            {
                var str = setting.name.Substring(0, setting.name.IndexOf("."));
                if (head == str)
                {
                    list.Add(setting);
                }
            }
            return list.ToArray();
        }

        public static T GetValue<T>(string name)
        {
            foreach (var setting in settings)
            {
                if (setting.name.ToLower() == name.ToLower())
                {
                    return (T) setting.value;
                }
            }
            return default(T);
        }

        public static T GetValueTrusted<T>(string name)
        {
            foreach (var setting in settings)
            {
                if (setting.name.ToLower() == name.ToLower())
                {
                    return (T) setting.heldValue;
                }
            }
            return default(T);
        }

        public static void Load()
        {
            var type = typeof (UISystem.UITheme);
            foreach (var info in type.GetFields())
            {
                if (GetSetting("Theme." + info.Name) == null)
                {
                    if (info.FieldType == typeof (SolidBrush))
                    {
                        var brush = (SolidBrush) info.GetValue(null);
                        settings.Add(new Setting("Theme." + info.Name, brush.Color, typeof (Color)));
                    }
                    else if (info.FieldType == typeof (Pen))
                    {
                        var pen = (Pen) info.GetValue(null);
                        settings.Add(new Setting("Theme." + info.Name, pen.Color, typeof (Color)));
                    }
                    else if (info.FieldType == typeof (Color))
                    {
                        var color = (Color) info.GetValue(null);
                        settings.Add(new Setting("Theme." + info.Name, color, typeof (Color)));
                    }
                }
            }
            if (!File.Exists("./Settings.cfg"))
            {
                Save();
            }
            else
            {
                var strArray = File.ReadAllLines("./Settings.cfg");
                var index = 0;
                while (++index < strArray.Length)
                {
                    var str = strArray[index];
                    if (str.Contains("="))
                    {
                        var strArray2 = str.Split(new[] {'='});
                        if (strArray2.Length == 6)
                        {
                            var name = str.Substring(0, str.IndexOf("="));
                            var str3 = str.Substring(str.IndexOf("=") + 1);
                            SetValue(name, str3);
                        }
                        else
                        {
                            var str4 = strArray2[1];
                            if ((GetSetting(strArray2[0]) != null) && GetSetting(strArray2[0]).hide)
                            {
                                str4 = Encryption.Decrypt(str4.Replace(",", "="));
                            }
                            SetValue(strArray2[0], str4);
                        }
                    }
                }
                foreach (var setting in GetSettingsForHead("Theme"))
                {
                    foreach (var info2 in type.GetFields())
                    {
                        if (info2.Name.ToLower() == setting.name.Replace("Theme.", "").ToLower())
                        {
                            if (info2.FieldType == typeof (SolidBrush))
                            {
                                var brush2 = new SolidBrush((Color) setting.value);
                                info2.SetValue(null, brush2);
                            }
                            else if (info2.FieldType == typeof (Pen))
                            {
                                var pen2 = new Pen((Color) setting.value);
                                info2.SetValue(null, pen2);
                            }
                            else if (info2.FieldType == typeof (Color))
                            {
                                info2.SetValue(null, (Color) setting.value);
                            }
                        }
                    }
                }
            }
        }

        public static void Save()
        {
            var builder = new StringBuilder();
            foreach (var str in GetHeads())
            {
                builder.AppendLine("[" + str + "]");
                foreach (var setting in GetSettingsForHead(str))
                {
                    var obj2 = setting.value;
                    if (obj2 == null)
                    {
                        obj2 = "";
                    }
                    if (setting.hide)
                    {
                        obj2 = Encryption.Encrypt(setting.heldValue.ToString()).Replace("=", ",");
                    }
                    builder.AppendLine(setting.name + "=" + obj2);
                }
                builder.AppendLine("");
            }
            File.WriteAllText("./Settings.cfg", builder.ToString());
        }

        public static void SetValue(string name, string value)
        {
            var setting = GetSetting(name);
            if (setting != null)
            {
                if (setting.type == typeof (Color))
                {
                    if (Regex.Match(value, "Color [A=[0-9]+, R=[0-9]+, G=[0-9]+, B=[0-9]+]").Success)
                    {
                        var strArray = value.Replace("Color [", "").Replace("]", "").Split(new[] {'=', ','});
                        setting.value = Color.FromArgb(int.Parse(strArray[1]), int.Parse(strArray[3]),
                            int.Parse(strArray[5]), int.Parse(strArray[7]));
                    }
                    else
                    {
                        setting.value = Color.FromName(value);
                    }
                }
                else
                {
                    setting.value = Convert.ChangeType(value, setting.type);
                }
            }
        }

        #region Nested type: DefaultGenerator

        public class DefaultGenerator
        {
            public static object GetDefaultValue(Type parameter)
            {
                return typeof (DefaultGenerator<>).MakeGenericType(new[] {parameter})
                    .InvokeMember("GetDefault", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                        null, null, new object[0]);
            }
        }

        public class DefaultGenerator<T>
        {
            public static T GetDefault()
            {
                return default(T);
            }
        }

        #endregion

        #region Nested type: Setting

        public class Setting
        {
            public object heldValue;
            public bool hide;
            public string name;
            public Type type;

            public Setting(string name, object value, Type type)
            {
                this.name = name;
                heldValue = value;
                this.type = type;
            }

            public object value
            {
                get
                {
                    if (!hide)
                    {
                        return heldValue;
                    }
                    return "";
                }
                set { heldValue = value; }
            }

            public static Setting Construct<T>(string name, T value)
            {
                return new Setting(name, value, typeof (T));
            }

            public static Setting Construct<T>(string name, T value, bool hideData)
            {
                return new Setting(name, value, typeof (T)) {hide = hideData};
            }
        }

        #endregion
    }
}