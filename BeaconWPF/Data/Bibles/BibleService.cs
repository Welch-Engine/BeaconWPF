using AsyncAwaitBestPractices;
using SQLite;
using System.Net.Http;
using System.Net.Http.Json;

namespace BeaconWPF.Data.Bibles
{
    public class BibleService : IBibleService
    {
        private SQLiteAsyncConnection dbConnection;
        private HttpClient httpClient;

        public BibleService(ICustomHttpFactory factory)
        {
            dbConnection = new SQLiteAsyncConnection(Constants.BibleDbPath, Constants.Flags);
            httpClient = factory.GetClient();
            CreateTable().SafeFireAndForget();
        }
        public async Task CreateTable() => await dbConnection.CreateTableAsync<Bible>().ConfigureAwait(false);

        public async Task<List<Book>> GetBooksAsync(string translation, bool useEnglish = false)
            => useEnglish ? await dbConnection.QueryAsync<Book>($"SELECT * FROM kjvBooks").ConfigureAwait(false) :
                            await dbConnection.QueryAsync<Book>($"SELECT * FROM {translation}Books").ConfigureAwait(false);

        public async Task<List<Chapter>> GetChaptersAsync(string translation, int book)
            => await dbConnection.QueryAsync<Chapter>($"SELECT DISTINCT Chapter FROM {translation} WHERE Book = {book}").ConfigureAwait(false);

        public async Task<List<BeaconVerse>> GetVersesAsync(string translation, int book, int chapter)
            => await dbConnection.QueryAsync<BeaconVerse>($"SELECT * FROM {translation} WHERE Book = {book} AND Chapter = {chapter}").ConfigureAwait(false);

        public async Task<List<Bible>> GetBiblesAsync() => await dbConnection.QueryAsync<Bible>("SELECT * FROM Bibles");

        public async Task DownloadBible(string translation)
        {
            //CHECK IF EXISTS
            bool languageExists = await dbConnection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Bibles WHERE Abbreviation = '{translation}'").ConfigureAwait(false) > 0;
            if (languageExists)
                return;

            //GET BIBLE FROM API
            var bible = await GetAPIBibleAsync(translation);

            //CREATE TABLE
            await dbConnection.ExecuteAsync($"CREATE VIRTUAL TABLE IF NOT EXISTS {translation} USING Fts5(Book, BookName, Chapter, Verse, Text)").ConfigureAwait(false);
            await dbConnection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS {translation}Books (Id INTEGER NOT NULL, Name TEXT NOT NULL)").ConfigureAwait(false);
            await dbConnection.InsertAsync(bible).ConfigureAwait(false);

            //CONVERT AND INSERT TO TABLE
            await ConvertAndSaveToDatabase(bible).ConfigureAwait(false);
            await dbConnection.ExecuteAsync("VACUUM").ConfigureAwait(false);
        }
        private async Task ConvertAndSaveToDatabase(Bible bible)
        {
            await dbConnection.RunInTransactionAsync(nonAsync =>
            {
                foreach (var book in bible.Books)
                {
                    nonAsync.Execute($"INSERT INTO {bible.Abbreviation}Books (Id, Name) VALUES (?, ?)", book.Id, book.Name);

                    foreach (var chapter in book.Chapters)
                    {
                        foreach(var verse in chapter.Verses)
                        {
                            nonAsync.Execute($"INSERT INTO {bible.Abbreviation} (Book, BookName, Chapter, Verse, Text) VALUES (?, ?, ?, ?, ?)",
                                                                                      book.Id, book.Name, chapter.Id, verse.Id, verse.Text);
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task<Bible> GetAPIBibleAsync(string translation)
        {
            HttpResponseMessage responseMessage = await httpClient.GetAsync($"https://api.getbible.net/v2/{translation}.json").ConfigureAwait(false);

            var bible = new Bible();
            if (responseMessage.IsSuccessStatusCode)
                bible = await responseMessage.Content.ReadFromJsonAsync<Bible>().ConfigureAwait(false);

            return bible!;
        }
    }

    public interface IBibleService
    {
        public Task<Bible> GetAPIBibleAsync(string translation);
        public Task<List<Bible>> GetBiblesAsync();
        public Task<List<Book>> GetBooksAsync(string translation, bool useEnglish = false);
        public Task<List<Chapter>> GetChaptersAsync(string translation, int book);
        public Task<List<BeaconVerse>> GetVersesAsync(string translation, int book, int chapter);
        public Task DownloadBible(string translation);
    }
}
