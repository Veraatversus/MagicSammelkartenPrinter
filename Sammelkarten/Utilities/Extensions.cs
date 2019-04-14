using Scryfall.API;
using Scryfall.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
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
            while (cardList.HasMore.GetValueOrDefault()) {
                await cardList.AddNextPageAsync();
                if (cancelToken?.IsCancelRequested ?? false) {
                    break;
                }
            }
            return cardList;
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