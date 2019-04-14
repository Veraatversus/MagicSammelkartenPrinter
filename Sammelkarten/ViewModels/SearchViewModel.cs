using Microsoft.Win32;
using Scryfall.API;
using Scryfall.API.Models;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sammelkarten {

    public class SearchViewModel : INotifyPropertyChanged {

        #region Constructors

        static SearchViewModel() {
            IncreaseCountCommand = new AsyncRelayCommand<object>(OnIncreaseCount);
            DecreaseCountCommand = new AsyncRelayCommand<object>(OnDecreaseCount);
        }

        public SearchViewModel() {
            InitializeCommands();
        }

        #endregion Constructors

        #region Events

        public static event Action<Card> CardCountChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public static ICommand IncreaseCountCommand { get; private set; }
        public static ICommand DecreaseCountCommand { get; private set; }
        public static ImageTypeEnum ImageFormat { get; set; } = ImageTypeEnum.Normal;
        public CardsToDocService CardsToDoc { get; } = new CardsToDocService();
        public ICommand OpenFolderCommand { get; private set; }

        public ICommand OpenHomePageCommand { get; private set; }
        public ICommand StartPrintingCommand { get; private set; }
        public ICommand StopCommand { get; private set; }

        public ICommand LoadCollectionCommand { get; private set; }
        public ICommand SaveCollectionCommand { get; private set; }
        public bool IsLoadingPage { get => isLoadingPage; set { isLoadingPage = value; RaisePropertyChanged(); } }

        public string WatermarkSearch { get => _watermarkSearch; set { _watermarkSearch = value; RaisePropertyChanged(); } }

        #endregion Properties

        #region Methods

        public static async Task OnDecreaseCount(object obj) {
            if (obj is string cardId) {
                var tempCard = CardCollection.Current.CardsToPrint.FirstOrDefault(c => c.Id?.ToString() == cardId);
                if (tempCard != null) {
                    if (tempCard.Count > 0) {
                        tempCard.Count--;
                        CardCountChanged?.Invoke(tempCard);
                    }
                }
            }
            else if (obj is IEnumerable list) {
                foreach (var card in list.OfType<Card>()) {
                    if (card.Count > 0) {
                        card.Count--;
                        CardCountChanged?.Invoke(card);
                    }
                }
            }
            else if (obj is Card card) {
                if (card.Count > 0) {
                    card.Count--;
                    CardCountChanged?.Invoke(card);
                }
            }
        }

        public static async Task OnIncreaseCount(object obj) {
            if (obj is string cardId) {
                var tempCard = CardCollection.Current.CardsToPrint.FirstOrDefault(c => c.Id?.ToString() == cardId);
                if (tempCard == null) {
                    tempCard = await App.ScryfallClient.Cards.GetByIdAsync(Guid.Parse(cardId));
                }
                tempCard.Count++;
                CardCountChanged?.Invoke(tempCard);
            }
            else if (obj is IEnumerable list) {
                foreach (var card in list.OfType<Card>()) {
                    card.Count++;
                    CardCountChanged?.Invoke(card);
                }
            }
            else if (obj is Card card) {
                card.Count++;
                CardCountChanged?.Invoke(card);
            }
        }

        public async Task Search(string q, SortOrder order, SortDirection direction, SearchMode mode = SearchMode.Single) {
            IsLoadingPage = true;
            switch (mode) {
                //Full Search
                case SearchMode.Full:
                    //var service = new CardsToDocService();
                    //DataContext = service;
                    //await service.ProcessPicturesAsync(q);
                    break;

                //SingleSearch
                case SearchMode.Single:
                    if (!string.IsNullOrWhiteSpace(q)) {
                        try {
                            CardCollection.Current.CurrentSearch = await App.ScryfallClient.Cards.SearchAsync(q, order: order, dir: direction);
                        }
                        catch (Exception) {
                        }
                    }
                    break;

                default:
                    break;
            }
            IsLoadingPage = false;
        }

        public void OpenFolder() {
            if (Directory.Exists(CardCollection.Current.FolderPath)) {
                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CardCollection.Current.FolderPath));
            }
        }

        public async Task AddSearchPage() {
            IsLoadingPage = true;
            if (CardCollection.Current.CurrentSearch != null) {
                await CardCollection.Current.CurrentSearch.AddNextPageAsync();
            }

            IsLoadingPage = false;
        }

        public void RaisePropertyChanged([CallerMemberName]string name = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion Methods

        #region Fields

        private bool isLoadingPage;
        private string _watermarkSearch = "Enter Search here...";

        #endregion Fields

        private void InitializeCommands() {
            OpenFolderCommand = new RelayCommand(OpenFolder);
            OpenHomePageCommand = new RelayCommand(() => Process.Start("https://scryfall.com/"));
            StartPrintingCommand = new AsyncRelayCommand(async () => await Task.Run(async () => await CardsToDoc.ProcessPicturesAsync()));
            StopCommand = new RelayCommand(CardsToDoc.CancelToken.Cancel);
            LoadCollectionCommand = new RelayCommand(OnLoadCollection);
            SaveCollectionCommand = new RelayCommand(CardCollection.Save);
        }

        private void OnLoadCollection() {
            var dialog = new OpenFileDialog {
                AddExtension = true,
                DefaultExt = ".json",
                Multiselect = false,
                InitialDirectory = CardCollection.Current.FolderPath
            };
            if (dialog.ShowDialog() == true) {
                CardCollection.Load(dialog.FileName);
            }
        }
    }
}