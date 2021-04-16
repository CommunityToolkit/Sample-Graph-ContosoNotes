using ContosoNotes.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ContosoNotes.Views
{
    public class MainViewModel : ObservableObject
    {
        private ObservableCollection<NotePageModel> _notePages;
        public ObservableCollection<NotePageModel> NotePages
        {
            get => _notePages;
            set => SetProperty(ref _notePages, value);
        }

        private NotePageModel _currentNotePage;
        public NotePageModel CurrentNotePage
        {
            get => _currentNotePage;
            set => SetProperty(ref _currentNotePage, value);
        }

        public MainViewModel()
        {
            _notePages = new ObservableCollection<NotePageModel>();
            _currentNotePage = null;
        }

        public async void Load(object parameter = null)
        {
            IObjectSerializer serializer = new SystemSerializer();
            IObjectStorageHelper roamingSettings = new LocalObjectStorageHelper(serializer);

            string notesListKey = "notesList";
            var notesListExists = roamingSettings.KeyExists(notesListKey);
            if (notesListExists)
            {
                // TODO: Update RoamingSettings DataStores to Read from remote if autoSync is true.
                List<string> notesList = roamingSettings.Read<List<string>>(notesListKey);

                foreach (var noteFileName in notesList.ToArray())
                {
                    try
                    {
                        // Parse the file and add the deserialized NotePage.
                        NotePageModel notePage = await roamingSettings.ReadFileAsync<NotePageModel>(noteFileName);
                        NotePages.Add(notePage);
                    }
                    catch
                    {
                        // We could not read the file.
                        notesList.Remove(noteFileName);
                    }
                }

                // TODO: Update the notes list if any note files were removed.

                // Check the app settings for an active note page.
                string notePageId = roamingSettings.Read<string>("currentNotePageId");
                if (notePageId != null)
                {
                    foreach (var notePage in NotePages)
                    {
                        if (notePageId == notePage.Id)
                        {
                            CurrentNotePage = notePage;
                            break;
                        }
                    }
                }
            }

            if (CurrentNotePage == null)
            {
                CurrentNotePage = new NotePageModel()
                {
                    NoteItems = new List<NoteItemModel>() { new NoteItemModel() }
                };
            }
        }
    }
}
