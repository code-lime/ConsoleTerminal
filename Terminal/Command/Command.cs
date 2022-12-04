#nullable enable

namespace Terminal.Command
{
    public interface ICommand
    {
        string Name { get; }
        IEnumerable<string> GetTab(string argLine, out int index);
    }
}
