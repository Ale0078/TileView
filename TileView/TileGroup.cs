using System.Windows;
using System.Windows.Controls;

using TileView.Extentions;

namespace TileView
{
    public class TileGroup : DockPanel
    {
        private bool _isNameSetted;
        private bool _isTileGroupCreated;

        public static readonly DependencyProperty GroupNameProperty;
        public static readonly DependencyProperty TilesProperty;

        public TileGroup()
        {
            FocusVisualStyle = null;
            LastChildFill = true;

            _isNameSetted = false;
            _isTileGroupCreated = false;
        }        

        static TileGroup()
        {
            FocusVisualStyleProperty.OverrideMetadata(typeof(TileGroup), new FrameworkPropertyMetadata(null));
            MarginProperty.OverrideMetadata(typeof(TileGroup), new FrameworkPropertyMetadata(new Thickness(20)));

            GroupNameProperty = DependencyProperty.Register(
                name: nameof(GroupName),
                propertyType: typeof(TextBlock),
                ownerType: typeof(TileGroup),
                typeMetadata: new PropertyMetadata(null, GroupNameChangedCallback),
                validateValueCallback: ValidateGroupNameCallback);

            TilesProperty = DependencyProperty.Register(
                name: nameof(Tiles),
                propertyType: typeof(TileGrid),
                ownerType: typeof(TileGroup),
                typeMetadata: new PropertyMetadata(),
                validateValueCallback: ValidateTilesCallback);
        }

        public TextBlock GroupName 
        {
            get => (TextBlock)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public TileGrid Tiles 
        {
            get => (TileGrid)GetValue(TilesProperty);
            set => SetValue(TilesProperty, value);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if ((visualAdded is TextBlock && !_isNameSetted) || (visualAdded is TileGrid && !_isTileGroupCreated))
            {
                base.OnVisualChildrenChanged(visualAdded, visualRemoved);

                if (visualAdded is TextBlock && !_isNameSetted)
                {
                    _isNameSetted = true;
                }

                if (visualAdded is Grid && !_isTileGroupCreated)
                {
                    _isNameSetted = true;
                }
            }
        }
        
        protected override Size MeasureOverride(Size constraint)
        {
            if (!_isNameSetted && !_isTileGroupCreated)
            {
                Children.Add(GroupName);
                Children.Add(Tiles);
            }
            
            return base.MeasureOverride(constraint);
        }

        private static bool ValidateGroupNameCallback(object groupName) =>
            groupName.DoesMatchType(typeof(TextBlock));

        private static bool ValidateTilesCallback(object tiles) =>
            tiles.DoesMatchType(typeof(TileGrid));

        private static void GroupNameChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            SetDock(((TileGroup)d).GroupName, Dock.Top);
    }
}
