using mshtml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sammelkarten.Utilities;
using Scryfall.API;
using Scryfall.API.Models;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace Sammelkarten {

    /// <summary>
    /// Interaktionslogik für SFBrowser.xaml
    /// </summary>
    public partial class SFBrowser : System.Windows.Controls.UserControl, INotifyPropertyChanged {

        #region Constructors

        public SFBrowser() {
            InitializeComponent();
            SearchViewModel.CardCountChanged += SearchViewModel_CardCountChanged;
            Browser.WebBrowserShortcutsEnabled = false;
            Browser.DocumentCompleted += Browser_DocumentCompleted;
            Browser.Navigate("https://scryfall.com/");
            EventHandler backevent = new EventHandler(backToolStripButton_Click);
            EventHandler forwardevent = new EventHandler(forwardToolStripButton_Click);
            _mbfilter = new MessageFilter(this, ref backevent, ref forwardevent);
            this.Loaded += HandleCreated;
            this.Unloaded += HandleDestroyed;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public bool IsAdding { get => isAdding; private set { isAdding = value; RaisePropertyChanged(); } }

        #endregion Properties

        #region Methods

        public void RaisePropertyChanged([CallerMemberName]string name = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion Methods

        #region Fields

        private static readonly HttpClient client = new HttpClient();

        private MessageFilter _mbfilter;

        private Dictionary<string, HtmlElement> saved_list_references = new Dictionary<string, HtmlElement>();

        private bool isAdding;

        private CardList lastpage = new CardList();

        #endregion Fields

        private void forwardToolStripButton_Click(object sender, EventArgs e) {
            if (Browser.CanGoForward) {
                Browser.GoForward();
            }
        }

        private void backToolStripButton_Click(object sender, EventArgs e) {
            if (Browser.CanGoBack) {
                Browser.GoBack();
            }
        }

        private void HandleDestroyed(object sender, EventArgs e) {
            Application.RemoveMessageFilter(_mbfilter);
        }

        private void HandleCreated(object sender, EventArgs e) {
            Application.AddMessageFilter(_mbfilter);
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            saved_list_references.Clear();

            //List Of Cards
            var myDiv = Browser.Document.GetElementsByTagName("DIV").OfType<HtmlElement>().Where(div => ((HTMLDivElement)div.DomElement).className == "card-grid-item");
            foreach (var item in myDiv) {
                AttachButtons(item, f => ((HTMLDivElement)f.DomElement).getAttribute("data-card-id").ToString());
            }

            //Single Card
            myDiv = Browser.Document.GetElementsByTagName("DIV").OfType<HtmlElement>().Where(div => ((HTMLDivElement)div.DomElement).className == "card-image-front");
            foreach (var item in myDiv) {
                AttachButtons(item, f => (Browser.Document.GetElementsByTagName("META").GetElementsByName("scryfall:card:id").OfType<HtmlElement>().FirstOrDefault()?.DomElement as IHTMLMetaElement)?.content);
            }
            var main = Browser.Document.GetElementsByTagName("DIV").OfType<HtmlElement>().FirstOrDefault(el => (el.DomElement as HTMLDivElement).className == "card-grid ");
            if (main != null) {
                // var div = main.Children[2];
                main.InsertAdjacentElement(HtmlElementInsertionOrientation.BeforeBegin, CreateHeaderDiv());
                main.AppendChild(CreateHeaderDiv());
            }
        }

        private HtmlElement CreateHeaderDiv() {
            var div = Browser.Document.CreateElement("DIV");
            div.Style = "display:flex; z-index:200; font-size:20px; flex-direction: row; justify-content: space-between; align-items: stretch";

            //Less Button
            var btnAddPage = Browser.Document.CreateElement("BUTTON");
            btnAddPage.InnerText = "Add Page";
            btnAddPage.Click += BtnAddPage_Click;
            div.AppendChild(btnAddPage);

            // More Button
            var btnAddFullSearch = Browser.Document.CreateElement("BUTTON");
            btnAddFullSearch.InnerText = "Add Full Search";
            btnAddFullSearch.Click += BtnAddFullSearch_Click;
            div.AppendChild(btnAddFullSearch);
            return div;
        }

        private void AttachButtons(HtmlElement item, Func<HtmlElement, string> action) {
            var cardId = action(item);
            if (cardId.IsNullOrWhiteSpace()) {
                return;
            }
            //LayoutDiv
            var div = Browser.Document.CreateElement("DIV");
            div.Style = "display:flex; font-size:20px; flex-direction: row; justify-content: space-between; align-items: stretch";

            //Less Button
            var btnLess = Browser.Document.CreateElement("BUTTON");
            btnLess.InnerText = "<<";
            btnLess.Id = cardId;
            btnLess.Click += BtnLess_Click;
            div.AppendChild(btnLess);

            // Text Field
            var plot = Browser.Document.CreateElement("P");
            plot.InnerText = CardCollection.Current.CardsToPrint.FirstOrDefault(c => c.Id == Guid.Parse(cardId))?.Count.ToString() ?? "0";
            div.AppendChild(plot);

            // More Button
            var btnMore = Browser.Document.CreateElement("BUTTON");
            btnMore.InnerText = ">>";
            btnMore.Id = cardId;
            btnMore.Click += BtnMore_Click;
            div.AppendChild(btnMore);

            item.AppendChild(div);

            //Add To SavedReferences

            saved_list_references.Add(cardId, plot);
        }

        private void BtnMore_Click(object sender, HtmlElementEventArgs e) {
            // var cardId = ((sender as HtmlElement)?.Parent.Parent.DomElement as HTMLDivElement)?.getAttribute("data-card-id");
            SearchViewModel.IncreaseCountCommand.Execute((sender as HtmlElement).Id);
        }

        private void BtnLess_Click(object sender, HtmlElementEventArgs e) {
            //var cardId = ((sender as HtmlElement)?.Parent.Parent.DomElement as HTMLDivElement)?.getAttribute("data-card-id");
            SearchViewModel.DecreaseCountCommand.Execute((sender as HtmlElement).Id);
        }

        private void SearchViewModel_CardCountChanged(Card card) {
            if (saved_list_references.ContainsKey(card.Id.ToString())) {
                var el = saved_list_references[card.Id.ToString()];
                el.InnerText = card.Count.ToString();
            }
        }

        private async void BtnAddFullSearch_Click(object sender, HtmlElementEventArgs e) {
            if (!IsAdding) {
                IsAdding = true;
                var textField = Browser.Document.GetElementById("header-search-field").DomElement as HTMLInputElement;
                if (!textField.value.IsNullOrWhiteSpace()) {
                    if (textField.value != CardCollection.Current.CurrentSearch?.SearchQuery) {
                        CardCollection.Current.CurrentSearch = await App.ScryfallClient.Cards.SearchAsync(textField.value);
                        await CardCollection.Current.CurrentSearch.AddAllPagesAsync();
                    }
                    else if (CardCollection.Current.CurrentSearch.HasMore.GetValueOrDefault()) {
                        await CardCollection.Current.CurrentSearch.AddAllPagesAsync();
                    }
                    foreach (var card in CardCollection.Current.CurrentSearch.Data.ToArray()) {
                        await SearchViewModel.OnIncreaseCount(card);
                    }
                }
                IsAdding = false;
            }
        }

        private async void BtnAddPage_Click(object sender, HtmlElementEventArgs e) {
            if (!IsAdding) {
                IsAdding = true;
                if (lastpage.SearchQuery != Browser.Url.ToString()) {
                    var myDiv = Browser.Document.GetElementsByTagName("DIV").OfType<HtmlElement>().Where(div => ((HTMLDivElement)div.DomElement).className == "card-grid-item");
                    var IdList = myDiv.Select(f => new JObject(new JProperty("id", ((HTMLDivElement)f.DomElement).getAttribute("data-card-id").ToString()))).ToArray();

                    var jsonObj = new JObject(new JProperty("identifiers", new JArray(IdList)));

                    var content = new StringContent(jsonObj.ToString(), Encoding.UTF8, "application/json");
                    var t = await client.PostAsync("https://api.scryfall.com/cards/collection", content);
                    if (t.StatusCode == HttpStatusCode.OK) {
                        var json = await t.Content.ReadAsStringAsync();
                        lastpage = JsonConvert.DeserializeObject<CardList>(json);
                        lastpage.SearchQuery = Browser.Url.ToString();
                        //cardList.TotalCards = cardList.Data.Count;
                        //cardList.HasMore = false;
                        //cardList.SearchQuery = textField.value;
                    }
                }

                foreach (var card in lastpage.Data) {
                    await SearchViewModel.OnIncreaseCount(card);
                }
                IsAdding = false;
            }
        }
    }
}