using System.Windows.Controls;

namespace Locima.SlidingBlock.Controls
{
    public partial class Acknowledgements : UserControl
    {
        public Acknowledgements()
        {
            InitializeComponent();
            Loaded += new System.Windows.RoutedEventHandler(Acknowledgements_Loaded);
        }

        void Acknowledgements_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
//            AckListBox.Height = LayoutRoot.RowDefinitions[1].ActualHeight;
        }

        
    }
}