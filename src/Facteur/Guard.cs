using System;
using System.Collections.Generic;
using System.Linq;

namespace Facteur
{
    internal static class Guard
    {
        internal static void ThrowIfNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);
        }

        internal static void ThrowIfNullOrEmpty(string argumentValue, string argumentName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                throw new ArgumentNullException(argumentName);
        }

        internal static void ThrowIfNullOrEmpty(IEnumerable<object> argumentValue, string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);

            if (!argumentValue.Any())
                throw new ArgumentNullException(argumentName);
        }
    }
}