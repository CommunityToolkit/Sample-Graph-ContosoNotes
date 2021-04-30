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

        private StorageManager _storageManager;

        public MainViewModel()
        {
            TogglePaneCommand = new RelayCommand(TogglePane);
            SaveCommand = new RelayCommand(Save);

            _isSignedIn = ProviderManager.Instance.GlobalProvider?.State == ProviderState.SignedIn;
            _isPaneOpen = true;
            _notesList = null;
            _currentNotePage = null;
            _storageManager = new StorageManager();

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
            if (ProviderManager.Instance.GlobalProvider?.State == ProviderState.Loading)
            {
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
        }

        private async void Save()
        {
            foreach (var item in _notesList.Items)
            {
                if (item.NotePageId == _currentNotePage.Id)
                {
                    item.NotePageTitle = _currentNotePage.PageTitle;
                    break;
                }
            }

            // Save any existing NotePage
            await _storageManager.SaveCurrentNotePageAsync(_currentNotePage);

            // Update the NotesList
            await _storageManager.SaveNotesListAsync(_notesList);
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

        private void OnKeyDetected(object sender, KeyDetectedEventArgs e)
        {
            
        }
    }
}
