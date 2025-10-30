using System.Numerics;
using Faryma.Composer.Desktop.Utils;

namespace Faryma.Composer.Desktop.Validation
{
    public sealed class SimpleValidator
    {
        private readonly List<(bool Condition, string Warning)> _warnings = [];

        public bool HasWarnings => _warnings.Any(x => x.Condition);

        public List<string> Warnings => _warnings
            .Where(x => x.Condition)
            .Select(x => x.Warning)
            .ToList();

        public SimpleValidator Check(bool condition, string warning)
        {
            _warnings.Add((condition, warning));

            return this;
        }

        public SimpleValidator CheckNumber<T>(object? value, string warning) where T : INumber<T>
        {
            _warnings.Add((!StringParser.TryParseNumber(value, out T? _), warning));

            return this;
        }

        public SimpleValidator CheckOptionalNumber<T>(object? value, string warning) where T : INumber<T>
        {
            if (value is null || (value is string str && string.IsNullOrEmpty(str)))
            {
                return this;
            }

            return CheckNumber<T>(value, warning);
        }
    }
}