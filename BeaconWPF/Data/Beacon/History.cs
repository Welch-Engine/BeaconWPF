using SQLite;

namespace BeaconWPF.Data.Beacon
{
    public class History
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = "";
        public int BibleID { get; set; }
        public int SongID { get; set; }
    }
}
