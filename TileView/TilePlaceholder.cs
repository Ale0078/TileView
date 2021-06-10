using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace TileView
{
    public class TilePlaceholder : Tile
    {
        static TilePlaceholder() 
        {
            TextProperty.OverrideMetadata(typeof(TilePlaceholder), new PropertyMetadata("Placeholder"));
            BackgroundProperty.OverrideMetadata(typeof(TilePlaceholder), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(10, 185, 189, 186))));
            FocusVisualStyleProperty.OverrideMetadata(typeof(TilePlaceholder), new FrameworkPropertyMetadata(null));
            ForegroundProperty.OverrideMetadata(typeof(TilePlaceholder), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(10, 255, 255, 255))));
            FontWeightProperty.OverrideMetadata(typeof(TilePlaceholder), new FrameworkPropertyMetadata(FontWeights.Light));
        }

        public TilePlaceholder()
        {
            Background = new SolidColorBrush(Color.FromArgb(10, 185, 189, 186));
            Foreground = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));

            FocusVisualStyle = null;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
