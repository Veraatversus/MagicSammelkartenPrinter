using System;
using System.Windows.Markup;

namespace Sammelkarten {

    public class EnumBindingSourceExtension : MarkupExtension {

        #region Constructors

        public EnumBindingSourceExtension() {
        }

        public EnumBindingSourceExtension(Type enumType) {
            EnumType = enumType;
        }

        #endregion Constructors

        #region Properties

        public Type EnumType {
            get => _enumType;
            set {
                if (value != _enumType) {
                    if (value != null) {
                        var enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum) {
                            throw new ArgumentException("Type must be for an Enum.");
                        }
                    }

                    _enumType = value;
                }
            }
        }

        #endregion Properties

        #region Methods

        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (_enumType == null) {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            var enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == _enumType) {
                return enumValues;
            }

            var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }

        #endregion Methods

        #region Fields

        private Type _enumType;

        #endregion Fields
    }
}