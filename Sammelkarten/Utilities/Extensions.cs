using Scryfall.API;
using Scryfall.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sammelkarten {

  public static class Extensions {

    #region Methods

    public static void Foreach<T>(this IEnumerable<T> list, Action<T> action) {
      foreach (var item in list) {
        action(item);
        //yield return item;
      }
    }

    public static async Task<CardList> AddNextPageAsync(this CardList cardList) {
      if (cardList.HasMore.GetValueOrDefault()) {
        var search = await App.ScryfallClient.Cards.SearchAsync(cardList.SearchQuery, page: cardList.NextPageInt);
        cardList.HasMore = search.HasMore;
        cardList.TotalCards = search.TotalCards;
        cardList.SearchQuery = search.SearchQuery;
        cardList.NextPageInt = search.NextPageInt;
        cardList.NextPage = search.NextPage;
        foreach (var card in search.Data) {
          cardList.Data.Add(card);
        }
      }
      return cardList;
    }

    public static async Task<CardList> AddAllPagesAsync(this CardList cardList, CancelToken cancelToken = default) {
      var w = new Window();
      w.Title = "Adding Pages To Card Collection...";
      var MyStackPanel = new StackPanel();
      var MyPercentProgress = new TextBlock();
      var pb = new ProgressBar();
      pb.Width = 200;
      pb.Height = 40;
      pb.Minimum = 0;
      MyStackPanel.Children.Add(MyPercentProgress);
      MyStackPanel.Children.Add(pb);
      w.Content = MyStackPanel;
      w.Width = 400;
      w.Height = 120;
      w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      w.Closed += W_Closed;
      w.Show();
      var maxpages =cardList.CardsProPage == 0 ? 1 : ((cardList.TotalCards.GetValueOrDefault(0) - cardList.Data.Count) / cardList.CardsProPage) + 1;
      pb.Maximum = maxpages;
      var pagecount = 0;
      IsSearching = true;
      while (cardList.HasMore.GetValueOrDefault()) {
        if (cancelToken?.IsCancelRequested ?? false || !IsSearching) {
          break;
        }
        await cardList.AddNextPageAsync();
        if (w != null) {
          pb.Value = pagecount++;
          MyPercentProgress.Text = $"Added Page {pagecount} from {maxpages}";
        }
      }
      w?.Close();
      IsSearching = false;
      return cardList;
    }
    public static bool IsSearching { get; set; }
    private static void W_Closed(object sender, EventArgs e) {
      IsSearching = false;
    }

    /// <summary>
    /// Gets a descendant by type at the given visual.
    /// </summary>
    /// <param name="visual">The visual that contains the descendant.</param>
    /// <param name="type">The type to search for the descendant.</param>
    /// <returns>Returns the searched descendant or null if nothing was found.</returns>
    public static Visual GetDescendantByType(this Visual visual, Type type) {
      if (visual == null) {
        return null;
      }

      if (visual.GetType() == type) {
        return visual;
      }

      // sometimes it's necessary to apply a template before getting the childrens
      if (visual is FrameworkElement frameworkElement) {
        frameworkElement.ApplyTemplate();
      }

      Visual foundElement = null;
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++) {
        var childVisual = VisualTreeHelper.GetChild(visual, i) as Visual;
        foundElement = GetDescendantByType(childVisual, type);
        if (foundElement != null) {
          break;
        }
      }
      return foundElement;
    }

    #endregion Methods
  }
}