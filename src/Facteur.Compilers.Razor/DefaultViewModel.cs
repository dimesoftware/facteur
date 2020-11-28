using System.Diagnostics.CodeAnalysis;

namespace Facteur
{
    /// <summary>
    /// Represents a view model for default templates
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class DefaultViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewModel"/> class
        /// </summary>
        internal DefaultViewModel()
        {
        }

        internal string Text { get; set; }
    }
}