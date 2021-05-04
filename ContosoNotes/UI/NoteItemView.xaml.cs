using ContosoNotes.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.UI
{
    public sealed partial class NoteItemView : UserControl
    {
        public static readonly DependencyProperty NoteItemProperty =
            DependencyProperty.Register(nameof(NoteItem), typeof(NoteItemModel), typeof(NoteItemView), new PropertyMetadata(null));

        public NoteItemModel NoteItem
        {
            get => (NoteItemModel)GetValue(NoteItemProperty);
            set => SetValue(NoteItemProperty, value);
        }

        public NoteItemView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NoteItem != null)
            {
                KeywordDetector.Instance.Analyse(NoteItem);
            }
        }
    }
}
