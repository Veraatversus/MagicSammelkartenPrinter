using Scryfall.API;
using Scryfall.API.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sammelkarten {

    public class CardsToDocService : ViewModelBase {

        #region Constructors

        public CardsToDocService() {
            WordDoc = new WordDocument();
            WordDoc.OnFileFull += WordDoc_OnFileFull;
        }

        #endregion Constructors

        #region Properties

        public CancelToken CancelToken { get; } = new CancelToken();

        public Card CurrentCard { get => _currentCard; private set { _currentCard = value; RaisePropertyChanged(); } }

        public bool IsRunning { get => _IsRunning; set { _IsRunning = value; RaisePropertyChanged(); } }

        public WordDocument WordDoc { get => _wordDoc; private set { _wordDoc = value; RaisePropertyChanged(); } }

        //public ScryfallPage WebDoc { get => _webDoc; set { _webDoc = value; RaisePropertyChanged(); } }
        //public int CurrentPosition { get => _currentPosition; set { _currentPosition = value; RaisePropertyChanged(); } }

        public int FileIndex { get; set; }

        #endregion Properties

        #region Methods

        public async Task ProcessPicturesAsync(string uri = null) {
            IEnumerable<Card> CardsToPrint = CardCollection.Current.CardsToPrint;
            IsRunning = true;
            FileIndex = 1;
            // CurrentPosition = 0;

            // Full Search
            if (uri != null) {
                //Prüfe ob Link eingegeben ist
                if (string.IsNullOrWhiteSpace(uri)) {
                    MessageBox.Show("Bitte erst einen Suchlink eingeben.");
                    return;
                }
                var search = await App.ScryfallClient.Cards.SearchAsync(uri);
                CardsToPrint = search.Data;
                await search.AddAllPagesAsync(CancelToken);
            }

            if (Directory.Exists(CardCollection.Current.FolderPath)) {
                var files = Directory.EnumerateFiles(CardCollection.Current.FolderPath).Where(f => Path.GetFileName(f).StartsWith(CardCollection.Current.Name, System.StringComparison.Ordinal) && Path.GetExtension(f) == ".docx").ToList();
                foreach (var file in files) {
                    File.Delete(file);
                }
            }

            await WordDoc.AddPictureTightAsync(CardCollection.Current.CardsToPrint, CancelToken);

            if (CancelToken?.IsCancelRequested ?? false) {
                IsRunning = false;
                CancelToken.Reset();
                return;
            }
            // CurrentCard = card;
            WordDoc.SaveToFile(CardCollection.Current.GetFullFilePath(FileIndex.ToString()));
            //File.WriteAllText(Path.Combine(CardCollection.Current.FolderPath, CardCollection.Current.CollectionName + "_Search.txt"), CardCollection.Current.CurrentSearch.SearchQuery);
            IsRunning = false;
        }

        #endregion Methods

        #region Fields

        private Card _currentCard;

        private bool _IsRunning;

        private WordDocument _wordDoc;

        private readonly int _currentPosition;

        #endregion Fields

        private void WordDoc_OnFileFull(WordDocument obj) {
            obj.SaveToFile(CardCollection.Current.GetFullFilePath(FileIndex.ToString()));
            FileIndex++;
        }
    }
}