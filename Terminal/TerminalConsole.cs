#nullable enable
using System.Collections.Concurrent;
using System.Text;

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
            bool tick = DateTime.Now.Ticks / (TimeSpan.TicksPerSecond / 2) % 2 == 0;
            yield return new InputLine(InputPrefix, InputLine, InputCursor, ConsoleColor.White, tick ? ConsoleColor.Gray : ConsoleColor.DarkGray, InputPosition);
        }

        /// <returns>true - Skip input, false - Call input</returns>
        protected virtual bool OnConsoleExecute(ConsoleKeyInfo info)
        {
            switch (info.Key)
            {
                case ConsoleKey.RightArrow:
                    InputPosition = Math.Min(InputLine.Length, InputPosition + 1);
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
                text = text.Insert(InputPosition, ch.ToString());
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

        private List<int> cacheInput = new List<int>();
        private async Task StartOutput()
        {
            while (true)
            {
                bool draw = false;

                try
                {
                    Console.Title = $"Top: {Console.CursorTop}";

                    Monitor.Enter(_lock);
                    ITerminalMessage? message = null;
                    IEnumerable<ITerminalLine> inputLines = Array.Empty<ITerminalLine>();
                    redraw = true;
                    int top = 0;
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

                            top = Console.CursorTop;

                            Console.ResetColor();
                            //Console.SetCursorPosition(0, top + 1 - lastInputHeight);
                            //Console.Write(cacheInput);
                            //Console.Write(new string(' ', width * lastInputHeight));
                            Console.SetCursorPosition(0, top + 1 - lastInputHeight);
                        }
                        message?.WriteConsole();
                    }
                    if (draw)
                    {
                        Console.ResetColor();
                        List<int> cacheBuilder = new List<int>();
                        int newTop = Console.CursorTop;
                        int offset_i = newTop - top;
                        top = newTop;
                        foreach (ITerminalLine line in inputLines)
                        {
                            int i = cacheBuilder.Count;
                            if (i > 0) Console.WriteLine();
                            int inLine = line.WriteLine();

                            int old_i = i + offset_i;

                            top = Console.CursorTop;
                            if (cacheInput.Count > old_i && old_i >= 0)
                            {
                                int inOldLine = cacheInput[old_i];
                                if (inOldLine > inLine)
                                {
                                    int delta = inOldLine - inLine;
                                    Console.ResetColor();
                                    Console.Write(new string('*', delta));
                                }
                            }
                            cacheBuilder.Add(inLine);
                        }
                        for (int i = Math.Max(0, cacheBuilder.Count + offset_i); i < cacheInput.Count; i++)
                        {
                            Console.Write('\n' + new string('%', cacheInput[i]));
                        }
                        Console.SetCursorPosition(0, top);
                        cacheInput = cacheBuilder;
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
                await Task.Delay(50);
            }
        }

        public void WriteMessage(ITerminalMessage message) => messages.Enqueue(message);

        public Task Start() => Task.WhenAll(StartInput(), StartOutput());
    }
}
