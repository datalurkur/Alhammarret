using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;

namespace Alhammarret
{
    public class DBInterface
    {
        private static readonly string[] kSetupTables = new string[]
        {
            // Settings
            "create table settings (name varchar(32), settingValue int)",
            "insert into settings (name, settingValue) values ('cannyLower', 10)",
            "insert into settings (name, settingValue) values ('cannyUpper', 20)",
            "insert into settings (name, settingValue) values ('cannyKernel', 3)",
            "insert into settings (name, settingValue) values ('cannyBlur', 3)",
            "insert into settings (name, settingValue) values ('minContour', 1000)",
            "insert into settings (name, settingValue) values ('maxContour', 1500)",
            "insert into settings (name, settingValue) values ('minArea', 70000)",
            "insert into settings (name, settingValue) values ('maxArea', 85000)",
            "insert into settings (name, settingValue) values ('rotations', 0)"
        };

        private static SQLiteConnection connection = null;

        public static void InitializeFromFile(string pathToDatabaseFile)
        {
            if (connection != null)
            {
                Teardown();
            }

            bool needsInit = false;
            if (!File.Exists(pathToDatabaseFile))
            {
                needsInit = true;
                SQLiteConnection.CreateFile(pathToDatabaseFile);
            }
            connection = new SQLiteConnection($"Data Source={pathToDatabaseFile};Version=3");
            connection.Open();
            if (needsInit)
            {
                SetupTables();
            }
        }

        public static void Teardown()
        {
            connection.Close();
            connection = null;
        }

        private static void SetupTables()
        {
            for (int i = 0; i < kSetupTables.Length; ++i)
            {
                SQLiteCommand cmd = new SQLiteCommand(kSetupTables[i], connection);
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetSetting(string name)
        {
            SQLiteCommand cmd = new SQLiteCommand($"select settingValue from settings where name='{name}'");
            return (int)cmd.ExecuteScalar();
        }
        public static void SetSetting(string name, int value)
        {
            SQLiteCommand cmd = new SQLiteCommand($"update settings set settingValue={value} where name='{name}'");
            cmd.ExecuteNonQuery();
        }
    }
}