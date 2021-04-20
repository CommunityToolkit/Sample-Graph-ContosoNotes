using ContosoNotes.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.Controls
{
    public sealed partial class NotePageRenderer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty NotePageProperty =
            DependencyProperty.Register("NotePage", typeof(NotePageModel), typeof(NotePageRenderer), new PropertyMetadata(null, OnNotePageChanged));

        private static void OnNotePageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NotePageRenderer notePageRenderer)
            {
                var notePage = e.NewValue as NotePageModel;

                if (notePage.NoteItems.Count == 0 || notePage.NoteItems[notePage.NoteItems.Count - 1] is TaskNoteItemModel)
                {
                    // If the last note is not a text note, add an empty one to get things started.
                    notePageRenderer.NotePage.NoteItems.Add(new NoteItemModel());
                }
            }
        }

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
        }

        private void SetValueDP(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NoteItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextItemTemplate { get; set; }

        public DataTemplate TaskItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is NoteItemModel noteItem)
            {
                if (noteItem is TaskNoteItemModel)
                {
                    return TaskItemTemplate;
                }
                else
                {
                    return TextItemTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
