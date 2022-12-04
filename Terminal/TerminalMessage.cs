#nullable enable

namespace Terminal
{
    public interface ITerminalMessage
    {
        void WriteConsole();
    }
    public readonly struct TerminalMessage : ITerminalMessage
    {
        public readonly DateTime Time;
        public readonly ConsoleColor Color;
        public readonly string Action;
        public readonly string Message;

        public TerminalMessage(ConsoleColor color, string action, string message) : this(DateTime.Now, color, action, message) { }
        public TerminalMessage(DateTime time, ConsoleColor color, string action, string message)
        {
            this.Time = time;
            this.Color = color;
            this.Action = action;
            this.Message = message;
        }

        public void WriteConsole()
        {
            Console.ForegroundColor = Color;
            Console.WriteLine($"[{Time:HH:mm:ss}] {Action}: {Message}");
        }
    }
}
