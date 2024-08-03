using BeaconWPF.Data.Bibles;
using BeaconWPF.Data.Songs;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using System.Windows;

namespace BeaconWPF.Pages
{
    public partial class BibleLibrary
    {
        //TODO: UX for when there is a search Term and moving books or chapters.

        private List<Book> books = new List<Book>();
        private List<Chapter> chapters = new List<Chapter>();
        private List<BeaconVerse> verses = new List<BeaconVerse>();
        private List<BeaconVerse> doubleViewVerses = new List<BeaconVerse>();
        private List<BeaconVerse> searchedVerses = new List<BeaconVerse>();

        private List<Tuple<string, string>> bibles = new List<Tuple<string, string>>();
        private List<Tuple<int, string>>? versePortions { get; set; } = new List<Tuple<int, string>>();
        private Tuple<int, string>? selectedPortion { get; set; } = new Tuple<int, string>(0, "");

        private Book selectedBook = new Book();
        private Chapter selectedChapter = new Chapter();
        private BeaconVerse selectedVerse = new BeaconVerse();

        private string bookSearchTerm = "";
        private string chapterSearchTerm = "";
        private string verseIdSearchTerm = "";
        private string verseSearchTerm = "";
        private Tuple<string, string> selectedBible = new Tuple<string, string>("", "");
        private Tuple<string, string> selectedDoubleViewBible = new Tuple<string, string>("", "");
        private bool doubleViewMode;
        private bool showDoubleViewOptions;
        private bool isSearchingVerses = false;
        private bool showBookAndChapter = false;

        protected override async Task OnInitializedAsync()
        {
            var biblesFromDatabase = await bibleService.GetBiblesAsync().ConfigureAwait(false);
            foreach (var bible in biblesFromDatabase)
            {
                bibles.Add(new Tuple<string, string>(bible.Abbreviation, bible.Language));
            }

            if (bibles.Count() <= 1)
                showDoubleViewOptions = false;
            else
                showDoubleViewOptions = true;

            selectedBible = bibles[0];
            selectedDoubleViewBible = bibles[0];

            if (!String.IsNullOrWhiteSpace(selectedBible.Item1))
                books = await bibleService.GetBooksAsync(selectedBible.Item1).ConfigureAwait(false);


            //SESSION
            var sessionBook = await sessionStorage.GetItemAsync<Book>("selectedBook");
            var sessionChapter = await sessionStorage.GetItemAsync<Chapter>("selectedChapter");
            var sessionBible = await sessionStorage.GetItemAsync<Tuple<string, string>>("selectedBible");
            var sessionDoubleViewBible = await sessionStorage.GetItemAsync<Tuple<string, string>>("selectedDoubleViewBible");
            var sessiondoubleViewMode = await sessionStorage.GetItemAsync<bool>("doubleViewMode");
            var sessionVerseSearchTerm = await sessionStorage.GetItemAsync<string>("verseSearchTerm");

            if (sessionBible is not null) await OnBibleChange(sessionBible);
            if (sessionDoubleViewBible is not null) await OnDoubleViewBibleChange(sessionDoubleViewBible);
            if (sessionBook is not null) await OnBookChange(sessionBook);
            if (sessionChapter is not null) await OnChapterChange(sessionChapter);
            if (!String.IsNullOrWhiteSpace(sessionVerseSearchTerm))
            {
                verseSearchTerm = sessionVerseSearchTerm;
                await OnVerseTermSearch();
            }
            doubleViewMode = sessiondoubleViewMode;
        }

        public async void Dispose()
        {
            await sessionStorage.SetItemAsync<Book>("selectedBook", selectedBook);
            await sessionStorage.SetItemAsync<Chapter>("selectedChapter", selectedChapter);
            await sessionStorage.SetItemAsync<Tuple<string, string>>("selectedBible", selectedBible);
            await sessionStorage.SetItemAsync<Tuple<string, string>>("selectedDoubleViewBible", selectedDoubleViewBible);
            await sessionStorage.SetItemAsync<bool>("doubleViewMode", doubleViewMode);
            await sessionStorage.SetItemAsync<string>("verseSearchTerm", verseSearchTerm);
        }

        //[MAIN EVENTS]=====================================
        private async Task OnBibleChange(Tuple<string, string> bible)
        {
            if (bible == selectedBible) return;

            await GetBooksFromBible(bible.Item1).ConfigureAwait(false);
            selectedBible = bible;
            selectedVerse = new BeaconVerse();

            if (selectedBook.Id != 0)
                await GetChaptersFromBook(bible.Item1, selectedBook.Id).ConfigureAwait(false);

            if (selectedChapter.Id != 0)
                await GetVersesFromChapter(bible.Item1, selectedBook.Id, selectedChapter.Id).ConfigureAwait(false);

        }
        private async Task OnBookChange(Book book)
        {
            if (selectedBook.Id == book.Id)
                return;

            selectedBook = book;
            await GetChaptersFromBook(selectedBible.Item1, book.Id).ConfigureAwait(false);

            if (selectedChapter.Id != 0)
                await GetVersesFromChapter(selectedBible.Item1, book.Id, selectedChapter.Id).ConfigureAwait(false);

            if (doubleViewMode)
                await OnDoubleViewBibleChange(selectedDoubleViewBible).ConfigureAwait(false);

            chapterSearchTerm = "";
            verseIdSearchTerm = "";
            await jSRuntime.InvokeVoidAsync("ScrollWithDefinedSize", "ChapterList", book.Id - 1).ConfigureAwait(false);
        }
        private async Task OnChapterChange(Chapter chapter)
        {
            if (selectedChapter.Id == chapter.Id) return;

            selectedChapter = chapter;
            await GetVersesFromChapter(selectedBible.Item1, selectedBook.Id, chapter.Id);

            verseIdSearchTerm = "";
            await jSRuntime.InvokeVoidAsync("ScrollWithDefinedSize", "ChapterList", chapter.Id - 1).ConfigureAwait(false);
            if (!doubleViewMode) return;

            doubleViewVerses.Clear();
            doubleViewVerses = await bibleService.GetVersesAsync(selectedDoubleViewBible.Item1, selectedBook.Id, selectedChapter.Id).ConfigureAwait(false);
        }
        private async Task OnVerseChange(BeaconVerse verse, bool isMain)
        {
            selectedVerse = verse;

            versePortions!.Clear();
            verse!.Text = verse!.Text.RemoveHighlight();

            var portions = Regex.Split(verse.Text, @"(?<=[\.,;:!\?])\s+");
            for (int i = 1; i <= portions.Length; i++)
            {
                versePortions.Add(new Tuple<int, string>(i, portions[i - 1]));
            }
            selectedPortion = new Tuple<int, string>(0, "");

            await Task.Delay(1).ConfigureAwait(false);

            //HACKY SOLUTION BUT IT WORKS
            if (isMain)
            {
                await jSRuntime.InvokeVoidAsync("ScrollLogarithmically", "selected_Verse", "VerseList1").ConfigureAwait(false);
                await Task.Delay(300).ConfigureAwait(false);
                await jSRuntime.InvokeVoidAsync("ScrollLogarithmically", "selected_Verse", "VerseList2").ConfigureAwait(false);
            }
            else
            {
                await jSRuntime.InvokeVoidAsync("ScrollLogarithmically", "selected_Verse", "VerseList2").ConfigureAwait(false);
                await Task.Delay(300).ConfigureAwait(false);
                await jSRuntime.InvokeVoidAsync("ScrollLogarithmically", "selected_Verse", "VerseList1").ConfigureAwait(false);
            }

            //DISPLAY WINDOW
            await InvokeAsync(() =>
            {
                DisplayWindow.Instance.Show();
                DisplayWindow.Instance.Content.Text = verse.Text;
                DisplayWindow.Instance.Content.TextWrapping = TextWrapping.Wrap;
                DisplayWindow.Instance.Content.Width = 1920;
                DisplayWindow.Instance.Content.HighlightCount = 0;
                DisplayWindow.Instance.Header1.Text = selectedBook.Name.RemoveHighlight() + " " + selectedChapter.Id + ":" + verse.Verse;
                DisplayWindow.Instance.Header2.Text = isMain ? selectedBible.Item2.ToUpper() : selectedDoubleViewBible.Item2.ToUpper();

                if (verse.Text.Count() > 400)
                {
                    DisplayWindow.Instance.Content.FontSize = 70;
                }
                else if (verse.Text.Count() > 200)
                {
                    DisplayWindow.Instance.Content.FontSize = 85;
                }
                else
                {
                    DisplayWindow.Instance.Content.FontSize = 100;
                }


            }).ConfigureAwait(false);
        }

        private async Task OnSearchVerseClick(BeaconVerse verse)
        {
            isSearchingVerses = false;
            verseSearchTerm = "";
            await OnVerseChange(verse, false).ConfigureAwait(false);
        }

        private async Task OnVersePortionChange(Tuple<int, string> versePortion)
        {
            await InvokeAsync(() =>
            {
                DisplayWindow.Instance.Content.HighlightCount = versePortion.Item1;
                //DisplayWindow.Instance.Content.FontSize = 80;
            }).ConfigureAwait(false);
        }

        private async Task GetBooksFromBible(string bible = "")
        {
            books.Clear();
            if (String.IsNullOrWhiteSpace(bible))
                books = await bibleService.GetBooksAsync(selectedBible.Item1).ConfigureAwait(false);
            else
                books = await bibleService.GetBooksAsync(bible).ConfigureAwait(false);
        }
        private async Task GetChaptersFromBook(string bible, int book)
        {
            chapters.Clear();
            chapters = await bibleService.GetChaptersAsync(bible, book).ConfigureAwait(false);
        }
        private async Task GetVersesFromChapter(string bible, int book, int chapter)
        {
            verses.Clear();
            verses = await bibleService.GetVersesAsync(bible, book, chapter).ConfigureAwait(false);
        }

        private async Task OnBookInputQuery()
        {
            bookSearchTerm = Regex.Replace(bookSearchTerm, @"(\d)(\p{L})", "$1 $2");
            books = await bibleService.SearchBooksAsync(selectedBible.Item1, bookSearchTerm).ConfigureAwait(false);

            if (books.Count() == 1)
            {
                await OnBookChange(books[0]).ConfigureAwait(false);
                await jSRuntime.InvokeVoidAsync("SwitchFocusTo", "ChapterSearchInput").ConfigureAwait(false);
            }
        }
        private async Task OnChapterInputQuery()
        {
            int searchedChapterNumber;
            bool isNumber = Int32.TryParse(chapterSearchTerm, out searchedChapterNumber);

            if (isNumber && searchedChapterNumber > 0 && chapters.Count() > 0)
            {
                await OnChapterChange(chapters[Math.Min(searchedChapterNumber - 1, chapters.Count() - 1)]);
                await jSRuntime.InvokeVoidAsync("ScrollWithDefinedSize", "ChapterList", searchedChapterNumber).ConfigureAwait(false);
                await Task.Delay(100).ConfigureAwait(false);
                await jSRuntime.InvokeVoidAsync("SwitchFocusTo", "VerseSearchInput").ConfigureAwait(false);
            }
            else
            {
                chapterSearchTerm = "";
            }
        }
        private async Task OnVerseInputQuery()
        {
            int searchedVerseNumber;
            bool isNumber = Int32.TryParse(verseIdSearchTerm, out searchedVerseNumber);

            if (isNumber && searchedVerseNumber > 0 && verses.Count() > 0)
            {
                await OnVerseChange(verses[Math.Min(searchedVerseNumber - 1, verses.Count() - 1)], true);
                await Task.Delay(100).ConfigureAwait(false);
                await jSRuntime.InvokeVoidAsync("SwitchFocusTo", "VerseList1").ConfigureAwait(false);
            }
            else
            {
                verseIdSearchTerm = "";
            }
        }


        private async Task OnVerseTermSearch()
        {
            doubleViewMode = false;
            isSearchingVerses = !String.IsNullOrWhiteSpace(verseSearchTerm);
            showBookAndChapter = verseSearchTerm.StartsWith(".");

            if (verseSearchTerm.StartsWith("."))
                searchedVerses.Clear(); //TODO: Search entire bible. probably do 25 or 50 results only
            else
                searchedVerses = await bibleService.SearchVerseFromChapterAsync(selectedBible.Item1, selectedBook.Id, selectedChapter.Id, verseSearchTerm).ConfigureAwait(false);
        }

        //[DOUBLE VIEW EVENTS]=====================================
        private async Task OnDoubleViewModeToggle()
        {
            doubleViewMode = !doubleViewMode;
            
            if (doubleViewMode)
            {
                if(selectedDoubleViewBible == selectedBible && bibles.Count != 0)
                {
                    selectedDoubleViewBible = bibles.Where(b => b != selectedDoubleViewBible).ToList()!.FirstOrDefault()!;
                    await OnDoubleViewBibleChange(selectedDoubleViewBible).ConfigureAwait(false);
                }

                //doubleViewVerses.Clear();
                //doubleViewVerses = await bibleService.GetVersesAsync(selectedDoubleViewBible, selectedBook.Id, selectedChapter.Id).ConfigureAwait(false);
            }
        }
        private async Task OnDoubleViewBibleChange(Tuple<string, string> bible)
        {
            selectedDoubleViewBible = bible;
            doubleViewVerses.Clear();
            doubleViewVerses = await bibleService.GetVersesAsync(bible.Item1, selectedBook.Id, selectedChapter.Id).ConfigureAwait(false);
        }
        private async Task OnBookSearchInputKeyDown(KeyboardEventArgs e)
        {
            await Task.Delay(100).ConfigureAwait(false);

            if (e.Key == "Enter" && books.Count() != 0)
            {
                await OnBookChange(books[0]).ConfigureAwait(false);
                await jSRuntime.InvokeVoidAsync("SwitchFocusTo", "ChapterSearchInput").ConfigureAwait(false);
            }
        }
    }
}