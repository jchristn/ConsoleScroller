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

                // Always start from the current cursor position
                _StartingTop = Console.CursorTop;

                // Make sure we have enough room in the buffer
                int bufferHeight = Console.BufferHeight;
                int availableLines = bufferHeight - _StartingTop;

                // Add lines to the buffer if needed
                if (availableLines < maxLines)
                {
                    int linesToAdd = maxLines - availableLines;
                    for (int i = 0; i < linesToAdd; i++)
                    {
                        Console.WriteLine();
                    }
                }

                // Reserve the display area by clearing each line
                for (int i = 0; i < maxLines; i++)
                {
                    if (_StartingTop + i < Console.BufferHeight)
                    {
                        Console.SetCursorPosition(0, _StartingTop + i);
                        ClearCurrentLine();
                    }
                }

                // Return to the starting position
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

                // Position cursor right after the last displayed line
                // If we have fewer lines than max, position after the last actual line
                int lastLinePos = _StartingTop + Math.Min(_Lines.Count, _MaxLines);

                // Ensure we don't go beyond buffer height
                lastLinePos = Math.Min(lastLinePos, Console.BufferHeight - 1);

                Console.SetCursorPosition(0, lastLinePos);

                // Ensure there's a new line after our scroller content
                Console.WriteLine();
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
                // We will always append the new line, regardless of buffer size
                _Lines.Enqueue(new ColorLine(
                    text,
                    foreground ?? Console.ForegroundColor,
                    background ?? Console.BackgroundColor));

                // When we exceed MaxLines, we'll show only the most recent ones
                // The display logic handles this in RefreshDisplay()

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
                // Store current console colors
                Console.ForegroundColor = _DefaultForegroundColor;
                Console.BackgroundColor = _DefaultBackgroundColor;

                // Clear all lines in our display area first
                for (int i = 0; i < _MaxLines; i++)
                {
                    int linePosition = _StartingTop + i;
                    if (linePosition >= Console.BufferHeight)
                        break;

                    Console.SetCursorPosition(0, linePosition);
                    ClearCurrentLine();
                }

                // Convert queue to array for indexed access
                ColorLine[] allLines = _Lines.ToArray();

                // Calculate which lines to display (most recent _MaxLines)
                int linesToShow = Math.Min(_MaxLines, allLines.Length);
                int startIndex = allLines.Length - linesToShow;

                // Display the lines in the correct positions
                for (int i = 0; i < linesToShow; i++)
                {
                    int lineIndex = startIndex + i;
                    int displayPosition = _StartingTop + i;

                    if (displayPosition >= Console.BufferHeight)
                        break;

                    Console.SetCursorPosition(0, displayPosition);
                    Console.ForegroundColor = allLines[lineIndex].Foreground;
                    Console.BackgroundColor = allLines[lineIndex].Background;
                    Console.Write(allLines[lineIndex].Text);
                }
            }
            finally
            {
                if (!_Disposed)
                {
                    Console.ForegroundColor = originalForeground;
                    Console.BackgroundColor = originalBackground;

                    // Ensure we don't set cursor beyond buffer height
                    if (originalTop < Console.BufferHeight)
                    {
                        Console.SetCursorPosition(originalLeft, originalTop);
                    }
                    else
                    {
                        Console.SetCursorPosition(originalLeft, Console.BufferHeight - 1);
                    }
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