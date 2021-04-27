using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings;
using ContosoNotes.Common;
using ContosoNotes.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Input;

namespace ContosoNotes.Views
{
    public class MainViewModel : ObservableObject
    {
        public RelayCommand TogglePaneCommand { get; }
        public RelayCommand SaveCommand { get; }

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
        private readonly IObjectStorageHelper _localStorageHelper;
        private readonly IObjectSerializer _serializer;

        public MainViewModel()
        {
            TogglePaneCommand = new RelayCommand(TogglePane);
            SaveCommand = new RelayCommand(Save);

            _isSignedIn = ProviderManager.Instance.GlobalProvider?.State == ProviderState.SignedIn;
            _isPaneOpen = true;
            _notesList = null;
            _currentNotePage = null;
            _serializer = new JsonObjectSerializer();
            _localStorageHelper = new LocalObjectStorageHelper(_serializer);

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
                    _roamingStorageHelper = await RoamingSettingsHelper.CreateForCurrentUser(RoamingDataStore.OneDrive, false, true, _serializer);
                    await _roamingStorageHelper.Sync();
                    break;

                case ProviderState.SignedOut:
                    _roamingStorageHelper = null;
                    break;

                case ProviderState.Loading:
                default:
                    return;
            }

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
                    await SaveCurrentNotePageAsync();
                }
            }

            // Clear the notes list so we can repopulate it.
            NotesList = new NotesListModel();

            var notesListItemsDict = new Dictionary<string, int>();

            // Get any remote notes.
            var remoteNotes = await GetNotesListAsync(_roamingStorageHelper);
            if (remoteNotes != null)
            {
                for (var i = 0; i < remoteNotes.Items.Count; i++)
                {
                    var notesListItem = remoteNotes.Items[i];
                    NotesList.Items.Add(notesListItem);
                    notesListItemsDict.Add(notesListItem.NotePageId, i);
                }
            }

            // Get any local notes.
            var localNotes = await GetNotesListAsync(_localStorageHelper);
            if (localNotes != null)
            {
                bool updateNotesList = false;
                foreach (var notesListItem in localNotes.Items)
                {
                    if (!notesListItemsDict.ContainsKey(notesListItem.NotePageId))
                    {
                        NotesList.Items.Add(notesListItem);

                        // Sync these notes back to the remote, if available.
                        updateNotesList = true;
                    }
                }

                if (updateNotesList)
                {
                    await SaveNotesListAsync();
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
                        foreach (var notesListItem in NotesList.Items)
                        {
                            if (currentNotePageId == notesListItem.NotePageId)
                            {
                                string notePageFileName = GetNotePageFileName(notesListItem);
                                CurrentNotePage = await _roamingStorageHelper.ReadFileAsync<NotePageModel>(notePageFileName);
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
                        foreach (var notesListItem in NotesList.Items)
                        {
                            if (currentNotePageId == notesListItem.NotePageId)
                            {
                                string notePageFileName = GetNotePageFileName(notesListItem);
                                CurrentNotePage = await _localStorageHelper.ReadFileAsync<NotePageModel>(notePageFileName);
                                break;
                            }
                        }
                    }
                }

                // Try to grab the first note page.
                if (CurrentNotePage == null && _notesList.Items.Count > 0)
                {
                    string notePageFileName = GetNotePageFileName(_notesList.Items[0]);
                    CurrentNotePage = await _localStorageHelper.ReadFileAsync<NotePageModel>(notePageFileName);
                }
            }

            if (CurrentNotePage != null)
            {
                // Just making sure we are all synced up.
                await SaveCurrentNotePageAsync();
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
        }

        public async void Save()
        {
            // Save any existing NotePage
            await SaveCurrentNotePageAsync();

            // Update the NotesList
            await SaveNotesListAsync();
        }

        private async Task SaveCurrentNotePageAsync()
        {
            if (_currentNotePage != null && !_currentNotePage.IsEmpty)
            {
                string notePageFileName = GetNotePageFileName(_currentNotePage);

                // We have transitioned between local and roaming data, but there is already an active NotePage with content.
                // Save the progress (overriding any existing) and continue to use it as the current NotePage.
                await _localStorageHelper.SaveFileAsync(notePageFileName, _currentNotePage);
                _localStorageHelper.Save("currentNotePageId", CurrentNotePage.Id);

                if (_roamingStorageHelper != null)
                {
                    await _roamingStorageHelper.SaveFileAsync(notePageFileName, _currentNotePage);
                    _roamingStorageHelper.Save("currentNotePageId", CurrentNotePage.Id);
                }
            }
        }

        private async Task SaveNotesListAsync()
        {
            if (_notesList != null && _notesList.Items.Count > 0)
            {
                if (_currentNotePage != null)
                {
                    // Update the title for the current note page for display in the notes list.
                    foreach (var notesListItem in NotesList.Items)
                    {
                        if (notesListItem.NotePageId == _currentNotePage.Id)
                        {
                            notesListItem.NotePageTitle = _currentNotePage.PageTitle;
                            break;
                        }
                    }
                }

                const string notesListFileName = "notesList.json";
                NotesListModel notesListModel = new NotesListModel(_notesList.Items);

                await _localStorageHelper.SaveFileAsync(notesListFileName, notesListModel);

                if (_roamingStorageHelper != null)
                {
                    await _roamingStorageHelper.SaveFileAsync(notesListFileName, notesListModel);
                }
            }
        }

        private async Task<NotesListModel> GetNotesListAsync(IObjectStorageHelper storageHelper)
        {
            if (storageHelper == null)
            {
                return null;
            }

            // Get the list of stored notes.
            string notesListFileName = "notesList.json";
            
            try
            {
                NotesListModel notesList = await storageHelper.ReadFileAsync<NotesListModel>(notesListFileName);
                return notesList;
            }
            catch
            {
                return null;
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
                    if (++noteItemIndex < CurrentNotePage.NoteItems.Count)
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

        private void OnKeyDetected(object sender, KeyDetectedEventArgs e)
        {
            
        }

        private string GetNotePageFileName(NotePageModel notePage)
        {
            return notePage.Id + ".json";
        }

        private string GetNotePageFileName(NotesListItemModel notesListItem)
        {
            return notesListItem.NotePageId + ".json";
        }
    }
}
