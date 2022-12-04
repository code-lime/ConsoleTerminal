#nullable enable
using System.Collections.Concurrent;

namespace Terminal
{
    public class TerminalConsole
    {
        private readonly ConcurrentQueue<ITerminalMessage> messages = new ConcurrentQueue<ITerminalMessage>();
        private readonly object _lock = new object();

        protected string InputPrefix = "> ";
        protected string InputLine = "";
        protected int InputPosition = 0;
        protected string InputCursor = "_";

        protected virtual IEnumerable<ITerminalLine> GetTerminalLines()
        {
            bool tick = DateTime.Now.Second % 2 == 0;
            yield return new InputLine(InputPrefix, InputLine, InputCursor, ConsoleColor.White, tick ? ConsoleColor.Gray : ConsoleColor.DarkGray, InputPosition);
        }

        /// <returns>true - Skip input, false - Call input</returns>
        protected virtual bool OnConsoleExecute(ConsoleKeyInfo info)
        {
            switch (info.Key)
            {
                case ConsoleKey.RightArrow:
                    InputPosition = Math.Min(InputLine.Length + 1, InputPosition + 1);
                    Redraw();
                    return true;
                case ConsoleKey.LeftArrow:
                    InputPosition = Math.Max(0, InputPosition - 1);
                    Redraw();
                    return true;
                default: return false;
            }
        }
        protected void Redraw() => redraw = true;

        private bool redraw = false;
        private List<ITerminalLine> lastInputLines = new List<ITerminalLine>() { new TerminalLine("") };

        private static async ValueTask<ConsoleKeyInfo> ReadKeyAsync(bool intercept = false)
        {
            while (!Console.KeyAvailable) await Task.Delay(10);
            return Console.ReadKey(intercept);
        }
        private string AppendChar(string text, char ch)
        {
            int chID = ch;
            if (chID == 8)
            {
                if (InputPosition <= 0 || text.Length <= 0) return text;
                text = text.Remove(InputPosition - 1, 1);
                InputPosition--;
                return text;
            }
            else if (chID == 127 || (chID >= 0 && chID <= 31)) return text;
            else if (chID <= 255)
            {
                if (text.Length <= InputPosition)
                {
                    InputPosition = text.Length + 1;
                    return text + ch;
                }
                text = text.Insert(InputPosition - 1, ch.ToString());
                InputPosition++;
                return text;
            }
            else return text;
        }

        private async Task StartInput()
        {
            while (true)
            {
                try
                {
                    ConsoleKeyInfo key = await ReadKeyAsync(true);
                    lock (_lock)
                    {
                        if (OnConsoleExecute(key)) continue;
                        InputLine = AppendChar(InputLine, key.KeyChar).TrimStart(' ');
                        redraw = true;
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.ToString());
                    Console.ResetColor();
                    Environment.Exit(998);
                }
            }
        }

        private static int SplitLines(IEnumerable<ITerminalLine> inputLines, int width) => inputLines.Sum(v => v.SplitLines(width));

        /*
        private static List<string> SplitInput(string input, int width)
        {
            List<string> lines = new List<string>();
            string modify = input;
            while (true)
            {
                int lenght = modify.Length;
                if (lenght > width)
                {
                    lines.Add(modify[..(width - 1)]);
                    modify = modify[width..];
                }
                else
                {
                    lines.Add(modify);
                    break;
                }
            }
            return lines;
        }
        private static List<string> SplitInput(IEnumerable<string> inputLines, int width)
        {
            List<string> lines = new List<string>();
            foreach (string line in inputLines) lines.AddRange(SplitInput(line, width));
            return lines;
        }
        */

        private async Task StartOutput()
        {
            Console.CursorVisible = false;
            while (true)
            {
                bool draw = false;

                try
                {
                    Monitor.Enter(_lock);
                    ITerminalMessage? message = null;
                    IEnumerable<ITerminalLine> inputLines = Array.Empty<ITerminalLine>();
                    while (redraw || messages.TryDequeue(out message))
                    {
                        redraw = false;
                        if (!draw)
                        {
                            draw = true;

                            int width = Console.BufferWidth;

                            inputLines = GetTerminalLines();
                            int lastInputHeight = SplitLines(lastInputLines, width);
                            lastInputLines.Clear();
                            lastInputLines.AddRange(inputLines);

                            int top = Console.CursorTop;

                            Console.ResetColor();
                            Console.SetCursorPosition(0, top + 1 - lastInputHeight);
                            Console.Write(new string(' ', width * lastInputHeight));
                            Console.SetCursorPosition(0, top + 1 - lastInputHeight);
                        }
                        message?.WriteConsole();
                    }
                    if (draw)
                    {
                        Console.ResetColor();
                        bool writeNewLine = false;
                        foreach (ITerminalLine line in inputLines)
                        {
                            if (writeNewLine) Console.WriteLine();
                            writeNewLine = true;
                            line.WriteLine();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.ToString());
                    Console.ResetColor();
                    Environment.Exit(998);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
                await Task.Delay(10);
            }
        }

        public void WriteMessage(ITerminalMessage message) => messages.Enqueue(message);

        public Task Start() => Task.WhenAll(StartInput(), StartOutput());
    }
}
