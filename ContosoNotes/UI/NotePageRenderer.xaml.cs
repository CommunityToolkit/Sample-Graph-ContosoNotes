using ContosoNotes.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.UI
{
    public sealed partial class NotePageRenderer : UserControl
    {
        public static readonly DependencyProperty NotePageProperty =
            DependencyProperty.Register(nameof(NotePage), typeof(NotePageModel), typeof(NotePageRenderer), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        public NotePageModel NotePage
        {
            get => (NotePageModel)GetValue(NotePageProperty);
            set => SetValue(NotePageProperty, value);
        }

        public NotePageRenderer()
        {
            InitializeComponent();

            KeywordDetector.Instance.RegisterKey(Windows.System.VirtualKey.Up);
            KeywordDetector.Instance.RegisterKey(Windows.System.VirtualKey.Down);
        }
    }
}
