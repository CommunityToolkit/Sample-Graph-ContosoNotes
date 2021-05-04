using ContosoNotes.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.UI
{
    public sealed partial class TextNoteItemView : UserControl
    {
        public static readonly DependencyProperty NoteItemProperty =
            DependencyProperty.Register(nameof(NoteItem), typeof(NoteItemModel), typeof(TextNoteItemView), new PropertyMetadata(null));

        public NoteItemModel NoteItem
        {
            get => (NoteItemModel)GetValue(NoteItemProperty);
            set => SetValue(NoteItemProperty, value);
        }

        public TextNoteItemView()
        {
            InitializeComponent();
        }

        public async void FocusEnd(FocusState focusState)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //NoteItemTextBox.Focus(focusState);

                NoteItemTextBox.SelectionStart = NoteItemTextBox.Text.Length;
            });
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
