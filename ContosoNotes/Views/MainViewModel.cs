using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings;
using ContosoNotes.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.System;

namespace ContosoNotes.Views
{
    public class MainViewModel : ObservableObject
    {
        public RelayCommand TogglePaneCommand { get; }
        
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

        private IRoamingSettingsDataStore _roamingStorageHelper;
        private IObjectStorageHelper _localStorageHelper;

        public MainViewModel()
        {
            TogglePaneCommand = new RelayCommand(TogglePane);

            _isSignedIn = ProviderManager.Instance.GlobalProvider?.State == ProviderState.SignedIn;
            _isPaneOpen = true;
            _notesList = null;
            _currentNotePage = null;
            _localStorageHelper = new LocalObjectStorageHelper(new SystemSerializer());

            KeywordDetector.Instance.RegisterKeyword("todo:");
            KeywordDetector.Instance.KeywordDetected += OnKeywordDetected;

            //KeywordDetector.Instance.RegisterKey();
            //KeywordDetector.Instance.KeyDetected += OnKeyDetected;

            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        private void TogglePane()
        {
            IsPaneOpen = !IsPaneOpen;
        }

        private void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            IsSignedIn = ProviderManager.Instance.GlobalProvider?.State == ProviderState.SignedIn;

            Load();
        }

        public async void Load()
        {
            switch (ProviderManager.Instance.GlobalProvider?.State)
            {
                case ProviderState.SignedIn:
                    _roamingStorageHelper = await RoamingSettingsHelper.CreateForCurrentUser(RoamingDataStore.OneDrive);
                    break;

                case ProviderState.SignedOut:
                    _roamingStorageHelper = null;
                    break;

                case ProviderState.Loading:
                default:
                    return;
            }

            // Handle any existing NotePage
            if (CurrentNotePage != null && !CurrentNotePage.IsEmpty)
            {
                // We have transitioned between local and roaming data, but there is already an active NotePage with content.
                // Save the progress (overriding any existing) and continue to use it as the current NotePage.
                await _localStorageHelper.SaveFileAsync(CurrentNotePage.Id, CurrentNotePage);
                await _roamingStorageHelper?.SaveFileAsync(CurrentNotePage.Id, CurrentNotePage);

            }

            // Clear the notes list so we can repopulate it.
            NotesList = new NotesListModel();

            // Put the existing noteItems in a dictionary so we can more easily inspect the list of keys/ids.
            var noteListItemsDict = new Dictionary<string, NotesListItemModel>();

            // Get any remote notes.
            var remoteNotes = GetNotesList(_roamingStorageHelper);
            if (remoteNotes != null)
            {
                foreach (var notesListItem in remoteNotes.Items)
                {
                    NotesList.Items.Add(notesListItem);
                    noteListItemsDict.Add(notesListItem.NotePageId, notesListItem);
                }
            }

            // Get any local notes.
            var localNotes = GetNotesList(_localStorageHelper);
            if (localNotes != null)
            {
                foreach (var notesListItem in localNotes.Items)
                {
                    if (!noteListItemsDict.ContainsKey(notesListItem.NotePageId))
                    {
                        NotesList.Items.Add(notesListItem);
                        // TODO: Sync these notes back to the remote, if available.
                    }
                }
            }

            // If we have notes in the list, attempt to pull the active/current note page.
            if (NotesList.Items.Count > 0)
            {
                // Check the roaming settings for an active note page.
                if (CurrentNotePage == null && _roamingStorageHelper != null)
                {
                    string currentNotePageId = _roamingStorageHelper.Read<string>("currentNotePageId");
                    if (currentNotePageId != null)
                    {
                        foreach (var notePage in NotesList.Items)
                        {
                            if (currentNotePageId == notePage.NotePageId)
                            {
                                CurrentNotePage = await _roamingStorageHelper.ReadFileAsync<NotePageModel>(notePage.NotePageFileName);
                                break;
                            }
                        }
                    }
                }

                // Check the local settings for an active note page.
                if (CurrentNotePage == null)
                {
                    string currentNotePageId = _localStorageHelper.Read<string>("currentNotePageId");
                    if (currentNotePageId != null)
                    {
                        foreach (var notePage in NotesList.Items)
                        {
                            if (currentNotePageId == notePage.NotePageId)
                            {
                                CurrentNotePage = await _localStorageHelper.ReadFileAsync<NotePageModel>(notePage.NotePageFileName);
                                break;
                            }
                        }
                    }
                }
            }

            if (CurrentNotePage != null)
            {
                _localStorageHelper.Save("currentNotePageId", CurrentNotePage.Id);
                if (_roamingStorageHelper != null)
                {
                    _roamingStorageHelper.Save("currentNotePageId", CurrentNotePage.Id);
                }
            }
            else
            {
                // Create a new empty NotePageModel, with a fresh item ready for input
                CurrentNotePage = new NotePageModel()
                {
                    NoteItems = new ObservableCollection<NoteItemModel>()
                    {
                        new NoteItemModel()
                    }
                };
            }
        }

        public async void Save()
        {
            // Handle any existing NotePage
            if (_currentNotePage != null && !_currentNotePage.IsEmpty)
            {
                // We have transitioned between local and roaming data, but there is already an active NotePage with content.
                // Save the progress (overriding any existing) and continue to use it as the current NotePage.
                await _localStorageHelper.SaveFileAsync(_currentNotePage.Id, _currentNotePage);

                if (_roamingStorageHelper != null)
                {
                    await _roamingStorageHelper.SaveFileAsync(_currentNotePage.Id, _currentNotePage);
                }
            }

            // Update the NotesList
            if (_notesList != null && _notesList.Items.Count > 0)
            {
                _localStorageHelper.Save("notesList", NotesList);

                if (_roamingStorageHelper != null)
                {
                    _roamingStorageHelper.Save("notesList", NotesList);
                }
            }
        }

        private NotesListModel GetNotesList(IObjectStorageHelper storageHelper)
        {
            if (storageHelper == null)
            {
                return null;
            }

            // Get the list of stored notes.
            string notesListKey = "notesList";
            var notesListExists = storageHelper.KeyExists(notesListKey);
            if (notesListExists)
            {
                try
                {
                    NotesListModel notesList = storageHelper.Read<NotesListModel>(notesListKey);
                    return notesList;
                }
                catch
                {
                    return null;
                }
            }

            return new NotesListModel();
        }

        private void OnKeywordDetected(object sender, KeywordDetectedEventArgs e)
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                var noteItem = sender as NoteItemModel;

                // Handle any registered keywords.
                if (e.Keyword == "TODO:")
                {
                    var noteItemIndex = CurrentNotePage.NoteItems.IndexOf(noteItem);

                    if (string.IsNullOrEmpty(e.PreText))
                    {
                        // Remove the now empty note item.
                        CurrentNotePage.NoteItems.RemoveAt(noteItemIndex);
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
                        NoteItemModel postItem = new NoteItemModel(e.PostText);
                        CurrentNotePage.NoteItems.Insert(++noteItemIndex, postItem);
                    }
                }
            });
        }

        private void OnKeyDetected(object sender, KeyDetectedEventArgs e)
        {
            
        }
    }
}
