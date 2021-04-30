using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ContosoNotes.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Load();
        }
    }
}
