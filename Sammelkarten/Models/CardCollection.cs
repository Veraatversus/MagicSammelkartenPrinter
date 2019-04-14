using Newtonsoft.Json;
using Sammelkarten.Models;
using Scryfall.API.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Sammelkarten {

    public class CardCollection : ViewModelBase {

        #region Events

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion Events

        #region Properties

        public static int MaxPagesPerFile { get => _maxPagesPerFile; set { _maxPagesPerFile = value; NotifyStaticPropertyChanged(); } }

        public static CardCollection Current { get => s_currentCardCollection; set { s_currentCardCollection = value; NotifyStaticPropertyChanged(); } }

        [JsonIgnore]
        public string FolderPath => Path.Combine("Cards", Current.Name);

        public ObservableCardCollection CardsToPrint { get; } = new ObservableCardCollection();

        public CardList CurrentSearch { get => _currentSearch; set { _currentSearch = value; RaisePropertyChanged(); } }

        public string Name { get; set; } = "Cards";

        public int CountCards { get => _countCards; set { _countCards = value; RaisePropertyChanged(); } }

        //public string SaveLocation { get; set; }
        public int CountFaces { get => _countFaces; set { _countFaces = value; RaisePropertyChanged(); } }

        public int CountPrint {
            get => _countPrint;
            set {
                if (_countPrint != value) {
                    _countPrint = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CountPages));
                    RaisePropertyChanged(nameof(CountFiles));
                }
            }
        }

        public float CountPages => CountPrint / 9.0f;

        public float CountFiles => CountPages / MaxPagesPerFile;

        #endregion Properties

        #region Methods

        public static void Load(string filePath) {
            if (File.Exists(filePath)) {
                try {
                    using (var file = File.OpenText(filePath)) {
                        var serializer = new JsonSerializer();
                        Current = (CardCollection)serializer.Deserialize(file, typeof(CardCollection));
                    }
                    // var json = File.ReadAllText(filePath);
                    //collection= JsonConvert.DeserializeObject<CardCollection>(json);
                }
                catch (Exception) {
                }
            }
        }

        public static void Save() {
            //var json = JsonConvert.SerializeObject(this);
            //File.WriteAllText(Path.Combine(Current.FolderPath, Current.Name + ".json"), json);
            Directory.CreateDirectory(Current.FolderPath);
            using (var file = File.CreateText(Path.Combine(Current.FolderPath, Current.Name + ".json"))) {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, Current);
            }
        }

        public string GetFullFilePath(string index = "0") {
            return Path.Combine(FolderPath, $"{CardCollection.Current.Name}_{index}");
        }

        public void CardCountChanged(Card card, int newCount) {
            if (card.IsInCollection && newCount <= 0) {
                CardCollection.Current.CardsToPrint.Remove(card);
                CountCards--;
                var facecount = card.PrintImages.Count();
                CountFaces -= facecount;
                CountPrint -= card.Count * facecount;
            }
            else if (!card.IsInCollection && newCount >= 1) {
                var indexPrint = CardCollection.Current.CardsToPrint.IndexOf(card);
                if (indexPrint >= 0) {
                    var indexSearch = CardCollection.Current.CurrentSearch?.Data.IndexOf(card);
                    if (indexSearch.GetValueOrDefault(-1) >= 0) {
                        var realCard = CardCollection.Current.CardsToPrint[indexPrint];
                        CardCollection.Current.CurrentSearch.Data[indexSearch.Value] = realCard;
                    }
                }
                else {
                    CardCollection.Current.CardsToPrint.Add(card);
                    CountCards++;
                    var facecount = card.PrintImages.Count();
                    CountFaces += facecount;
                    CountPrint += newCount * facecount;
                }
            }
            else {
                var facecount = card.PrintImages.Count();
                CountPrint += facecount * (newCount - card.Count);
            }
        }

        #endregion Methods

        #region Fields

        private static int _maxPagesPerFile = 50;

        private static CardCollection s_currentCardCollection = new CardCollection();

        private CardList _currentSearch;

        private int _countCards;

        private int _countFaces;

        private int _countPrint;

        #endregion Fields

        private static void NotifyStaticPropertyChanged([CallerMemberName]string propertyName = null) {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}