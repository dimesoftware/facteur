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
            => Resolve(typeof(T).Name);

        /// <summary>
        /// Resolves the model type to its corresponding template file name
        /// </summary>
        /// <param name="model">The model instance</param>
        /// <typeparam name="T">The view model</typeparam>
        /// <returns>The file name</returns>
        public string Resolve<T>(T model)
            => Resolve(model.GetType().Name);

        /// <summary>
        /// Resolves the model type to its corresponding template file name
        /// </summary>
        /// <param name="typeName">The model instance</param>
        /// <returns>The file name</returns>
        private static string Resolve(string typeName)
        {
            int index = FindTemplateNameIndex(typeName);
            return typeName.Remove(index, typeName.Length - index);
        }

        /// <summary>
        /// Finds the index of the mail/view model suffix
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static int FindTemplateNameIndex(string typeName)
            => typeName.Contains("MailModel")
                ? typeName.IndexOf("MailModel", StringComparison.Ordinal)
                : typeName.IndexOf("ViewModel", StringComparison.Ordinal);
    }
}