using System;
using System.Globalization;
using System.Windows.Data;

namespace Sammelkarten {

    [ValueConversion(typeof(bool), typeof(bool))]
    internal class InvertBooleanConverter : IValueConverter {

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(bool)value;
        }

        #endregion Methods
    }
}