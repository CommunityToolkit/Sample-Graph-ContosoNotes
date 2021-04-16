using ContosoNotes.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.Controls
{
    public sealed partial class TaskNoteItemView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TaskNoteItemProperty =
            DependencyProperty.Register(nameof(TaskNoteItem), typeof(TaskNoteItemModel), typeof(TaskNoteItemView), new PropertyMetadata(null, OnTaskNoteItemChanged));

        private static void OnTaskNoteItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskNoteItemView taskNoteItemView)
            {
                var noteItem = e.NewValue as TaskNoteItemModel;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NoteItemModel TaskNoteItem
        {
            get => (NoteItemModel)GetValue(TaskNoteItemProperty);
            set => SetValueDP(TaskNoteItemProperty, value);
        }

        private void SetValueDP(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
