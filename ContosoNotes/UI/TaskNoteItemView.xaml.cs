using ContosoNotes.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ContosoNotes.UI
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

        public ICommand DeleteTaskCommand
        {
            get { return (ICommand)GetValue(DeleteTaskCommandProperty); }
            set { SetValue(DeleteTaskCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteTaskCommandProperty =
            DependencyProperty.Register(nameof(DeleteTaskCommand), typeof(ICommand), typeof(TaskNoteItemView), new PropertyMetadata(null));

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

        private void TaskNoteItemView_GettingFocus(UIElement sender, GettingFocusEventArgs args)
        {
            if (args.OldFocusedElement != ItemText)
            {
                args.NewFocusedElement = ItemText;
            }
        }

        private void SetValueDP(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                FocusManager.TryMoveFocus(FocusNavigationDirection.Down);

                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Left &&
                ItemText.SelectionStart == 0)
            {
                ItemCheck.Focus(FocusState.Keyboard);

                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Back && string.IsNullOrEmpty(ItemText.Text))
            {
                DeleteTaskCommand.Execute(TaskNoteItem);
            }
        }

        private void CheckBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Right)
            {
                ItemText.Focus(FocusState.Keyboard);

                e.Handled = true;
            }
        }
    }
}
