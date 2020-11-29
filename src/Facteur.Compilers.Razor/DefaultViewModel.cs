using System.Diagnostics.CodeAnalysis;

namespace Facteur
{
    /// <summary>
    /// Represents a view model for default templates
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DefaultViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewModel"/> class
        /// </summary>
        public DefaultViewModel()
        {
        }

        public string Text { get; set; }
    }
}