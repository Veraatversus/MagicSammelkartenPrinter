using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Sammelkarten {

    /// <summary>Provides extension methods for <see cref="DependencyObject"/> objects. </summary>
    public static class DependencyObjectExtensions {

        #region Methods

        /// <summary>Traverses the visual tree and returns the first child of the desired type. </summary>
        /// <typeparam name="T">The child type to find. </typeparam>
        /// <param name="obj">The parent object. </param>
        /// <returns>The child object. </returns>
        public static T FindVisualChild<T>(this DependencyObject obj)
            where T : DependencyObject {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++) {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        /// <summary>Traverses the visual tree and returns all children of the desired type. </summary>
        /// <typeparam name="T">The child type to find. </typeparam>
        /// <param name="obj">The parent object. </param>
        /// <returns>The children. </returns>
        public static List<T> FindVisualChildren<T>(this DependencyObject obj)
            where T : DependencyObject {
            var results = new List<T>();
            FindVisualChildren<T>(obj, results);
            return results;
        }

        public static T FindVisualParent<T>(this DependencyObject child) where T : DependencyObject {
            //get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null)
                return null;

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        private static void FindVisualChildren<T>(DependencyObject obj, List<T> results)
            where T : DependencyObject {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++) {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    results.Add((T)child);

                FindVisualChildren(child, results);
            }
        }

        #endregion Methods
    }
}