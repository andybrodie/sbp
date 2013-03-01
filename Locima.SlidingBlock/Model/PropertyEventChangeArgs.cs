using System;
using System.ComponentModel;

namespace Locima.SlidingBlock.Model
{

    /// <summary>
    /// Used to pass messages between the model and view model in a similar model to that provided <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public class PropertyEventChangeArgs : EventArgs
    {
        /// <summary>
        /// The name of the property that has been updated
        /// </summary>
        public string Name { get; set; }
    }
}