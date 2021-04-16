using ContosoNotes.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ContosoNotes.Controls
{
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
                    return TextItemTemplate;
                }
                else
                {
                    return TextItemTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    public sealed partial class NotePageRenderer : UserControl
    {
        public static readonly DependencyProperty NotePageProperty =
            DependencyProperty.Register("NotePage", typeof(NotePageModel), typeof(NotePageRenderer), new PropertyMetadata(null));

        public NotePageModel NotePage
        {
            get => (NotePageModel)GetValue(NotePageProperty);
            set => SetValue(NotePageProperty, value);
        }
    }
}
