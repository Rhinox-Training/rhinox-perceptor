using System.Text;
using UnityEngine;


namespace Rhinox.Perceptor
{
    public class CompositeMessageBuilder : ILogMessageBuilder
    {
        private readonly ILogMessageBuilder[] _builders;
        private readonly string _delimiter;

        public CompositeMessageBuilder(string delimiter = "#", params ILogMessageBuilder[] builders)
        {
            _delimiter = delimiter;
            _builders = builders;
        }

        public string BuildMessage(LogLevels level, string message, Object associatedObject = null)
        {
            if (_builders == null || _builders.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            for (int index = 0; index < _builders.Length; ++index)
            {
                sb.Append(_builders[index].BuildMessage(level, message, associatedObject));
                if (index < _builders.Length - 1)
                    sb.Append(_delimiter);
            }

            return sb.ToString();
        }
    }

    public static class CompositeMessageBuilderExtensions
    {
        public static ILogMessageBuilder Append(this ILogMessageBuilder builder, ILogMessageBuilder otherBuilder,
            string delimiter = "")
        {
            return new CompositeMessageBuilder(delimiter, builder, otherBuilder);
        }
    }
}