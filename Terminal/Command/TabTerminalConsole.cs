#nullable enable

namespace Terminal.Command
{
    public class TabTerminalConsole : TerminalConsole
    {
        private int offsetIndex = 0;

        protected virtual int TabIndex { get; set; }
        protected virtual IEnumerable<ICommand> Commands { get; }

        public TabTerminalConsole(IEnumerable<ICommand> commands)
        {
            Commands = commands;
        }
        
        private IEnumerable<(string text, ITerminalLine line, int index)> GetPreviewList()
        {
            if (this.InputLine.Length == 0) yield break;
            int newIndex = TabIndex + offsetIndex;
            offsetIndex = 0;
            string[] split = this.InputLine.Split(' ', 2);
            string name = split[0];

            ICommand[] commands = Commands.Where(v => v.Name.StartsWith(name)).ToArray();
            int commandLenght = commands.Length;

            if (commandLenght > 0)
            {
                if (newIndex < 0) TabIndex = 0;
                else if (newIndex >= commandLenght) TabIndex = commandLenght - 1;
                else TabIndex = newIndex;

                if (split.Length == 1)
                {
                    int startIndex = Math.Max(TabIndex - 2, 0);
                    int endIndex = Math.Min(startIndex + 5, commandLenght);
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        ICommand command = commands[i];
                        yield return (command.Name, new TerminalLine(command.Name, i == TabIndex ? ConsoleColor.Yellow : ConsoleColor.Gray), i);
                    }
                }
            }
        }

        protected override IEnumerable<ITerminalLine> GetTerminalLines()
        {
            foreach ((_, ITerminalLine line, _) in GetPreviewList()) yield return line;
            foreach (ITerminalLine line in base.GetTerminalLines()) yield return line;
        }
        protected override bool OnConsoleExecute(ConsoleKeyInfo info)
        {
            switch (info.Key)
            {
                case ConsoleKey.DownArrow:
                    {
                        offsetIndex++;
                        Redraw();
                        return true;
                    }
                case ConsoleKey.UpArrow:
                    {
                        offsetIndex--;
                        Redraw();
                        return true;
                    }
                case ConsoleKey.Tab:
                    {

                        return true;
                    }
                case ConsoleKey.Escape:
                    {
                        InputLine = "";
                        Redraw();
                        return true;
                    }
                default: return base.OnConsoleExecute(info);
            }
        }
    }
}
