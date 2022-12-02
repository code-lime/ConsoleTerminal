using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTerminal.Terminal
{
    public class TerminalConsole
    {
        private struct MessageInfo
        {

        }
        private readonly ConcurrentQueue<MessageInfo> messages = new ConcurrentQueue<MessageInfo>();
        private readonly object _lock = new object();
        private string[] inputLines = new string[] { "" };

        private static async ValueTask<ConsoleKeyInfo> ReadKeyAsync(bool intercept = false)
        {
            while (!Console.KeyAvailable) await Task.Delay(10);
            return Console.ReadKey(intercept);
        }
        private static string ReadAppend(string line, bool intercept = false)
        {
            TextReader reader = Console.In;
            int ch = reader.Read();
            if (ch == -1) return line;
            StringBuilder builder = new StringBuilder(line);
            if (ch == '\r' || ch == '\n')
            {
                if (ch == '\r' && reader.Peek() == '\n') reader.Read();
                return line;
            }
            return builder.Append((char)ch).ToString();
        }

        /*private static string ReadKeyAppendAsync(string line)
        {
            int ch = Console.Read();
            if (ch == -1) return line;
            if (ch == '\r' || ch == '\n')
            {
                if (ch == '\r' && Peek() == '\n')
                {
                    Read();
                }

                return sb.ToString();
            }
            sb.Append((char)ch);
        }

        private static string? ReadLine()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                int ch = Read();
                if (ch == -1) break;
                if (ch == '\r' || ch == '\n')
                {
                    if (ch == '\r' && Peek() == '\n')
                    {
                        Read();
                    }

                    return sb.ToString();
                }
                sb.Append((char)ch);
            }
            if (sb.Length > 0)
            {
                return sb.ToString();
            }

            return null;
        }*/


        private async Task StartInput()
        {
            string line = "";
            while (true)
            {
                line = ReadAppend(line);
                Console.Title = $"{line} / {line.Length}"; 
                //ConsoleKeyInfo info = await ReadKeyAsync(true);
                //char ch = (char)Console.Read();//info.KeyChar;
                /*switch (ch)
                {
                    case '\b': line = line[..^1]; break;
                    default: line += ch; break;
                }
                Console.Write(ch);
                //await Console.In.ReadAsync(singleChar, 0, 1);
                //Console.WriteLine(singleChar[0]);*/
            }
        }
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
                    modify = modify[(width - 1)..];
                }
                else
                {
                    lines.Add(modify);
                    break;
                }
            }
            return lines;
        }
        private static List<string> SplitInput(string[] inputLines, int width)
        {
            List<string> lines = new List<string>();
            foreach (string line in inputLines) lines.AddRange(SplitInput(line, width));
            return lines;
        }
        private async Task StartOutput()
        {
            return;
            while (true)
            {
                bool draw = false;

                try
                {
                    while (messages.TryDequeue(out MessageInfo message))
                    {
                        if (!draw)
                        {
                            draw = true;

                            Monitor.Enter(_lock);
                            Console.CursorVisible = false;

                            int width = Console.BufferWidth;

                            List<string> splitInput = SplitInput(inputLines, width);
                            int inputHeight = splitInput.Count;

                            int top = Console.CursorTop;

                            Console.SetCursorPosition(0, top - inputHeight);
                            Console.Write(new string(' ', width * inputHeight));
                            Console.SetCursorPosition(0, top - inputHeight);
                        }
                    }
                }
                finally
                {
                    if (draw)
                    {
                        int lenght = inputLines.Length;
                        for (int i = 0; i < lenght; i++)
                        {
                            if (i == lenght - 1) Console.WriteLine(inputLines[i]);
                            else Console.Write(inputLines[i]);
                        }
                        Console.CursorVisible = true;

                        Monitor.Exit(_lock);
                    }
                }
                await Task.Delay(10);
            }
        }
        public Task Start() => Task.WhenAll(StartInput(), StartOutput());
    }
}
