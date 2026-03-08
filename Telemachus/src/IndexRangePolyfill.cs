// Polyfills for C# 8 Index/Range operators on .NET Framework 4.7.2
// These types are built-in on .NET Core 3+ but must be provided manually for older targets.

namespace System
{
    internal readonly struct Index
    {
        private readonly int _value;

        public Index(int value, bool fromEnd = false)
        {
            _value = fromEnd ? ~value : value;
        }

        public int Value => _value < 0 ? ~_value : _value;
        public bool IsFromEnd => _value < 0;

        public int GetOffset(int length) =>
            IsFromEnd ? length - Value : Value;

        public static implicit operator Index(int value) => new Index(value);
    }

    internal readonly struct Range
    {
        public Index Start { get; }
        public Index End { get; }

        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }

        public static Range StartAt(Index start) => new Range(start, new Index(0, true));
        public static Range EndAt(Index end) => new Range(new Index(0), end);
        public static Range All => new Range(new Index(0), new Index(0, true));

        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            int start = Start.GetOffset(length);
            int end = End.GetOffset(length);
            return (start, end - start);
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class RuntimeHelpers2
    {
        // Required by the compiler for Index/Range support on strings/arrays
    }
}
