using ContosoNotes.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.UI
{
    public sealed partial class NoteItemView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty NoteItemProperty =
            DependencyProperty.Register(nameof(NoteItem), typeof(NoteItemModel), typeof(NoteItemView), new PropertyMetadata(null, OnNoteItemChanged));

        private static void OnNoteItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NoteItemView noteItemView)
            {
                var noteItemModel = e.NewValue as NoteItemModel;
                noteItemView.NoteItemTextBox.Text = noteItemModel?.Text ?? string.Empty;
                
                noteItemModel.PropertyChanged -= noteItemView.NoteItemModel_PropertyChanged;
                noteItemModel.PropertyChanged += noteItemView.NoteItemModel_PropertyChanged;
            }
        }

        private void NoteItemModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is NoteItemModel noteItem && e.PropertyName == nameof(NoteItemModel.Text))
            {
                NoteItemTextBox.Text = noteItem.Text;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NoteItemModel NoteItem
        {
            get => (NoteItemModel)GetValue(NoteItemProperty);
            set => SetValueDP(NoteItemProperty, value);
        }

        public NoteItemView()
        {
            InitializeComponent();
            (Content as FrameworkElement).DataContext = this;
        }

        private void SetValueDP(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (NoteItem != null)
            {
                // Set the model manually, otherwise the value won't update until the user leaves the TextBox.
                NoteItem.Text = sender.Text;

                KeywordDetector.Instance.Analyse(NoteItem);
            }
        }

        private void NoteItemTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (NoteItem != null && sender is TextBox noteItemTextBox)
            {
                KeywordDetector.Instance.Analyse(NoteItem, e.Key, noteItemTextBox.SelectionStart);
            }
        }
    }
}
