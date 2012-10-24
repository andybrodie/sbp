using System;
using System.Windows;
using Locima.SlidingBlock.ViewModel;

namespace Locima.SlidingBlock.Controls
{
    /// <summary>
    ///   Selects the appropriate template for showing a high score item (<see cref="HighScoreItemViewModel" />) for use within <see
    ///    cref="HighScores" />
    /// </summary>
    /// <remarks>
    ///   There are two templates defined that allow for "normal" entries and "highlighted" entries.  There is usually just one highlighted entry, which is the entry just created by the player.
    /// </remarks>
    public class HighscoreTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        ///   The template to use for highlighted entries
        /// </summary>
        public DataTemplate Highlighted { get; set; }

        /// <summary>
        ///   The template to use for normal entries
        /// </summary>
        public DataTemplate Normal { get; set; }

        /// <summary>
        ///   Selects either the <see cref="Highlighted" /> template or <see cref="Normal" /> template, depending on the value of <see
        ///    cref="HighScoreItemViewModel.IsHighlighted" />
        /// </summary>
        /// <param name="item"> The <see cref="HighScoreItemViewModel" /> that is to be rendered </param>
        /// <param name="container"> <c>this</c> </param>
        /// <returns>
        ///   <para> Either <see cref="Highlighted" /> or <see cref="Normal" /> . If no value for <see
        ///    cref="HighScoreItemViewModel.IsHighlighted" /> could be found then this defers to <see
        ///    cref="DataTemplateSelector.SelectTemplate" /> (this returns <c>null</c> which means that the item won't be rendered. </para>
        ///   <para> Serves you right for including an object that wasn't an instance of <see cref="HighScoreItemViewModel" />! </para>
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            HighScoreItemViewModel model = item as HighScoreItemViewModel;

            // Set to whether we want the "Highlighted" template (true), "Normal" template (false) or don't care (null)
            Boolean? isHighlighted;

            // Sample data in the designer uses a faked MenuItemViewModel
            if (model == null)
            {
                if (item != null)
                {
                    string isEnabledString = RetrieveDesignerProperty(item, "IsHighlighted");
                    bool isHighlightedPrimitive;
                    if (Boolean.TryParse(isEnabledString, out isHighlightedPrimitive))
                    {
                        isHighlighted = isHighlightedPrimitive;
                    }
                    else
                    {
                        isHighlighted = null;
                    }
                }
                else
                {
                    isHighlighted = null;
                }
            }
            else
                // The item passed to the template selector was an instance of MenuItemViewModel, so in that case just use the value in the IsEnabled property
            {
                isHighlighted = model.IsHighlighted;
            }

            return isHighlighted.HasValue
                       ? (isHighlighted.Value ? Highlighted : Normal)
                       : base.SelectTemplate(item, container);
        }
    }
}