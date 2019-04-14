using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sammelkarten {

    public abstract class ViewModelBase : INotifyPropertyChanged {

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

        public void RaisePropertyChanged([CallerMemberName]string name = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion Methods
    }
}