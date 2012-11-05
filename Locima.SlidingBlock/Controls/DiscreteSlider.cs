using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Locima.SlidingBlock.Controls
{

    /// <summary>
    /// Modification to the standard <see cref="Slider"/> control that snaps between discrete values (small increment/decrement set from <see cref="RangeBase.SmallChange"/>)
    /// </summary>
    public class DiscreteSlider : Slider
    {
        private bool _busy;
        private double _discreteValue;


        /// <summary>
        /// When the value changes, if the change is less that our discrete <see cref="RangeBase.SmallChange"/> then don't change the value.  If it's bigger then proceed normally
        /// </summary>
        /// <param name="oldValue">The old value of the slider</param>
        /// <param name="newValue">The new value of the slider</param>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            const double smallChangeDelta = 0.000001;

            if (!_busy)
            {
                try
                {
                    _busy = true;

                    if (SmallChange > 0)
                    {
                        double newDiscreteValue = (int)(Math.Round(newValue / SmallChange)) * SmallChange;

                        if (Math.Abs(newDiscreteValue - Value) > smallChangeDelta || Math.Abs(Value - oldValue) > smallChangeDelta)
                        {
                            Value = newDiscreteValue;
                            base.OnValueChanged(_discreteValue, newDiscreteValue);
                            _discreteValue = newDiscreteValue;
                        }
                    }
                    else
                    {
                        base.OnValueChanged(oldValue, newValue);
                    }
                }
                finally
                {
                    _busy = false;
                }
            }
        }
    }
}