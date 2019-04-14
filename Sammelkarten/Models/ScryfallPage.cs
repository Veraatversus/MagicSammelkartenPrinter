using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;

namespace Sammelkarten {

    public class ScryfallPage : ViewModelBase, INotifyPropertyChanged {

        #region Constructors

        public ScryfallPage(string uri) {
            SearchUri = uri;
        }

        #endregion Constructors

        #region Properties

        public static HttpClient NetClient { get; set; } = new HttpClient();
        public int BilderAnzahl { get => _BilderAnzahl; set { _BilderAnzahl = value; RaisePropertyChanged(); } }
        public string SearchUri { get; set; }

        #endregion Properties

        #region Methods

        public IEnumerable<Card> Cards() {
            BilderAnzahl = 0;

            //Lade Webseite herunter
            var WebDoc = LoadDocument(SearchUri);

            //Setze Anzahl der Bilder
            var DivAnzahl = WebDoc.DocumentNode.Descendants("div").FirstOrDefault(div => div.GetAttributeValue("class", null) == "search-info")?.Descendants("strong").FirstOrDefault();
            var GesamtAnzahl = Convert.ToInt32(DivAnzahl.ChildNodes.LastOrDefault().InnerText.Replace("cards", "").Replace(",", "").Trim());
            var StartAnzahl = Convert.ToInt32(DivAnzahl.ChildNodes.FirstOrDefault().InnerText.Split('–').FirstOrDefault().Trim());
            BilderAnzahl = GesamtAnzahl - (StartAnzahl - 1);

            //Suche div das die Bilder enthält

            var CardGridInner = SucheBilderListe(WebDoc);
            //Solange noch Seiten zum Blättern vorhanden sind
            while (CardGridInner != null) {
                //Durchlaufe Bilder
                foreach (var div in CardGridInner.Elements("div")) {
                    //Hole Bild quelle.
                    var image = div.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", null);

                    //Hole Eventuelle alternativ quelle
                    if (string.IsNullOrWhiteSpace(image)) {
                        image = div.Descendants("img").FirstOrDefault()?.GetAttributeValue("data-src", null);
                    }

                    //Überspringe karten ohne bilder und aller letzten spacer
                    if (string.IsNullOrWhiteSpace(image)) {
                        continue;
                    }

                    //Erstelle Karte
                    var newCard = new Card {
                        Name = div.Descendants("span").FirstOrDefault()?.InnerText,
                        Uri = image
                    };
                    yield return newCard;
                }

                //Setze Bilder Liste zurück
                CardGridInner = null;

                //Suche Weiter Button zum Blättern
                var nextbtn = WebDoc.DocumentNode.Descendants("div").FirstOrDefault(a => a.GetAttributeValue("class", null) == "search-controls-pagination").ChildNodes.Where(child => child.Name != "#text").ElementAt(2);
                var btnText = nextbtn.GetAttributeValue("class", null);

                //Prüfe ob weiter Seite verfügbar ist
                if (!string.IsNullOrWhiteSpace(btnText) && btnText.StartsWith("button-n") && !btnText.Contains("disabled")) {
                    //Setze neue Seite und Bilderliste
                    var newlink = nextbtn.GetAttributeValue("href", null);
                    WebDoc = LoadDocument("https://scryfall.com" + newlink);
                    CardGridInner = SucheBilderListe(WebDoc);
                }
            }
        }

        #endregion Methods

        #region Fields

        private int _BilderAnzahl;

        #endregion Fields

        private static HtmlNode SucheBilderListe(HtmlDocument WebDoc) {
            return WebDoc.DocumentNode.Descendants("div").FirstOrDefault(div => div.GetAttributeValue("class", null) == "card-grid-inner");
        }

        private HtmlDocument LoadDocument(string uri) {
            //Lade Webseite herunter
            var HtmlString = NetClient.GetStringAsync(uri).GetAwaiter().GetResult();

            //Lade Seite in HtmlDocument
            var WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HtmlString);
            return WebDoc;
        }
    }
}