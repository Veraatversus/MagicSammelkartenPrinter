using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Sammelkarten.Converter {

    public class EnumDescriptionTypeConverter : EnumConverter {

        #region Constructors

        public EnumDescriptionTypeConverter(Type type)
            : base(type) {
        }

        #endregion Constructors

        #region Methods

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                if (value != null) {
                    var fi = value.GetType().GetField(value.ToString());
                    if (fi != null) {
                        var attributes = (EnumMemberAttribute[])fi.GetCustomAttributes(typeof(EnumMemberAttribute), false);
                        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Value))) ? attributes[0].Value : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        #endregion Methods
    }
}