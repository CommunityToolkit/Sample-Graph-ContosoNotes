using ContosoNotes.Models;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.UI
{
    public sealed partial class NotePageRenderer : UserControl
    {
        public static readonly DependencyProperty NotePageProperty =
            DependencyProperty.Register(nameof(NotePage), typeof(NotePageModel), typeof(NotePageRenderer), new PropertyMetadata(null));

        public NotePageModel NotePage
        {
            get => (NotePageModel)GetValue(NotePageProperty);
            set => SetValue(NotePageProperty, value);
        }

        public ICommand DeleteTaskCommand
        {
            get { return (ICommand)GetValue(DeleteTaskCommandProperty); }
            set { SetValue(DeleteTaskCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeleteTaskCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeleteTaskCommandProperty =
            DependencyProperty.Register(nameof(DeleteTaskCommand), typeof(ICommand), typeof(NotePageRenderer), new PropertyMetadata(null));

        public NotePageRenderer()
        {
            InitializeComponent();

            KeywordDetector.Instance.RegisterKey(Windows.System.VirtualKey.Up);
            KeywordDetector.Instance.RegisterKey(Windows.System.VirtualKey.Down);
        }
    }
}
