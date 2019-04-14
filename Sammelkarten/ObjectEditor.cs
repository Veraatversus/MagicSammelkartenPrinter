using Syncfusion.Windows.PropertyGrid;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sammelkarten {

    internal class ObjectEditor : ITypeEditor {

        #region Methods

        public void Attach(PropertyViewItem property, PropertyItem info) {
            var binding = new Binding("Value") {
                Source = info,
                Converter = new PropertyStringConverter()
            };
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);
        }

        public object Create(PropertyInfo PropertyInfo) {
            textBox = new TextBox { TextWrapping = TextWrapping.Wrap, IsReadOnly = true };
            return textBox;
        }

        public void Detach(PropertyViewItem property) {
        }

        #endregion Methods

        #region Fields

        private TextBox textBox;

        #endregion Fields
    }
}