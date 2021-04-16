using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ContosoNotes.Views
{
    public sealed partial class MainPage : Page
    {
        private static readonly MainViewModel ViewModel = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Load(e.Parameter);
        }
    }
}
