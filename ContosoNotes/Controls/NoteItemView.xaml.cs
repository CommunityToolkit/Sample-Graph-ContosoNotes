using ContosoNotes.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.Controls
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
                noteItemView.RenderNoteItem(noteItemModel);
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

        private void RenderNoteItem(NoteItemModel noteItemModel)
        {
            //NoteTextBlock.Text = noteItemModel?.Text;
        }

        private void SetValueDP(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
