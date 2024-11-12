namespace ConsoleScroller
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Console scroller.
    /// </summary>
    public class Scroller : IDisposable
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly int _MaxLines;
        private readonly Queue<ColorLine> _Lines;
        private readonly int _StartingTop;
        private readonly object _Lock = new object();
        private bool _Disposed;
        private ConsoleColor _DefaultForegroundColor;
        private ConsoleColor _DefaultBackgroundColor;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create the console scroller.
        /// </summary>
        /// <param name="maxLines">Maximum number of lines to consume on the console.</param>
        public Scroller(int maxLines)
        {
            if (maxLines < 1) throw new ArgumentOutOfRangeException(nameof(maxLines));

            _MaxLines = maxLines;
            _Lines = new Queue<ColorLine>();

            lock (_Lock)
            {
                _DefaultForegroundColor = Console.ForegroundColor;
                _DefaultBackgroundColor = Console.BackgroundColor;
                _StartingTop = Console.CursorTop;

                for (int i = 0; i < maxLines; i++)
                {
                    Console.WriteLine();
                }

                Console.SetCursorPosition(0, _StartingTop);
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;

            lock (_Lock)
            {
                _Disposed = true;
                Console.ForegroundColor = _DefaultForegroundColor;
                Console.BackgroundColor = _DefaultBackgroundColor;
                Console.SetCursorPosition(0, _StartingTop + _MaxLines);
            }
        }

        /// <summary>
        /// Add a line to the console scroller.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="foreground">Foreground color.</param>
        /// <param name="background">Background color.</param>
        public void WriteLine(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(Scroller));

            lock (_Lock)
            {
                if (_Lines.Count >= _MaxLines)
                {
                    _Lines.Dequeue();
                }

                _Lines.Enqueue(new ColorLine(
                    text,
                    foreground ?? Console.ForegroundColor,
                    background ?? Console.BackgroundColor));

                RefreshDisplay();
            }
        }

        #endregion

        #region Private-Methods

        private void RefreshDisplay()
        {
            var originalLeft = Console.CursorLeft;
            var originalTop = Console.CursorTop;
            var originalForeground = Console.ForegroundColor;
            var originalBackground = Console.BackgroundColor;

            try
            {
                // Clear the display area line by line
                Console.ForegroundColor = _DefaultForegroundColor;
                Console.BackgroundColor = _DefaultBackgroundColor;

                for (int i = 0; i < _MaxLines; i++)
                {
                    Console.SetCursorPosition(0, _StartingTop + i);
                    ClearCurrentLine();
                }

                // Write the lines
                int currentLine = 0;
                foreach (var line in _Lines)
                {
                    Console.SetCursorPosition(0, _StartingTop + currentLine);
                    Console.ForegroundColor = line.Foreground;
                    Console.BackgroundColor = line.Background;
                    Console.Write(line.Text);
                    currentLine++;
                }
            }
            finally
            {
                if (!_Disposed)
                {
                    Console.ForegroundColor = originalForeground;
                    Console.BackgroundColor = originalBackground;
                    Console.SetCursorPosition(originalLeft, originalTop);
                }
            }
        }

        private void ClearCurrentLine()
        {
            int currentLeft = Console.CursorLeft;
            int currentTop = Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth - currentLeft));
            Console.SetCursorPosition(currentLeft, currentTop);
        }

        #endregion

        #region Private-Embedded-Classes

        private class ColorLine
        {
            public string Text { get; }
            public ConsoleColor Foreground { get; }
            public ConsoleColor Background { get; }

            public ColorLine(string text, ConsoleColor foreground, ConsoleColor background)
            {
                Text = text;
                Foreground = foreground;
                Background = background;
            }
        }

        #endregion
    }
}