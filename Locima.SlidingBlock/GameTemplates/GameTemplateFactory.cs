using System;
using System.IO;

namespace Locima.SlidingBlock.GameTemplates
{

    /// <summary>
    /// Base implementation of <see cref="IGameTemplateFactory"/> that adds helper methods
    /// </summary>
    public abstract class GameTemplateFactory : IGameTemplateFactory
    {

        /// <inheritdoc/>
        protected abstract string TemplatePath { get; }

        /// <summary>
        /// Creates the Uri for the image filename passed, based on this template
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        protected Uri CreateImageUri(string filename)
        {
            return new Uri(Path.Combine(TemplatePath, filename),UriKind.Relative);
        }

        /// <inheritdoc/>
        public abstract string PersistentId { get; }

        /// <inheritdoc/>
        public abstract GameTemplate Create();
    }
}
