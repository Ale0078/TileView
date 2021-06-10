using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

using TileView.Extentions;

namespace TileView
{
    public class TileView : Grid
    {
        private bool _isPageSetted;

        private Point _tileToMoveOffset;

        private Tile _tileToMove;
        private TileGroup _groupWithTileToMove;
        private Frame _page;

        public static readonly DependencyProperty OrientationProperty;

        public TileView() 
        {
            _page = new Frame();

            _page.LoadCompleted += LoadCompletedEventHandler;
            _page.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            SetZIndex(_page, 1);

            FocusVisualStyle = null;
        }

        static TileView() 
        {
            FocusVisualStyleProperty.OverrideMetadata(typeof(TileView), new FrameworkPropertyMetadata(null));
            BackgroundProperty.OverrideMetadata(typeof(TileView), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(128, 55, 237))));            

            OrientationProperty = DependencyProperty.Register(
                name: nameof(Orientation), 
                propertyType: typeof(Orientation),
                ownerType: typeof(TileView), 
                typeMetadata: new PropertyMetadata(Orientation.Horizontal),
                validateValueCallback: ValidateOrientationCallback);
        }

        public Orientation Orientation 
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is not null
                && (visualAdded is TileGroup
                    || (visualAdded is Frame && !_isPageSetted)))
            {
                base.OnVisualChildrenChanged(visualAdded, visualRemoved);

                if (visualAdded is TileGroup)
                {
                    AddNewGroup((UIElement)visualAdded);
                }

                if (visualAdded is Frame && !_isPageSetted)
                {
                    _isPageSetted = true;
                }
            }
            else if (visualRemoved is not null)
            {
                base.OnVisualChildrenChanged(visualAdded, visualRemoved);

                RowDefinitions.Clear();
                ColumnDefinitions.Clear();

                foreach (UIElement child in Children)
                {
                    if (child is TileGroup)
                    {
                        AddNewGroup(child);
                    }
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (!_isPageSetted)
            {
                Children.Add(_page);
            }

            return base.MeasureOverride(constraint);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            SetFocus();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetZIndex(_page, 0);
                _page.Content = null;

                InvalidateVisual();
            }
            else if (e.Key == Key.OemPlus) 
            {
                DialogWindiw dialog = new DialogWindiw();

                if (dialog.ShowDialog() == true)
                {
                    TextBlock text = new TextBlock
                    {
                        Text = dialog.Text
                    };

                    TileGrid tiles = new TileGrid();

                    tiles.RowDefinitions.Add(new RowDefinition());
                    tiles.RowDefinitions.Add(new RowDefinition());

                    tiles.ColumnDefinitions.Add(new ColumnDefinition());
                    tiles.ColumnDefinitions.Add(new ColumnDefinition());

                    TileGroup group = new TileGroup
                    {
                        GroupName = text,
                        Tiles = tiles
                    };

                    Children.Add(group);
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Tile tileToGetPage = e.Source as Tile;

            if (tileToGetPage is null)
            {
                e.Handled = true;

                return;
            }

            if (tileToGetPage is TilePlaceholder)
            {
                RemoveEmptyGroup();

                e.Handled = true;

                return;
            }

            _page.Source = new Uri(tileToGetPage.Source, UriKind.Relative);

            switch (Orientation) 
            {
                case Orientation.Horizontal:

                    SetColumnSpan(_page, ColumnDefinitions.Count);

                    break;
                case Orientation.Vertical:

                    SetRowSpan(_page, RowDefinitions.Count);

                    break;
            }

            if (((Tile)e.Source).DoesSetDataContext)
            {
                _page.DataContext = ((Tile)e.Source).Content;
            }
            else 
            {
                _page.DataContext = null;
            }

            SetZIndex(_page, 1000);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            _tileToMove = e.Source as Tile;

            if (_tileToMove is null)
            {
                e.Handled = true;

                return;
            }

            SetZIndex(_tileToMove, 100);

            foreach (UIElement child in Children)
            {
                if (child is Frame)
                {
                    continue;
                }

                if (((TileGroup)child).Tiles.Children.Contains(_tileToMove))
                {
                    SetZIndex(child, 1000);

                    _groupWithTileToMove = child as TileGroup;

                    break;
                }
            }

            _tileToMoveOffset = e.GetPosition(this);

            Vector tileOffset = (Vector)_tileToMove.DataContext;

            _tileToMoveOffset.X -= tileOffset.X;
            _tileToMoveOffset.Y -= tileOffset.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_tileToMove is null)
            {
                return;
            }

            Point mousePosition = e.GetPosition(this);

            mousePosition.X -= _tileToMoveOffset.X + _tileToMove.Margin.Left;
            mousePosition.Y -= _tileToMoveOffset.Y + _tileToMove.Margin.Top;

            Size renderSize = _tileToMove.RenderSize;

            renderSize.Width += _tileToMove.Margin.Left + _tileToMove.Margin.Right;
            renderSize.Height += _tileToMove.Margin.Top + _tileToMove.Margin.Bottom;

            _tileToMove.Arrange(new Rect(mousePosition, renderSize));
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (_tileToMove is null)
            {
                return;
            }

            SetZIndex(_tileToMove, 1);
            SetZIndex(_groupWithTileToMove, 1);

            _tileToMove = null;
            _groupWithTileToMove = null;
        }

        protected override void OnPreviewDrop(DragEventArgs e)
        {
            Tile sender = e.Source as Tile;

            Point mousePosition = e.GetPosition(this);            

            foreach (object group in Children)
            {
                if (!group.DoesMatchType(typeof(TileGroup)))
                {
                    continue;
                }

                foreach (Tile tileToDrop in ((TileGroup)group).Tiles.Children)
                {
                    if (tileToDrop.Equals(sender)
                    || tileToDrop.DesiredSize.Width == 0
                    || tileToDrop.DesiredSize.Height == 0)
                    {
                        continue;
                    }

                    Point childPossition = tileToDrop.GetPosition(this);

                    if ((childPossition.X < mousePosition.X
                            && mousePosition.X < (childPossition.X + tileToDrop.RenderSize.Width * tileToDrop.RenderTransform.Value.M11))
                        && (childPossition.Y < mousePosition.Y
                            && mousePosition.Y < (childPossition.Y + tileToDrop.RenderSize.Height * tileToDrop.RenderTransform.Value.M22)))
                    {
                        e.Data.SetData(typeof(Tile) ,tileToDrop);

                        break;
                    }
                }
            }
        }

        private static bool ValidateOrientationCallback(object orientation) 
        {
            if (orientation is Orientation)
            {
                return true;
            }

            return false;
        }

        private void LoadCompletedEventHandler(object sender, NavigationEventArgs e) 
        {
            if (((Frame)sender).Content == null)
            {
                return;
            }

            Frame frameToGetDataContext = (Frame)sender;
            Page pageToSetDataContext = (Page)((Frame)sender).Content;

            if (frameToGetDataContext.DataContext is not null)
            {
                pageToSetDataContext.DataContext = frameToGetDataContext.DataContext;
            }
        }

        private void SetFocus() 
        {
            Focusable = true;
            Keyboard.Focus(this);
        }

        private void RemoveEmptyGroup() 
        {
            foreach (UIElement group in Children)
            {
                if (group is Frame)
                {
                    continue;
                }

                if (((TileGroup)group).Tiles.Children.Count == 0)
                {                    
                    switch (Orientation)
                    {
                        case Orientation.Horizontal:
                            ColumnDefinitions.RemoveAt(GetColumn(group));
                            break;
                        case Orientation.Vertical:
                            RowDefinitions.RemoveAt(GetRow(group));
                            break;
                    }

                    Children.Remove(group);

                    SetFocus();

                    break;
                }
            }
        }

        private void AddNewGroup(UIElement group) 
        {
            switch (Orientation)
            {
                case Orientation.Horizontal:

                    ColumnDefinitions.Add(new ColumnDefinition());
                    SetColumn(group, ColumnDefinitions.Count - 1);

                    break;
                case Orientation.Vertical:

                    RowDefinitions.Add(new RowDefinition());
                    SetRow(group, RowDefinitions.Count - 1);

                    break;
            }
        }
    }
}
