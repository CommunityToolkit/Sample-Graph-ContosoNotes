using ContosoNotes.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ContosoNotes.UI
{
    public sealed partial class TaskNoteItemView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TaskNoteItemProperty =
            DependencyProperty.Register(nameof(TaskNoteItem), typeof(TaskNoteItemModel), typeof(TaskNoteItemView), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskNoteItemModel TaskNoteItem
        {
            get => (TaskNoteItemModel)GetValue(TaskNoteItemProperty);
            set => SetValueDP(TaskNoteItemProperty, value);
        }

        public TaskNoteItemView()
        {
            InitializeComponent();
            (Content as FrameworkElement).DataContext = this;

            GettingFocus += TaskNoteItemView_GettingFocus;
        }

        public async void FocusEnd()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ItemText.SelectionStart = ItemText.Text.Length;
            });
        }

        private void TaskNoteItemView_GettingFocus(UIElement sender, GettingFocusEventArgs args)
        {
            try
            {
                if (args.OldFocusedElement != ItemText)
                {
                    args.NewFocusedElement = ItemText;
                }
            }
            catch
            {
                // There is some problem here causing an Arguement exception to be thrown when settings the newly focused element.
                // Need to investigate.
            }
        }

        private void SetValueDP(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left && ItemText.SelectionStart == 0)
            {
                ItemCheck.Focus(FocusState.Keyboard);

                e.Handled = true;
            }
        }

        private void CheckBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Right)
            {
                ItemText.Focus(FocusState.Keyboard);

                e.Handled = true;
            }
        }
    }
}
