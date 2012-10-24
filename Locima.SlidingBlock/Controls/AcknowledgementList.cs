using System.Collections.ObjectModel;

namespace Locima.SlidingBlock.Controls
{
    /// <summary>
    /// This class exists purely because the version of Silverlight in WP7.1 doesn't allow the use of the <c>TypeParam</c> attribute in XAML.  Because I wanted to declare my <see cref="Acknowledgement"/>
    /// instances within XAML (see <see cref="Acknowledgements"/> I had to do it this way. 
    /// </summary>
    /// <remarks>
    /// Yuk.</remarks>
    public class AcknowledgementList : ObservableCollection<Acknowledgement>
    {
    }
}