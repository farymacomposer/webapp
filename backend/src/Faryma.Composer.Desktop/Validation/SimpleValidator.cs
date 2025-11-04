using System.Numerics;
using Faryma.Composer.Desktop.Utils;

namespace Faryma.Composer.Desktop.Validation
{
    public sealed class SimpleValidator
    {
        public bool HasWarnings => Warnings.Count > 0;

        public List<string> Warnings { get; } = [];

        public SimpleValidator WarnIf(bool violationCondition, string warning)
        {
            if (violationCondition)
            {
                Warnings.Add(warning);
            }

            return this;
        }

        public SimpleValidator RequireNumber<T>(string? value, string warning) where T : INumber<T> =>
            WarnIf(!StringParser.TryParseNumber(value, out T? _), warning);

        public SimpleValidator RequireUrl(string? value, string warning) =>
            WarnIf(!Uri.TryCreate(value, UriKind.Absolute, out _), warning);

        public SimpleValidator RequireNumberIfProvided<T>(string? value, string warning) where T : INumber<T>
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return this;
            }

            return RequireNumber<T>(value, warning);
        }

        public SimpleValidator RequireUrlIfProvided(string? value, string warning)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return this;
            }

            return RequireUrl(value, warning);
        }
    }
}