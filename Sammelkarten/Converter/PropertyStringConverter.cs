using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Sammelkarten {

    [ValueConversion(typeof(object), typeof(string))]
    internal class PropertyStringConverter : IValueConverter {

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is IDictionary dic) {
                return string.Join("\r\n", dic.Keys.Cast<object>().Zip(dic.Values.Cast<object>(), (o, k) => $"{o} => {k}"));
            }

            if (!(value is string) && value is IEnumerable list) {
                return string.Join("\r\n", list.Cast<object>());
            }
            //if (value is IList<object> list2) {
            //    return string.Join("\r\n", list2);
            //}
            else {
                return value?.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        #endregion Methods
    }
}