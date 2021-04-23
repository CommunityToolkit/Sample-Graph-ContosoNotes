using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings;
using ContosoNotes.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;

namespace ContosoNotes.Views
{
    public class MainViewModel : ObservableObject
    {
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

        public MainViewModel()
        {
            _notesList = null;
            _currentNotePage = null;

            KeywordDetector.Instance.RegisterKeyword("TODO:");
            KeywordDetector.Instance.KeywordDetected += OnKeywordDetected;

            //KeywordDetector.Instance.RegisterKey();
            //KeywordDetector.Instance.KeyDetected += OnKeyDetected;

            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        private void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            Load();
        }

        public async void Load()
        {
            IObjectStorageHelper localStorageHelper = new LocalObjectStorageHelper(new SystemSerializer());
            IRoamingSettingsDataStore roamingStorageHelper;

            switch (ProviderManager.Instance.GlobalProvider?.State)
            {
                case ProviderState.SignedIn:
                    roamingStorageHelper = await RoamingSettingsHelper.CreateForCurrentUser(RoamingDataStore.OneDrive);
                    break;

                case ProviderState.SignedOut:
                    roamingStorageHelper = null;
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
                await localStorageHelper.SaveFileAsync(CurrentNotePage.Id, CurrentNotePage);
                await roamingStorageHelper?.SaveFileAsync(CurrentNotePage.Id, CurrentNotePage);
            }

            // Clear the notes list so we can repopulate it.
            NotesList = null;

            // Put the existing noteItems in a dictionary so we can more easily inspect the list of keys/ids.
            var noteListItemsDict = new Dictionary<string, NotesListItemModel>();

            // Get any remote notes.
            var remoteNotes = GetNotesList(roamingStorageHelper);
            if (remoteNotes != null)
            {
                foreach (var notesListItem in remoteNotes.Items)
                {
                    NotesList.Items.Add(notesListItem);
                    noteListItemsDict.Add(notesListItem.NotePageId, notesListItem);
                }
            }

            // Get any local notes.
            var localNotes = GetNotesList(localStorageHelper);
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

            // Check the roaming settings for an active note page.
            if (CurrentNotePage == null && roamingStorageHelper != null)
            {
                string currentNotePageId = roamingStorageHelper.Read<string>("currentNotePageId");
                if (currentNotePageId != null)
                {
                    foreach (var notePage in NotesList.Items)
                    {
                        if (currentNotePageId == notePage.NotePageId)
                        {
                            CurrentNotePage = await roamingStorageHelper.ReadFileAsync<NotePageModel>(notePage.NotePageFileName);
                            break;
                        }
                    }
                }
            }

            // Check the local settings for an active note page.
            if (CurrentNotePage == null)
            {
                string currentNotePageId = localStorageHelper.Read<string>("currentNotePageId");
                if (currentNotePageId != null)
                {
                    foreach (var notePage in NotesList.Items)
                    {
                        if (currentNotePageId == notePage.NotePageId)
                        {
                            CurrentNotePage = await localStorageHelper.ReadFileAsync<NotePageModel>(notePage.NotePageFileName);
                            break;
                        }
                    }
                }
            }

            if (CurrentNotePage != null)
            {
                localStorageHelper.Save("currentNotePageId", CurrentNotePage.Id);
                if (roamingStorageHelper != null)
                {
                    roamingStorageHelper.Save("currentNotePageId", CurrentNotePage.Id);
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
