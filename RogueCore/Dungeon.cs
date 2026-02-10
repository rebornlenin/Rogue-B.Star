using System;
using System.Collections.Generic;
using System.Drawing;

namespace RogueCore
{
    /// <summary>
    /// Represents a single cell/tile in the dungeon
    /// </summary>
    public class Cell
    {
        public bool Visible { get; set; } = false;
        public bool Solid { get; set; } = false;
        public Char Character { get; set; } = new Char();
        public int DebugCounter { get; set; } = 0;

        public Cell Clone()
        {
            return new Cell
            {
                Visible = Visible,
                Solid = Solid,
                Character = Character?.Clone() ?? new Char(),
                DebugCounter = DebugCounter
            };
        }
    }

    /// <summary>
    /// Represents a dungeon map consisting of cells/tiles
    /// </summary>
    public class Dungeon
    {
        private readonly Cell[] _cells;
        public int Width { get; }
        public int Height { get; }
        private readonly Char _invisibleTile = new Char();

        public Dungeon(int w, int h)
        {
            Width = w;
            Height = h;
            _cells = new Cell[w * h];
            Clear();
        }

        /// <summary>
        /// Clears the dungeon by initializing all cells to default values
        /// </summary>
        public void Clear()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _cells[y * Width + x] = new Cell();
                }
            }
        }

        /// <summary>
        /// Gets a cell at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The cell at the specified coordinates, or a default cell if out of bounds</returns>
        public Cell GetCell(int x, int y)
        {
            if (!IsValidCoordinate(x, y))
                return new Cell();

            return _cells[y * Width + x]?.Clone() ?? new Cell();
        }

        /// <summary>
        /// Sets a cell at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="tile">The tile to set</param>
        public void SetCell(int x, int y, Cell tile)
        {
            if (!IsValidCoordinate(x, y) || tile == null)
                return;

            _cells[y * Width + x] = tile.Clone();
        }

        /// <summary>
        /// Displays the visible parts of the dungeon on the screen
        /// </summary>
        /// <param name="screen">The screen to display on</param>
        /// <param name="lineStart">The starting line on the screen</param>
        public void Display(Screen screen, int lineStart)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = GetCell(x, y);
                    var character = cell.Visible ? cell.Character : _invisibleTile;
                    screen.SetChar(x, lineStart + y, character);
                }
            }

            screen.Invalidate();
        }

        private bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private int FillCallback(Point point, object ctx)
        {
            SetCell(point.X, point.Y, ctx as Cell);
            return 0;
        }

        /// <summary>
        /// Fills a line between two points with the specified tile
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="end">Ending point</param>
        /// <param name="tile">Tile to fill with</param>
        public void FillLine(Point start, Point end, Cell tile)
        {
            Tracer.TraceLine(start, end, FillCallback, tile);
        }

        /// <summary>
        /// Fills a rectangular area with the specified tile
        /// </summary>
        /// <param name="rect">Rectangle to fill</param>
        /// <param name="tile">Tile to fill with</param>
        public void FillRect(Rectangle rect, Cell tile)
        {
            for (var y = 0; y < rect.Height; y++)
            {
                for (var x = 0; x < rect.Width; x++)
                {
                    SetCell(rect.X + x, rect.Y + y, tile);
                }
            }
        }

        /// <summary>
        /// Fills a circular area with the specified tile
        /// </summary>
        /// <param name="center">Center of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="tile">Tile to fill with</param>
        public void FillCircle(Point center, int radius, Cell tile)
        {
            Tracer.TraceCircle(center, radius, FillCallback, tile);
        }

        /// <summary>
        /// Makes all cells in the dungeon visible
        /// </summary>
        public void ShowAll()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = GetCell(x, y);
                    cell.Visible = true;
                    SetCell(x, y, cell);
                }
            }
        }

        /// <summary>
        /// Hides all cells in the dungeon (makes them invisible)
        /// </summary>
        public void HideAll()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = GetCell(x, y);
                    cell.Visible = false;
                    SetCell(x, y, cell);
                }
            }
        }

        private int DungeonFovCallback(Point point, object ctx)
        {
            var cell = GetCell(point.X, point.Y);
            cell.Visible = true;

            if (cell.Solid)
                return -1;

            SetCell(point.X, point.Y, cell);
            return 0;
        }

        /// <summary>
        /// Updates the field of view from a specific point with a given radius
        /// </summary>
        /// <param name="point">The origin point for the FOV calculation</param>
        /// <param name="radius">The radius of the field of view</param>
        public void UpdateFov(Point point, int radius)
        {
            Tracer.TraceFov(point, radius, DungeonFovCallback);
        }
    }
}
