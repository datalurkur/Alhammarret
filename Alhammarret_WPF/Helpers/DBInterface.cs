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
            "insert into settings (name, settingValue) values ('Canny Lower', 10)",
            "insert into settings (name, settingValue) values ('Canny Upper', 20)",
            "insert into settings (name, settingValue) values ('Canny Kernel', 3)",
            "insert into settings (name, settingValue) values ('Canny Blur', 3)",
            "insert into settings (name, settingValue) values ('Min Contour', 1000)",
            "insert into settings (name, settingValue) values ('Max Contour', 1500)",
            "insert into settings (name, settingValue) values ('Min Area', 70000)",
            "insert into settings (name, settingValue) values ('Max Area', 85000)",
            "insert into settings (name, settingValue) values ('Rotations', 0)"
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

        private static void CheckConnection()
        {
            if (connection == null)
            {
                string appDir = System.AppDomain.CurrentDomain.BaseDirectory;
                InitializeFromFile($"{appDir}\\test.sqlite");
            }
        }

        private static void SetupTables()
        {
            for (int i = 0; i < kSetupTables.Length; ++i)
            {
                SQLiteCommand cmd = new SQLiteCommand(kSetupTables[i], connection);
                cmd.ExecuteNonQuery();
            }

            // DEBUG
            SQLiteCommand cmd2 = new SQLiteCommand("select * from settings", connection);
            SQLiteDataReader rdr = cmd2.ExecuteReader();
            foreach (System.Collections.Specialized.NameValueCollection nvc in rdr.GetValues())
            {
                Console.WriteLine("Found nvc");
            }
        }

        public static int GetSetting(string name)
        {
            CheckConnection();
            SQLiteCommand cmd = new SQLiteCommand($"select settingValue from settings where name='{name}'", connection);
            return (int)cmd.ExecuteScalar();
        }
        public static void SetSetting(string name, int value)
        {
            CheckConnection();
            SQLiteCommand cmd = new SQLiteCommand($"update settings set settingValue={value} where name='{name}'", connection);
            cmd.ExecuteNonQuery();
        }
    }
}