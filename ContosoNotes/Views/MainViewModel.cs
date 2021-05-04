using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Net.Graph.Extensions;
using ContosoNotes.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using Windows.System;

namespace ContosoNotes.Views
{
    public class MainViewModel : ObservableObject
    {
        private static SemaphoreSlim _mutex = new SemaphoreSlim(1);

        public RelayCommand LaunchMicrosoftTodoCommand { get; }
        public RelayCommand TogglePaneCommand { get; }
        public RelayCommand SaveCommand { get; }

        public RelayCommand<TaskNoteItemModel> DeleteTaskCommand { get; }

        private bool _isPaneOpen;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => SetProperty(ref _isPaneOpen, value);
        }

        private bool _isSignedIn;
        public bool IsSignedIn
        {
            get => _isSignedIn;
            set => SetProperty(ref _isSignedIn, value);
        }

        private NotesListModel _notesList;
        public NotesListModel NotesList
        {
            get => _notesList;
            set => SetProperty(ref _notesList, value);
        }

        private NotePageModel _currentNotePage;
        public NotePageModel CurrentNotePage
        {
            get => _currentNotePage;
            set => SetProperty(ref _currentNotePage, value);
        }

        private DateTime? _lastSync;
        public DateTime? LastSync
        {
            get => _lastSync;
            set => SetProperty(ref _lastSync, value);
        }

        private StorageManager _storageManager;
        private DispatcherQueue _dispatcherQueue;
        private DispatcherQueueTimer _timer;

        public MainViewModel()
        {
            LaunchMicrosoftTodoCommand = new(LaunchMicrosoftTodo);
            TogglePaneCommand = new(TogglePane);
            SaveCommand = new(Save);
            DeleteTaskCommand = new(DeleteTask);

            _currentNotePage = null;
            _isSignedIn = ProviderManager.Instance.GlobalProvider?.State == ProviderState.SignedIn;
            _isPaneOpen = false;
            _lastSync = null;
            _notesList = null;
            _storageManager = new StorageManager();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _timer = _dispatcherQueue.CreateTimer();

            KeywordDetector.Instance.RegisterKeyword("todo:");
            KeywordDetector.Instance.KeywordDetected += OnKeywordDetected;

            //KeywordDetector.Instance.RegisterKey();
            //KeywordDetector.Instance.KeyDetected += OnKeyDetected;

            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        private async void LaunchMicrosoftTodo()
        {
            string taskListId = string.Empty;

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                var graph = ProviderManager.Instance.GlobalProvider.Graph();

                try
                {
                    var existingLists = await graph.Me.Todo.Lists.Request().Filter("displayName eq 'ContosoNotes'").GetAsync();
                    if (existingLists.Count > 0)
                    {
                        taskListId = existingLists[0].Id;
                    }
                }
                catch
                {
                }
            }

            var uri = new Uri("https://to-do.live.com/tasks/" + taskListId);
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void TogglePane()
        {
            IsPaneOpen = !IsPaneOpen;
        }

        private void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            IsSignedIn = ProviderManager.Instance.GlobalProvider?.State == ProviderState.SignedIn;

            if (ProviderManager.Instance.GlobalProvider?.State != ProviderState.Loading)
            {
                Load();
            }
        }

        public async void Load()
        {
            await _mutex.WaitAsync();

            try
            {
                // Handle any existing NotePage
                if (CurrentNotePage != null)
                {
                    if (CurrentNotePage.IsEmpty)
                    {
                        CurrentNotePage = null;
                    }
                    else
                    {
                        // We have transitioned between local and roaming data, but there is already an active NotePage with content.
                        // Save the progress (overriding any existing) and continue to use it as the current NotePage.
                        await _storageManager.SaveCurrentNotePageAsync(_currentNotePage);
                    }
                }

                // Clear the notes list so we can repopulate it.
                NotesList = new NotesListModel();
                NotesList = await _storageManager.GetNotesListAsync();

                // If we have notes in the list, attempt to pull the active/current note page.
                if (_notesList.Items.Count > 0)
                {
                    if (_currentNotePage == null)
                    {
                        CurrentNotePage = await _storageManager.GetCurrentNotePageAsync(_notesList);
                    }

                    if (_currentNotePage == null)
                    {
                        // We didn't find a "current" page, so just grab the first one in the list.
                        CurrentNotePage = await _storageManager.GetNotePageAsync(_notesList.Items[0]);
                    }
                }

                if (_currentNotePage != null)
                {
                    // Just making sure we are all synced up.
                    await _storageManager.SaveCurrentNotePageAsync(_currentNotePage);
                }
                else
                {
                    // Create a new empty NotePageModel, with a fresh item ready for input
                    CurrentNotePage = new NotePageModel()
                    {
                        PageTitle = "New note",
                        NoteItems = new ObservableCollection<NoteItemModel>()
                    {
                        new NoteItemModel()
                    }
                    };

                    NotesList.Items.Add(new NotesListItemModel()
                    {
                        NotePageId = CurrentNotePage.Id,
                        NotePageTitle = CurrentNotePage.PageTitle,
                    });
                }

                InitSaveTimer();
            }
            finally
            {
                _mutex.Release();
            }
        }

        private void InitSaveTimer()
        {
            if (!_timer.IsRunning)
            {
                _timer.Tick += OnTimerTick;
                _timer.Interval = TimeSpan.FromSeconds(10);
                _timer.IsRepeating = true;

                _timer.Start();
            }
        }

        private void OnTimerTick(DispatcherQueueTimer timer, object e)
        {
            // TODO: Detect changes before saving. 

            Save();
        }

        private async void Save()
        {
            // Find and update the note page title in the notes list.
            foreach (var item in _notesList.Items)
            {
                if (item.NotePageId == _currentNotePage.Id)
                {
                    item.NotePageTitle = _currentNotePage.PageTitle;
                    break;
                }
            }

            try
            {
                // Save any existing NotePage
                await _storageManager.SaveCurrentNotePageAsync(_currentNotePage);

                // Update the NotesList
                await _storageManager.SaveNotesListAsync(_notesList);

                LastSync = DateTime.Now;
            }
            catch
            {
            }
        }

        private void OnKeywordDetected(object sender, KeywordDetectedEventArgs e)
        {
            var noteItem = sender as NoteItemModel;

            // Handle any registered keywords.
            if (e.Keyword == "todo:")
            {
                var noteItemIndex = CurrentNotePage.NoteItems.IndexOf(noteItem);

                if (string.IsNullOrEmpty(e.PreText))
                {
                    // Remove the now empty note item.
                    CurrentNotePage.NoteItems.RemoveAt(noteItemIndex--);
                }
                else
                {
                    // Update the existing NoteItem with the text prior to the detected keyword.
                    CurrentNotePage.NoteItems[noteItemIndex].Text = e.PreText;
                }

                var taskItem = new TaskNoteItemModel();
                CurrentNotePage.NoteItems.Insert(++noteItemIndex, taskItem);

                // Insert a new item with the text from after the detected keyword, if any.
                if (!string.IsNullOrEmpty(e.PostText))
                {
                    // Check if we have a text note next to pre-pend with our split
                    if (++noteItemIndex < CurrentNotePage.NoteItems.Count && CurrentNotePage.NoteItems[noteItemIndex] is not TaskNoteItemModel)
                    {
                        var note = CurrentNotePage.NoteItems[noteItemIndex];
                        note.Text = e.PostText + note.Text;
                    }
                    else
                    {
                        // Otherwise we insert a new text note
                        NoteItemModel postItem = new NoteItemModel(e.PostText);
                        CurrentNotePage.NoteItems.Insert(noteItemIndex, postItem);
                    }
                }
                else if (++noteItemIndex == CurrentNotePage.NoteItems.Count)
                {
                    // If we're at the end we also want a blank note to help for navigating
                    NoteItemModel postItem = new NoteItemModel(" ");
                    CurrentNotePage.NoteItems.Insert(noteItemIndex, postItem);
                }
            }
        }

        private void DeleteTask(TaskNoteItemModel task)
        {
            var taskIndex = CurrentNotePage.NoteItems.IndexOf(task);

            CurrentNotePage.NoteItems.RemoveAt(taskIndex);

            // Check if we see a text note before us and after to merge
            if (taskIndex > 0 && CurrentNotePage.NoteItems[taskIndex - 1] is not TaskNoteItemModel &&
                taskIndex < CurrentNotePage.NoteItems.Count && CurrentNotePage.NoteItems[taskIndex] is not TaskNoteItemModel)
            {
                // Merge two texts together
                CurrentNotePage.NoteItems[taskIndex - 1].Text += CurrentNotePage.NoteItems[taskIndex].Text;

                CurrentNotePage.NoteItems.RemoveAt(taskIndex);
            }

            // Delete the task in the Graph as well.
            task.Delete();
        }
    }
}
