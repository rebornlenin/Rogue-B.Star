using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueCore
{
    /// <summary>
    /// Manages a queue of messages for display in a designated area of the screen
    /// </summary>
    public class Message
    {
        private const string MoreIndicator = " (more)";
        private readonly Queue<string> _messageQueue = new();
        private readonly int _lineStart;
        private readonly int _numLines;

        public Message(int lineStart, int numLines)
        {
            _lineStart = lineStart;
            _numLines = numLines;
        }

        /// <summary>
        /// Adds a message to the queue
        /// </summary>
        /// <param name="msg">The message to add</param>
        public void Add(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                _messageQueue.Enqueue(msg);
            }
        }

        /// <summary>
        /// Clears all messages from the queue
        /// </summary>
        public void Clear()
        {
            _messageQueue.Clear();
        }

        /// <summary>
        /// Displays messages on the screen, showing as many as fit in the allocated space
        /// </summary>
        /// <param name="screen">The screen to display messages on</param>
        public void ShowMore(Screen screen)
        {
            ClearMessageView(screen);

            if (_messageQueue.Count == 0)
            {
#if DEBUG
                screen.Print(0, _lineStart, "<empty>");
#endif
                return;
            }

            var displayedChars = 0;
            var linesUsed = 0;
            var currentLineText = string.Empty;

            // Process messages until we've filled all available lines or run out of messages
            while (linesUsed < _numLines && _messageQueue.Count > 0)
            {
                var nextMessage = _messageQueue.Peek();
                var words = nextMessage.Split(' ');

                foreach (var word in words)
                {
                    if (string.IsNullOrEmpty(word)) continue;

                    var testLine = string.IsNullOrEmpty(currentLineText) 
                        ? word 
                        : $"{currentLineText} {word}";

                    var isLastLine = linesUsed == (_numLines - 1);
                    var requiredSpace = isLastLine 
                        ? testLine.Length + MoreIndicator.Length 
                        : testLine.Length;

                    if (requiredSpace > screen.ScreenWidth)
                    {
                        // If this line is too long even for a single word, try to fit as much as possible
                        if (string.IsNullOrEmpty(currentLineText))
                        {
                            // Single word is too long, truncate it
                            if (isLastLine)
                            {
                                var truncatedWord = word.Substring(0, Math.Max(0, screen.ScreenWidth - MoreIndicator.Length));
                                screen.Print(0, _lineStart + linesUsed, truncatedWord + MoreIndicator);
                            }
                            else
                            {
                                var truncatedWord = word.Substring(0, Math.Max(0, screen.ScreenWidth));
                                screen.Print(0, _lineStart + linesUsed, truncatedWord);
                            }
                            
                            _messageQueue.Dequeue();
                            displayedChars += truncatedWord.Length;
                            linesUsed++;
                            currentLineText = string.Empty;
                            
                            if (linesUsed >= _numLines)
                                break;
                        }
                        else
                        {
                            // Current line + word is too long, so print current line and start a new one
                            if (isLastLine && _messageQueue.Count > 1)
                            {
                                screen.Print(0, _lineStart + linesUsed, currentLineText + MoreIndicator);
                            }
                            else
                            {
                                screen.Print(0, _lineStart + linesUsed, currentLineText);
                            }
                            
                            displayedChars += currentLineText.Length;
                            linesUsed++;
                            currentLineText = word; // Start new line with current word
                            
                            if (linesUsed >= _numLines)
                                break;
                        }
                    }
                    else
                    {
                        currentLineText = testLine;
                    }
                }

                // If we've processed all words in the current message, remove it from the queue
                if (words.All(string.IsNullOrEmpty) || currentLineText.EndsWith(words.Last()))
                {
                    _messageQueue.Dequeue();
                    
                    // If we're moving to a new line or have filled the space, print the current line
                    if (!string.IsNullOrEmpty(currentLineText))
                    {
                        var isLastLine = linesUsed == (_numLines - 1);
                        if (isLastLine && _messageQueue.Count > 0)
                        {
                            screen.Print(0, _lineStart + linesUsed, currentLineText + MoreIndicator);
                        }
                        else
                        {
                            screen.Print(0, _lineStart + linesUsed, currentLineText);
                        }
                        
                        displayedChars += currentLineText.Length;
                        linesUsed++;
                        currentLineText = string.Empty;
                        
                        if (linesUsed >= _numLines)
                            break;
                    }
                }
            }
        }

        private void ClearMessageView(Screen screen)
        {
            for (var y = 0; y < _numLines; y++)
                screen.ClearLine(_lineStart + y);
        }
    }
}
