using ContosoNotes.Models;
using Microsoft.Toolkit.Uwp.UI;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ContosoNotes.UI
{
    public sealed partial class NotePageRenderer : UserControl
    {
        public static readonly DependencyProperty NotePageProperty =
            DependencyProperty.Register(nameof(NotePage), typeof(NotePageModel), typeof(NotePageRenderer), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for DeleteTaskCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeleteTaskCommandProperty =
            DependencyProperty.Register(nameof(DeleteTaskCommand), typeof(ICommand), typeof(NotePageRenderer), new PropertyMetadata(null));

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

        public NotePageRenderer()
        {
            InitializeComponent();

            KeyUp += OnKeyUp;
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            TextBox textBox = e.OriginalSource as TextBox;

            if (e.Key == VirtualKey.Enter && textBox != null)
            {
                TaskNoteItemView taskNoteItemView = textBox.FindParent<TaskNoteItemView>();

                if (taskNoteItemView != null)
                {
                    taskNoteItemView.TaskNoteItem.Save();

                    if (textBox.SelectionStart == textBox.Text.Length)
                    {
                        // Find the correct index for insertion.
                        int itemIndex = NotePage.NoteItems.IndexOf(taskNoteItemView.TaskNoteItem);

                        // Insert new task after focused element
                        NotePage.NoteItems.Insert(itemIndex + 1, new TaskNoteItemModel());

                        e.Handled = true;
                    }
                }
            }
            else if (e.Key == VirtualKey.Back && textBox != null && string.IsNullOrEmpty(textBox.Text))
            {
                TaskNoteItemView taskNoteItemView = textBox.FindParent<TaskNoteItemView>();
                if (taskNoteItemView != null)
                {
                    DeleteTaskCommand.Execute(taskNoteItemView.TaskNoteItem);
                }
            }
        }
    }
}
