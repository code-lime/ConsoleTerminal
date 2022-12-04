#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace Terminal
{
    public interface ITerminalLine : IEquatable<ITerminalLine>
    {
        int Lenght { get; }
        ITerminalLine Copy();
        //IEnumerable<ITerminalLine> Split(int width);
        int SplitLines(int width);
        void WriteLine();
    }
    public readonly struct TerminalLine : ITerminalLine, IEquatable<TerminalLine>
    {
        public readonly string Line;
        public readonly ConsoleColor Color;

        public int Lenght => Line.Length;

        public TerminalLine(string line) : this(line, ConsoleColor.White) { }
        public TerminalLine(string line, ConsoleColor color)
        {
            Line = line;
            Color = color;
        }

        ITerminalLine ITerminalLine.Copy() => Copy();
        public TerminalLine Copy() => new TerminalLine(Line, Color);

        public bool Equals(TerminalLine other) => other.Line == Line && other.Color == Color;
        public bool Equals(ITerminalLine? other) => other is TerminalLine _other && Equals(_other, this);
        public override bool Equals([NotNullWhen(true)] object? other) => other is ITerminalLine _other && Equals(_other);
        public override int GetHashCode() => HashCode.Combine(Line, Color);
        public static bool operator ==(TerminalLine left, TerminalLine right) => left.Equals(right);
        public static bool operator !=(TerminalLine left, TerminalLine right) => !(left == right);

        /*public IEnumerable<ITerminalLine> Split(int width)
        {
            string modify = Line;
            while (true)
            {
                int lenght = modify.Length;
                if (lenght > width)
                {
                    yield return new TerminalLine(modify[..(width - 1)], Color);
                    modify = modify[width..];
                }
                else
                {
                    yield return new TerminalLine(modify, Color);
                    break;
                }
            }
        }*/
        public int SplitLines(int width)
            => (Lenght - 1) / width + 1;
        public void WriteLine()
        {
            Console.ForegroundColor = Color;
            Console.Write(Line);
        }
    }
    public readonly struct InputLine : ITerminalLine, IEquatable<InputLine>
    {
        public readonly string Prefix;
        public readonly string Line;
        public readonly string CursorPostfix;
        public readonly ConsoleColor Color;
        public readonly ConsoleColor CursorColor;
        public readonly int CursorPosition;

        public int Lenght => Prefix.Length + Line.Length + CursorPostfix.Length;

        public InputLine(string prefix, string line, string cursorPostfix, ConsoleColor color, ConsoleColor cursorColor, int cursorPosition)
        {
            Prefix = prefix;
            Line = line;
            CursorPostfix = cursorPostfix;
            Color = color;
            CursorColor = cursorColor;
            CursorPosition = cursorPosition;
        }

        ITerminalLine ITerminalLine.Copy() => Copy();
        public InputLine Copy() => new InputLine(Prefix, Line, CursorPostfix, Color, CursorColor, CursorPosition);

        public bool Equals(InputLine other) =>  other.Prefix == Prefix &&  other.Line == Line &&  other.CursorPostfix == CursorPostfix && other.Color == Color && other.CursorColor == CursorColor && other.CursorPosition == CursorPosition;
        public bool Equals(ITerminalLine? other) => other is InputLine _other && Equals(_other, this);
        public override bool Equals([NotNullWhen(true)] object? other) => other is ITerminalLine _other && Equals(_other);
        public override int GetHashCode() => HashCode.Combine(Prefix, Line, CursorPostfix, Color, CursorColor, CursorPosition);
        public static bool operator ==(InputLine left, InputLine right) => left.Equals(right);
        public static bool operator !=(InputLine left, InputLine right) => !(left == right);

        public int SplitLines(int width) => (Lenght - 1) / width + 1;
        public void WriteLine()
        {
            Console.ForegroundColor = Color;
            Console.Write(Prefix);
            if (CursorPosition >= Line.Length)
            {
                Console.Write(Line);
                Console.ForegroundColor = CursorColor;
                Console.Write(CursorPostfix);
            }
            else
            {
                Console.Write(Line[..CursorPosition]);
                ConsoleColor bcg = Console.BackgroundColor;
                Console.BackgroundColor = CursorColor;
                Console.Write(Line[CursorPosition..(CursorPosition+1)]);
                Console.BackgroundColor = bcg;
                Console.ForegroundColor = Color;
                Console.Write(Line[(CursorPosition+1)..]);
            }
        }
    }
}
