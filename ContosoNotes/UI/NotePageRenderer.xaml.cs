using ContosoNotes.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.UI
{
    public sealed partial class NotePageRenderer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty NotePageProperty =
            DependencyProperty.Register("NotePage", typeof(NotePageModel), typeof(NotePageRenderer), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        public NotePageModel NotePage
        {
            get => (NotePageModel)GetValue(NotePageProperty);
            set => SetValueDP(NotePageProperty, value);
        }

        public NotePageRenderer()
        {
            InitializeComponent();
            (Content as FrameworkElement).DataContext = this;

            KeywordDetector.Instance.RegisterKey(Windows.System.VirtualKey.Up);
            KeywordDetector.Instance.RegisterKey(Windows.System.VirtualKey.Down);
        }

        private void SetValueDP(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
