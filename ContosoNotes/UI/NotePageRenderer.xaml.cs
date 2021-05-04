using ContosoNotes.Models;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections;
using System.Collections.Specialized;
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
            DependencyProperty.Register(nameof(NotePage), typeof(NotePageModel), typeof(NotePageRenderer), new PropertyMetadata(null, OnNotePageChanged));

        private static void OnNotePageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NotePageRenderer notePageRenderer)
            {
                if (e.OldValue is NotePageModel oldNotePageModel)
                {
                    oldNotePageModel.NoteItems.CollectionChanged -= notePageRenderer.OnNoteItemsCollectionChanged;
                }

                if (e.NewValue is NotePageModel newNotePageModel)
                {
                    newNotePageModel.NoteItems.CollectionChanged += notePageRenderer.OnNoteItemsCollectionChanged;
                }
            }
        }

        private void OnNoteItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count == 1)
            {
                NoteItemModel newItem = e.NewItems[0] as NoteItemModel;

                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (NoteItemsListView.ContainerFromItem(newItem) is FrameworkElement newItemContainer)
                    {
                        var newTaskNoteItemView = newItemContainer.FindDescendant<TaskNoteItemView>();

                        if (newTaskNoteItemView != null)
                        {
                            newTaskNoteItemView.Focus(FocusState.Programmatic);
                        }
                        else
                        {
                            TextNoteItemView newTextNoteItem = newItemContainer.FindDescendant<TextNoteItemView>();
                            newTextNoteItem?.FocusEnd(FocusState.Programmatic);
                        }
                    }
                });
            }
        }

        public NotePageModel NotePage
        {
            get => (NotePageModel)GetValue(NotePageProperty);
            set => SetValue(NotePageProperty, value);
        }

        // Using a DependencyProperty as the backing store for DeleteTaskCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeleteTaskCommandProperty =
            DependencyProperty.Register(nameof(DeleteTaskCommand), typeof(ICommand), typeof(NotePageRenderer), new PropertyMetadata(null));

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
                if (textBox.FindParent<TaskNoteItemView>() is TaskNoteItemView taskNoteItemView)
                {
                    taskNoteItemView.TaskNoteItem.Save();

                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        // Find the correct index for insertion.
                        int newItemIndex = NotePage.NoteItems.IndexOf(taskNoteItemView.TaskNoteItem);

                        // Remove the empty task
                        NotePage.NoteItems.Remove(taskNoteItemView.TaskNoteItem);

                        if (newItemIndex == NotePage.NoteItems.Count)
                        {
                            // Insert a new text item after target element
                            NotePage.NoteItems.Insert(newItemIndex, new NoteItemModel());
                        }

                        e.Handled = true;
                    }
                    else if (textBox.SelectionStart == textBox.Text.Length)
                    {
                        // Find the correct index for insertion.
                        int newItemIndex = NotePage.NoteItems.IndexOf(taskNoteItemView.TaskNoteItem) + 1;

                        // Insert new task after focused element
                        NotePage.NoteItems.Insert(newItemIndex, new TaskNoteItemModel());
                        
                        e.Handled = true;
                    }
                }
            }
            else if (e.Key == VirtualKey.Back && textBox != null && string.IsNullOrEmpty(textBox.Text))
            {
                if (textBox.FindParent<TaskNoteItemView>() is TaskNoteItemView taskNoteItemView)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        FocusPrevious(taskNoteItemView.TaskNoteItem);

                        DeleteTaskCommand.Execute(taskNoteItemView.TaskNoteItem);
                    });
                }
                else if (textBox.FindParent<TextNoteItemView>() is TextNoteItemView textNoteItemView)
                {
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (textBox.SelectionStart == 0)
                        {
                            int targetItemIndex = NoteItemsListView.Items.IndexOf(textNoteItemView.NoteItem);

                            // I'm at the start of the text box.
                            // I either want to merge any test with the previous text/task
                            // or just disappear if empty
                            if (!string.IsNullOrEmpty(textBox.Text))
                            {
                                // Grab any text and add it to the previous item,
                                int previousItemIndex = Math.Max(targetItemIndex - 1, 0);

                                if (NoteItemsListView.ContainerFromIndex(previousItemIndex) is DependencyObject previousItemContainer)
                                {
                                    if (previousItemContainer.FindDescendant<TaskNoteItemView>() is TaskNoteItemView previousTaskNoteItem)
                                    {
                                        previousTaskNoteItem.TaskNoteItem.Text += textBox.Text;
                                        previousTaskNoteItem.Focus(FocusState.Programmatic);
                                    }
                                    else if (previousItemContainer.FindDescendant<TextNoteItemView>() is TextNoteItemView previousTextNoteItem)
                                    {
                                        previousTextNoteItem.NoteItem.Text += textBox.Text;
                                        previousTextNoteItem.Focus(FocusState.Programmatic);
                                    }
                                }
                            }
                            else
                            {
                                FocusPrevious(textNoteItemView.NoteItem);
                            }

                            // Delete the text item
                            (NoteItemsListView.ItemsSource as IList).RemoveAt(targetItemIndex);
                        }
                    });
                }
            }
        }

        public void FocusPrevious(NoteItemModel noteItem)
        {
            int targetItemIndex = NotePage.NoteItems.IndexOf(noteItem);
            int previousItemIndex = Math.Max(targetItemIndex - 1, 0);
            var previousItemContainer = NoteItemsListView.ContainerFromIndex(previousItemIndex) as FrameworkElement;

            Control previousTaskNoteItem = previousItemContainer.FindDescendant<TaskNoteItemView>();
            if (previousTaskNoteItem != null)
            {
                previousTaskNoteItem.Focus(FocusState.Programmatic);
            }
            else
            {
                TextNoteItemView previousTextNoteItem = previousItemContainer.FindDescendant<TextNoteItemView>();
                previousTextNoteItem?.FocusEnd(FocusState.Programmatic);
            }
        }
    }
}
