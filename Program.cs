using ConsoleTerminal.Terminal;

public static class Program
{
    public static void Log(string text)
    {

    }

    public static void Main(string[] args)
    {
        new TerminalConsole().Start().Wait();
        /*Thread thread = new Thread(() =>
        {
            while (true)
            {
                Log("Current tick: " + System.DateTime.Now.Ticks);
                Thread.Sleep(100);
            }
        });
        while (true)
        {
            Console.ReadKey();
        }*/
    }
}