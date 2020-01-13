using Scryfall.API.Models;
using Syncfusion.Windows.PropertyGrid;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SortOrder = Scryfall.API.Models.SortOrder;

namespace Sammelkarten {

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        #region Constructors

        public MainWindow() {
            InitializeComponent();
            DataContext = ViewModel;
            Loaded += MainWindow_Loaded;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public SearchViewModel ViewModel { get; set; } = new SearchViewModel();

        #endregion Properties

        #region Methods

        public void RaisePropertyChanged([CallerMemberName]string name = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            var customEditor = new CustomEditor() { Editor = new ObjectEditor() };
            foreach (var item in typeof(Card).GetProperties().Select(f => f.Name)) {
                customEditor.Properties.Add(item);
            }
            PropGrid.CustomEditorCollection.Add(customEditor);
            PropGrid2.CustomEditorCollection.Add(customEditor);
            PropGrid.RefreshPropertygrid();
            PropGrid2.RefreshPropertygrid();
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e) {
            await Search();
        }

        private async Task Search() {
            await ViewModel.Search(tbSearch.Text, (SortOrder)cbSortOrder.SelectedItem, (SortDirection)cbSortDirection.SelectedItem);
        }

        private async void lvSearch_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e) {
            var scrollViewer = (ScrollViewer)e.OriginalSource;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight
                && scrollViewer.ScrollableHeight > 0
                && !ViewModel.IsLoadingPage) {
                await ViewModel.AddSearchPage();
            }
        }

        private async void tbSearch_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                await Search();
            }
        }

        #endregion Methods
    }
}