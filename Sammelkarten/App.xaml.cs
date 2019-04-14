using Scryfall.API;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Sammelkarten {

    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {

        #region Properties

        public static HttpClient NetClient { get; } = new HttpClient();

        public static ScryfallClient ScryfallClient { get; } = new ScryfallClient();

        #endregion Properties

        #region Methods

        private void Maingrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var grid = (sender as Grid);
            var lb = grid.FindVisualParent<ListBox>();
            if (lb.SelectedItems.Count == 1) {
                lb.SelectedItems.Clear();
            }
            Selector.SetIsSelected(grid.FindVisualParent<ListBoxItem>(), true);
        }

        #endregion Methods
    }
}