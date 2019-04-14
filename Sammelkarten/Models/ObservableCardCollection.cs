using Scryfall.API.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Sammelkarten.Models {

    public class ObservableCardCollection : ObservableCollection<Card> {

        #region Properties

        public int CountFaces {
            get => _countFaces;
            private set { _countFaces = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(CountFaces))); }
        }

        #endregion Properties

        #region Methods

        protected override void InsertItem(int index, Card item) {
            base.InsertItem(index, item);
            item.IsInCollection = true;
            CountFaces += item.PrintImages.Count();
        }

        protected override void RemoveItem(int index) {
            var item = this[index];
            item.IsInCollection = false;
            CountFaces -= item.PrintImages.Count();
            base.RemoveItem(index);
        }

        #endregion Methods

        #region Fields

        private int _countFaces;

        #endregion Fields
    }
}