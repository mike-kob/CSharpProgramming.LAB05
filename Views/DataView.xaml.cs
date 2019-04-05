using System.Windows.Controls;
using LAB05.Tools.Navigation;
using LAB05.ViewModels;

namespace LAB05.Views
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl, INavigatable
    {
        public DataView()
        {
            InitializeComponent();
            DataContext = new DataViewModel();
        }
    }
}
