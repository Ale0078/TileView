using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TileView
{
    public class TileGrid : Grid
    {
        private Tile _droppedTile;

        static TileGrid() 
        {
            FocusVisualStyleProperty.OverrideMetadata(typeof(TileGrid), new FrameworkPropertyMetadata(null));
        }

        public TileGrid()
        {
            AllowDrop = true;
            FocusVisualStyle = null;
            _droppedTile = null;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is Tile || visualRemoved is not null)
            {
                base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                for (int j = 0; j < ColumnDefinitions.Count; j++)
                {
                    GridPosition tileToCheckPosition = new GridPosition(i, j);

                    Tile tileToCheck = GetTileByPosition(this, tileToCheckPosition);

                    if (tileToCheck is null)
                    {
                        TilePlaceholder placeholder = new TilePlaceholder();

                        Children.Add(placeholder);

                        tileToCheckPosition.SetTilePosition(placeholder);
                    }
                }
            }

            return base.MeasureOverride(constraint);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            
            InvalidateVisual();
        }

        protected override void OnPreviewDrop(DragEventArgs e)
        {
            _droppedTile = e.Data.GetData(typeof(Tile)) as Tile;;

            if (_droppedTile is null)
            {
                e.Handled = true;
            }
        }
        
        protected override void OnDrop(DragEventArgs e)
        {
            Tile source = e.Source as Tile;

            TileGrid parendOfDroppedTile = _droppedTile.Parent as TileGrid;

            if (!parendOfDroppedTile.Equals(this))
            {
                ChangeOtherTileGrid(source, parendOfDroppedTile);

                return;
            }

            ChangeCurrentTileGrid(source);

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.Source is not TilePlaceholder)
            {
                return;
            }

            e.Handled = true;

            GridPosition placeholderPosition = GridPosition.GetTilePosition(e.Source as TilePlaceholder);

            bool haveTile = HaveTileAtColumn(placeholderPosition);

            if (!haveTile)
            {
                for (int i = 0; i < RowDefinitions.Count; i++)
                {
                    Children.Remove(GetTileByPosition(this, new GridPosition(i, placeholderPosition.Column)));
                }

                ColumnDefinitions.RemoveAt(placeholderPosition.Column);

                if (ColumnDefinitions.Count == 0)
                {
                    e.Handled = false;

                    return;
                }

                for (int i = 0; i < RowDefinitions.Count; i++)
                {
                    for (int j = placeholderPosition.Column; j < ColumnDefinitions.Count; j++)
                    {
                        GridPosition tilePosition = new GridPosition(i, j);

                        Tile tileToSwitch = GetTileByPosition(this, tilePosition.GetNextPosition());

                        if (tileToSwitch is null)
                        {
                            continue;
                        }

                        tilePosition.SetTilePosition(tileToSwitch);
                    }
                }

                Focusable = true;
                Keyboard.Focus(this);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            SetZIndex(e.Source as UIElement, 100);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            SetZIndex(e.Source as UIElement, 1);
        }

        private void ChangeCurrentTileGrid(Tile source)
        {
            GridPosition droppedTilePosition = GridPosition.GetTilePosition(_droppedTile);
            GridPosition droppedTilePositionCopy = droppedTilePosition;

            bool isEmptyCell = false;
            bool isNullTile = false;
            Direction searchingDirection;

            if (droppedTilePosition.Row == 0)
            {
                searchingDirection = Direction.Left;
            }
            else
            {
                searchingDirection = Direction.Right;
            }

            Tile tileToSwitch = null;
            Tile previousTile;

            for (int i = 0; i < Children.Count; i++)
            {
                previousTile = tileToSwitch;

                tileToSwitch = GetTileByPosition(this, droppedTilePositionCopy, tileToSwitch);

                searchingDirection = SetDirection(searchingDirection, droppedTilePositionCopy, GridPosition.GetTilePosition(source));

                droppedTilePositionCopy = droppedTilePositionCopy.GetNextPosition(searchingDirection);

                if (tileToSwitch is null)
                {
                    isNullTile = true;

                    GridPosition.GetTilePosition(source as Tile).SetTilePosition(previousTile);
                }

                if (tileToSwitch is TilePlaceholder)
                {
                    i -= 2;

                    isEmptyCell = true;

                    Children.Remove(tileToSwitch);

                    continue;
                }

                if (isNullTile || isEmptyCell || tileToSwitch.Equals(source))
                {
                    droppedTilePosition.SetTilePosition(source);

                    break;
                }

                droppedTilePositionCopy.SetTilePosition(tileToSwitch);
            }
        }

        private void ChangeOtherTileGrid(Tile source, TileGrid tileGridWithDroppedTile) 
        {
            if (_droppedTile is TilePlaceholder)
            {
                FillPlaceholder(source, tileGridWithDroppedTile);

                return;
            }

            AddNewTile(source, tileGridWithDroppedTile);
        }

        private void FillPlaceholder(Tile source, TileGrid tileGridWithDroppedTile) 
        {
            GridPosition.GetTilePosition(_droppedTile).SetTilePosition(source);

            Children.Remove(source);

            tileGridWithDroppedTile.Children.Remove(_droppedTile);
            tileGridWithDroppedTile.Children.Add(source);
        }

        private void AddNewTile(Tile source, TileGrid tileGridWithDroppedTile) 
        {
            tileGridWithDroppedTile.ColumnDefinitions.Add(new ColumnDefinition());

            bool isStart = false;

            GridPosition droppedTilePosition = GridPosition.GetTilePosition(_droppedTile);
            Tile tileToSwitch = _droppedTile;

            for (int i = droppedTilePosition.Column; i < tileGridWithDroppedTile.ColumnDefinitions.Count; i++)
            {
                if (!isStart)
                {
                    droppedTilePosition.SetTilePosition(source);

                    Children.Remove(source);

                    tileGridWithDroppedTile.Children.Add(source);

                    isStart = true;
                }

                droppedTilePosition = droppedTilePosition.GetNextPosition();

                droppedTilePosition.SetTilePosition(tileToSwitch);

                tileToSwitch = GetTileByPosition(tileGridWithDroppedTile ,droppedTilePosition, tileToSwitch);

                if (tileToSwitch is null)
                {
                    break;
                }
            }
        }

        private static Tile GetTileByPosition(TileGrid tileGridToSearch, GridPosition tilePosition, Tile invalidTile = null) 
        {
            foreach (Tile tileToSearch in tileGridToSearch.Children)
            {
                if (GridPosition.GetTilePosition(tileToSearch).Equals(tilePosition))
                {
                    if (!ReferenceEquals(tileToSearch, invalidTile))
                    {
                        return tileToSearch; 
                    }
                }
            }

            return null;
        }

        private Direction SetDirection(Direction searchingDirection, GridPosition droppedTilePosition, GridPosition sourcePosition) 
        {   
            if (droppedTilePosition.DoesMatch(0, ColumnDefinitions.Count - 1) && ColumnDefinitions.Count != 1)
            {
                searchingDirection = Direction.Left;
            }
            else if (droppedTilePosition.DoesMatch(RowDefinitions.Count - 1, 0) && RowDefinitions.Count != 1)
            {
                searchingDirection = Direction.Right;
            }
            else if (droppedTilePosition.DoesMatch(null, ColumnDefinitions.Count - 1) && ColumnDefinitions.Count != 1)
            {
                searchingDirection = Direction.Top;
            }
            else if (droppedTilePosition.DoesMatch(0, 0) || droppedTilePosition.DoesMatch(0, sourcePosition.Column))
            {
                searchingDirection = Direction.Bottom;
            }

            return searchingDirection;
        }

        private bool HaveTileAtColumn(GridPosition placeholderPosition) 
        {
            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                if (GetTileByPosition(this, new GridPosition(i, placeholderPosition.Column)) is not TilePlaceholder)
                {
                    return true;
                }
            }

            return false;
        }





        private struct GridPosition
        {
            public GridPosition(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public int Row { get; set; }
            public int Column { get; set; }

            public void Deconstruct(out int row, out int column) 
            {
                row = Row;
                column = Column;
            }
            
            public static GridPosition GetTilePosition(Tile tileToGetPosition) =>
                new GridPosition(GetRow(tileToGetPosition), GetColumn(tileToGetPosition));

            public void SetTilePosition(Tile tileToSetPosition) 
            {
                SetColumn(tileToSetPosition, Column);
                SetRow(tileToSetPosition, Row);                
            }

            public bool DoesMatch(int? rowEquality, int? columnEquality) =>
                (Row == rowEquality || rowEquality is null) && (Column == columnEquality || columnEquality is null);

            public GridPosition GetNextPosition(Direction direction = Direction.Right) => direction switch
            {
                Direction.Left => new GridPosition(Row, Column - 1),
                Direction.Right => new GridPosition(Row, Column + 1),
                Direction.Top => new GridPosition(Row - 1, Column),
                Direction.Bottom => new GridPosition(Row + 1, Column),
                _ => throw new InvalidOperationException()
            };
        }

        private enum Direction
        {
            Left,
            Right,
            Top,
            Bottom
        }
    }
}
