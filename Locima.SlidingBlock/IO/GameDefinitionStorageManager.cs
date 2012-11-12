using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Locima.SlidingBlock.GameTemplates;
using Locima.SlidingBlock.IO.IsolatedStorage;

namespace Locima.SlidingBlock.IO
{
    public class GameDefinitionStorageManager
    {

        public static IGameTemplateManager Instance { get; private set; }

        static GameDefinitionStorageManager()
        {
            Instance = new GameTemplateIsolatedStorageManager();
        }
    }

    public interface IGameTemplateManager
    {
        void Initialise();
        List<GameDefinition> GetCustomGameDefinitions();
        GameDefinition Load(string id);
        void Save(GameDefinition gameDefinition);
    }
}
