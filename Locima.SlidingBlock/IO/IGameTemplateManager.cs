using System.Collections.Generic;
using Locima.SlidingBlock.GameTemplates;

namespace Locima.SlidingBlock.IO
{
    public interface IGameTemplateManager
    {
        void Initialise();
        List<GameTemplate> GetGameTemplates();
        GameTemplate Load(string id);
        void Save(GameTemplate gameTemplate);
    }
}