using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Threading;

namespace RogueCore
{
    /// <summary>
    /// Represents a character with color information for the text-based display
    /// </summary>
    public class Char
    {
        public Color BackColor { get; set; }
        public Color FrontColor { get; set; }
        public char Character { get; set; }

        public Char()
        {
            BackColor = Color.Black;
            FrontColor = Color.LightGray;
            Character = ' ';
        }

        public Char(char character, Color frontColor, Color backColor)
        {
            Character = character;
            FrontColor = frontColor;
            BackColor = backColor;
        }

        public Char Clone()
        {
            return new Char(Character, FrontColor, BackColor);
        }
    }

    /// <summary>
    /// A control that emulates a text-based video display with colored characters and cursor support
    /// </summary>
    public class Screen : Control
    {
        private Char[] _screen;
        private int _screenWidth;
        private int _screenHeight;

        public int ScreenWidth
        {
            get => _screenWidth;
            set
            {
                _screenWidth = value;
                ResizeScreen();
            }
        }
        
        public int ScreenHeight
        {
            get => _screenHeight;
            set
            {
                _screenHeight = value;
                ResizeScreen();
            }
        }

        private BufferedGraphics _gfx = null;
        private BufferedGraphicsContext _context;

        private Point _cursorPos = new Point();
        private bool _cursorShowToggle = true;
        private bool _cursorVisible = true;

        public Screen()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            ScreenWidth = 80;
            ScreenHeight = 25;

            var dt = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            dt.Tick += CursorTick;
            dt.Start();
        }

        private void ResizeScreen()
        {
            _screen = new Char[_screenWidth * _screenHeight];
            ClearScreen();
        }

        private void CursorTick(object sender, EventArgs e)
        {
            _cursorShowToggle = !_cursorShowToggle;
            Invalidate();
        }

        public void ClearScreen()
        {
            for (var y = 0; y < ScreenHeight; y++)
            {
                for (var x = 0; x < ScreenWidth; x++)
                {
                    SetChar(x, y, new Char());
                }
            }
            Invalidate();
        }

        public void SetChar(int x, int y, Char character)
        {
            if (!IsValidCoordinate(x, y))
                return;

            _screen[y * ScreenWidth + x] = character?.Clone() ?? new Char();
        }

        public Char GetChar(int x, int y)
        {
            if (!IsValidCoordinate(x, y))
                return new Char();

            return _screen[y * ScreenWidth + x]?.Clone() ?? new Char();
        }

        private bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < ScreenWidth && y >= 0 && y < ScreenHeight;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_gfx == null)
            {
                ReallocateGraphics();
            }

            Draw(_gfx.Graphics);

            _gfx.Render(e.Graphics);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_gfx != null)
            {
                _gfx.Dispose();
                ReallocateGraphics();
            }

            Invalidate();
            base.OnSizeChanged(e);
        }

        private void ReallocateGraphics()
        {
            _context = BufferedGraphicsManager.Current;
            _context.MaximumBuffer = new Size(Width + 1, Height + 1);

            _gfx = _context.Allocate(CreateGraphics(),
                 new Rectangle(0, 0, Width, Height));
        }

        private void Draw(Graphics gr)
        {
            if (Width == 0 || Height == 0)
                return;

            gr.Clear(BackColor);

            var textSize = gr.MeasureString("X", Font);

            float charWidth = textSize.Width - 3;
            float charHeight = textSize.Height - 1;

            for (var y = 0; y < ScreenHeight; y++)
            {
                for (var x = 0; x < ScreenWidth; x++)
                {
                    var character = _screen[y * ScreenWidth + x];

                    DrawChar(x, y, charWidth, charHeight, gr, character);
                }
            }

            DrawCursor(charWidth, charHeight, gr);
        }

        private void DrawChar(int x, int y, float charWidth, float charHeight, Graphics gr, Char character)
        {
            var text = new string(character.Character, 1);

            gr.FillRectangle(new SolidBrush(character.BackColor),
                    x * charWidth, y * charHeight,
                    charWidth, charHeight);

            gr.DrawString(text, Font, new SolidBrush(character.FrontColor),
                x * charWidth, y * charHeight);
        }

        private void DrawCursor(float charWidth, float charHeight, Graphics gr)
        {
            if (_cursorPos.X < 0 || _cursorPos.X >= ScreenWidth)
                return;
            if (_cursorPos.Y < 0 || _cursorPos.Y >= ScreenHeight)
                return;

            var cursorChar = new Char { Character = '_' };

            var text = new string(cursorChar.Character, 1);

            if (_cursorShowToggle && _cursorVisible)
            {
                gr.DrawString(text, Font, new SolidBrush(cursorChar.FrontColor),
                    _cursorPos.X * charWidth, _cursorPos.Y * charHeight + 2);
                gr.DrawString(text, Font, new SolidBrush(cursorChar.FrontColor),
                    _cursorPos.X * charWidth, _cursorPos.Y * charHeight + 3);
            }
            else
            {
                gr.DrawString(text, Font, new SolidBrush(cursorChar.BackColor),
                    _cursorPos.X * charWidth, _cursorPos.Y * charHeight + 2);
                gr.DrawString(text, Font, new SolidBrush(cursorChar.BackColor),
                    _cursorPos.X * charWidth, _cursorPos.Y * charHeight + 3);
            }
        }

        public void SetCursor(Point point)
        {
            _cursorPos = point;
            Invalidate();
        }

        public Point GetCursor()
        {
            return _cursorPos;
        }

        public void ShowCursor()
        {
            _cursorVisible = true;
            Invalidate();
        }

        public void HideCursor()
        {
            _cursorVisible = false;
            Invalidate();
        }

        public bool IsCursorVisible()
        {
            return _cursorVisible;
        }

        public void ClearLine(int n)
        {
            if (n < 0 || n >= ScreenHeight)
                return;
            
            for (var x = 0; x < ScreenWidth; x++)
                SetChar(x, n, new Char());
                
            Invalidate();
        }

        public void Print(int x, int y, string text)
        {
            if (text == null) return;
            
            for (var i = 0; i < text.Length && x + i < ScreenWidth; i++)
            {
                var character = GetChar(x + i, y);
                character.Character = text[i];
                SetChar(x + i, y, character);
            }

            Invalidate();
        }

        public void PutChar(char charValue, bool backward = false)
        {
            var pos = GetCursor();

            if (backward)
                pos.X--;

            var character = GetChar(pos.X, pos.Y);
            character.Character = charValue;
            SetChar(pos.X, pos.Y, character);

            if (!backward)
                pos.X++;

            SetCursor(pos);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }
    }
}
