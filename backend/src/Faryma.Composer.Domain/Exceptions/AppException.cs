using System.Text;

namespace Faryma.Composer.Domain.Exceptions
{
    public abstract class AppException(string? message) : Exception(message)
    {
        public object? Context { get; protected set; }

        public virtual int StatusCode => 409;

        public virtual string Code => ToSnakeCase(GetType().Name.Replace("Exception", string.Empty, StringComparison.Ordinal));

        private static string ToSnakeCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new(value.Length + 4);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (i > 0 && char.IsUpper(c) && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1])))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString();
        }
    }
}
