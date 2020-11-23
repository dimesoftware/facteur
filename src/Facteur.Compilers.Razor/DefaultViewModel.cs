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

        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Action { get; set; }
    }
}