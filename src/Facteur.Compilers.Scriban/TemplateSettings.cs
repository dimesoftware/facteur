using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Facteur
{
    /// <summary>
    ///
    /// </summary>
    [DebuggerStepThrough]
    [ExcludeFromCodeCoverage]
    public class TemplateSettings
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateSettings"/> class
        /// </summary>
        [DebuggerStepThrough]
        private TemplateSettings()
        {
        }

        #endregion Constructor

        #region Properties

        public string Extension { get; set; } = ".sbnhtml";
        public string RelativePath { get; set; } = Path.Combine("Templates");

        private static volatile TemplateSettings _instance;
        private static readonly object _syncRoot = new object();

        #endregion Properties

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static TemplateSettings Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_syncRoot)
                {
                    _instance ??= new TemplateSettings();
                }

                return _instance;
            }
        }

        #endregion Methods
    }
}