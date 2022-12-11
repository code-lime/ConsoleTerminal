using Terminal;
using Terminal.Command;

public static class Program
{
    public class SimpleCommand : ICommand
    {
        public string Name { get; }
        public SimpleCommand(string name)
        {
            this.Name = name;
        }


        public IEnumerable<string> GetTab(string argLine, out int index)
        {
            throw new NotImplementedException();
        }
    }

    public static void Main(string[] args)
    {
        TerminalConsole terminal = new TabTerminalConsole(new ICommand[]
        {
            new SimpleCommand("tp"),
            new SimpleCommand("save"),
            new SimpleCommand("load"),
            new SimpleCommand("load.all"),
            new SimpleCommand("load.any1"),
            new SimpleCommand("load.an2y"),
            new SimpleCommand("load.an3y"),
            new SimpleCommand("load.any4")
        });
        /*terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
        terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));*/
        new Thread(() =>
        {
            for (int i = 0; i < 10000; i++)
            {
                terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
            }
            while (true)
            {
                terminal.WriteMessage(new TerminalMessage(ConsoleColor.Yellow, "WARN", "Current tick: " + System.DateTime.Now.Ticks));
                Thread.Sleep(100);
            }
        })
        {
            IsBackground = true
        }.Start();
        terminal.Start().Wait();
    }
}