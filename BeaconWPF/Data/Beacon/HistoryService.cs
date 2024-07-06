using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaconWPF.Data.Beacon
{
    public class HistoryService : IHistoryService
    {
        private SQLiteConnection dbConnection;

        public HistoryService() 
        {
            dbConnection = new SQLiteConnection(Constants.BeaconDbPath, Constants.Flags);
            dbConnection.CreateTable<History>();
        }

        public List<History> GetAllHistoryDates() 
            => dbConnection.Query<History>("SELECT * FROM History GROUP BY Number");

        public List<History> GetHistoryFromNumber(int number)
        {
            try 
            {
                var historyFromNumber = dbConnection.Query<History>("SELECT * FROM History WHERE Number = ?", number);
                historyFromNumber.Reverse();
                return historyFromNumber;
            }
            catch (Exception ex) {
                return null;
            }
        }

        public void AddHistory(History history)
        {
            if(GetAllHistoryDates().Any())
            {
                DateTime latestHistoryDate = GetAllHistoryDates().Max(h => h.Date);
                int latestHistoryNumber = GetAllHistoryDates().Max(h => h.Number);

                if (latestHistoryDate < DateTime.Today)
                    history.Number = latestHistoryNumber + 1;
                else
                    history.Number = latestHistoryNumber;


                if (history.SongID <= -1)
                {
                    var bible = GetHistoryFromNumber(latestHistoryNumber).Find(h => h.BibleID == history.BibleID);
                    if (bible != null)
                    {
                        RemoveHistory(bible);
                    }
                }
                else
                {
                    var song = GetHistoryFromNumber(latestHistoryNumber).Find(h => h.SongID == history.SongID);
                    if (song is not null)
                    {
                        RemoveHistory(song);
                    }
                }

            }

            history.Date = DateTime.Today;
            dbConnection.Insert(history);
        }
        public void RemoveHistory(History history) => dbConnection.Delete(history);
        public void RemoveAllHistory()
        {
            try
            {
                dbConnection.DropTable<History>();
                dbConnection.CreateTable<History>();
                dbConnection.Execute("VACUUM");
            }
            catch (Exception ex) { }
        }
    }    

    public interface IHistoryService
    {
        public void AddHistory(History history);
        public void RemoveHistory(History history);
        public void RemoveAllHistory();
        public List<History> GetAllHistoryDates();
        public List<History> GetHistoryFromNumber(int number);
    }
}
