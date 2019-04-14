namespace Sammelkarten {

    public class CancelToken : ViewModelBase {

        #region Properties

        public bool IsCancelRequested {
            get => _isCancelRequested;
            private set { _isCancelRequested = value; RaisePropertyChanged(); }
        }

        #endregion Properties

        #region Methods

        public void Cancel() {
            IsCancelRequested = true;
        }

        public void Reset() {
            IsCancelRequested = false;
        }

        #endregion Methods

        #region Fields

        private bool _isCancelRequested;

        #endregion Fields
    }
}