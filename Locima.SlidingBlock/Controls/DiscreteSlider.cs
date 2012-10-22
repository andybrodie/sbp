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

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (!_busy)
            {
                try
                {
                    _busy = true;

                    if (SmallChange > 0)
                    {
                        double newDiscreteValue = (int)(Math.Round(newValue / SmallChange)) * SmallChange;

                        if (Math.Abs(newDiscreteValue - Value) > 0.000001 || Math.Abs(Value - oldValue) > 0.000001)
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