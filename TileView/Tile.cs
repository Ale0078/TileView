using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TileView.Extentions;

namespace TileView
{
    [TemplateVisualState(GroupName = TILE_ANIMATION_GROUP, Name = MOUSE_ENTER_TILE_ANIMATION)]
    [TemplateVisualState(GroupName = TILE_ANIMATION_GROUP, Name = MOUSE_LEAVE_TILE_ANIMATION)]    
    [TemplateVisualState(GroupName = TILE_ANIMATION_GROUP, Name = MOUSE_LEFT_BUTTON_DOWN_TILE_ANIMATION)]    
    [TemplateVisualState(GroupName = TILE_ANIMATION_GROUP, Name = MOUSE_LEFT_NUTTON_UP_TILE_ANIMATION)]
    public class Tile : Button
    {
        private const string TILE_ANIMATION_GROUP = "DefaultAnimation";

        private const string MOUSE_ENTER_TILE_ANIMATION = "MouseEnter";
        private const string MOUSE_LEAVE_TILE_ANIMATION = "MouseLeave";
        private const string MOUSE_LEFT_BUTTON_DOWN_TILE_ANIMATION = "MouseLeftButtonDown";
        private const string MOUSE_LEFT_NUTTON_UP_TILE_ANIMATION = "MouseLeftButtonUp";

        private object _tileDataContext;

        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty SourceProperty;
        public static readonly DependencyProperty DoseSetDataContextProperty;

        static Tile()
        {
            FocusVisualStyleProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(null));
            MarginProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(new Thickness(8)));
            FontSizeProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(16d));
            FontWeightProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(FontWeights.DemiBold));
            BackgroundProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(232, 197, 128))));

            TextProperty = DependencyProperty.Register(
                name: nameof(Text), 
                propertyType: typeof(string), 
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata(""));

            SourceProperty = DependencyProperty.Register(
                name: nameof(Source),
                propertyType: typeof(string),
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata("/"),
                validateValueCallback: ValidateSourceCallback);

            DoseSetDataContextProperty = DependencyProperty.Register(
                name: nameof(DoesSetDataContext), 
                propertyType: typeof(bool), 
                ownerType: typeof(Tile),
                typeMetadata: new PropertyMetadata(false),
                validateValueCallback: ValidateDoseSetDataContextCallback);
        }

        public Tile()
        {
            FocusVisualStyle = null;
            Background = new SolidColorBrush(Color.FromRgb(232, 197, 128));
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public bool DoesSetDataContext 
        {
            get => (bool)GetValue(DoseSetDataContextProperty);
            set => SetValue(DoseSetDataContextProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            
            VisualStateManager.GoToState(this, MOUSE_ENTER_TILE_ANIMATION, true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            VisualStateManager.GoToState(this, MOUSE_LEAVE_TILE_ANIMATION, true);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            
            e.Handled = false;

            VisualStateManager.GoToState(this, MOUSE_LEFT_NUTTON_UP_TILE_ANIMATION, true);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            _tileDataContext = ((Tile)e.Source).DataContext;
            ((Tile)e.Source).DataContext = VisualOffset;

            CaptureMouse();

            VisualStateManager.GoToState(this, MOUSE_LEFT_BUTTON_DOWN_TILE_ANIMATION, true);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            DataContext = _tileDataContext;

            DragDrop.DoDragDrop(this, 3, DragDropEffects.Move);

            ReleaseMouseCapture();
        }

        private static bool ValidateDoseSetDataContextCallback(object doesSet) =>
            doesSet.DoesMatchType(typeof(bool));

        private static bool ValidateSourceCallback(object doesSet) =>
            doesSet.DoesMatchType(typeof(string));
    }
}
