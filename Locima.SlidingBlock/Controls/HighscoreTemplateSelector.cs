using System;
using System.Windows;
using Locima.SlidingBlock.ViewModel;

namespace Locima.SlidingBlock.Controls
{
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