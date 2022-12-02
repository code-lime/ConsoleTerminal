using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTerminal.Terminal
{
    public abstract class SimpleTerminalConsole
    {
        protected abstract bool IsRunning { get; }
        protected abstract void RunCommand(string var1);
        protected abstract void Shutdown();

        protected void ProcessInput(string input)
        {
            string command = input.Trim();
            if (!string.IsNullOrEmpty(command)) RunCommand(command);
        }

        public void Start() => ReadCommands(Console.In);
        private void ReadCommands(TextReader reader)
        {
            string? line;
            while (IsRunning && (line = reader.ReadLine()) != null)
                ProcessInput(line);
        }
    }
}
