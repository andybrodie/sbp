using System.Collections.Generic;
using Locima.SlidingBlock.GameTemplates;

namespace Locima.SlidingBlock.IO
{
    public interface IGameTemplateManager
    {
        void Initialise();
        List<GameTemplate> GetGameTemplates(bool includeShadows, bool collapseShadows);
        GameTemplate Load(string id);
        void Save(GameTemplate gameTemplate);
        List<string> GetGameTemplateIds();
        void Delete(string id);

        /// <summary>
        /// Overwrites the object ("promotes") the object that this one is shadowing
        /// </summary>
        /// <param name="shadow">The shadow template to promote</param>
        /// <returns>The ID of the object this has been promoted to</returns>
        string PromoteShadow(GameTemplate shadow);

    }
}