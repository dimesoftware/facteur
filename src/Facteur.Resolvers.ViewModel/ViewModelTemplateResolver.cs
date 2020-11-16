using System;

namespace Facteur
{
    /// <summary>
    /// Represents a template resolver that matches the template by the view model class name
    /// </summary>
    public class ViewModelTemplateResolver : ITemplateResolver
    {
        /// <summary>
        /// Resolves the model type to its corresponding template file name
        /// </summary>
        /// <typeparam name="T">The view model</typeparam>
        /// <returns>The file name</returns>
        public string Resolve<T>()
        {
            int index = FindTemplateNameIndex<T>();
            return typeof(T).Name.Remove(index, typeof(T).Name.Length - index);
        }

        public string Resolve<T>(T model) => Resolve<T>();

        private static int FindTemplateNameIndex<T>() 
            => typeof(T).Name.Contains("MailModel")
                ? typeof(T).Name.IndexOf("MailModel", StringComparison.Ordinal)
                : typeof(T).Name.IndexOf("ViewModel", StringComparison.Ordinal);
    }
}