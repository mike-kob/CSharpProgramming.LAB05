using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LAB05.Tools.Managers;
using LAB05.Tools.Navigation;
using LAB05.ViewModels;

namespace LAB05.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window, IContentOwner
    {
        public ContentControl ContentControl
        {
            get { return _contentControl; }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            //.Instance.Initialize();
            NavigationManager.Instance.Initialize(new InitializationNavigationModel(this));
            NavigationManager.Instance.Navigate(ViewType.DataView);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            StationManager.CloseApp();
        }
    }

}
