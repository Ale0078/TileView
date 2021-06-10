using System.Windows;
using System.Windows.Media;

namespace TileView.Extentions
{
    public static class PanleExtention
    {
        public static T GetParent<T>(this DependencyObject child) where T : DependencyObject
        {
            if (child is null)
            {
                return null;
            }

            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent is null)
            {
                return null;
            }

            if (parent is T)
            {
                return parent as T;
            }

            return GetParent<T>(parent);
        }

        public static Point GetPosition(this UIElement element, Visual ancestor)
        {
            if (element == null)
            {
                return new Point(0, 0);
            }

            Point origin = element.RenderTransform.Inverse.Transform(new Point(0, 0));
            Point position = element.TransformToAncestor(ancestor).Transform(origin);

            return position;
        }
    }
}
