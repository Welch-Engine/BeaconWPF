using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BeaconWPF.Data
{
    public static class Constants
    {
        public const string SongDatabase = "Songs.db";
        public const string BibleDatabase = "Bible.db";
        public const string ArkDatabase = "Ark.db";
        public static string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Beacon");

        public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        public static bool exists = System.IO.Directory.Exists(basePath);

        public static string SongDbPath
        {
            get => Path.Combine(basePath, SongDatabase);
        }

        public static string BibleDbPath
        {
            get => Path.Combine(basePath, BibleDatabase);
        }

        public static string ArkDbPath
        {
            get => Path.Combine(basePath, ArkDatabase);
        }

        public static string webAPI = "https://ark.welchengine.com";
    }
}
