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