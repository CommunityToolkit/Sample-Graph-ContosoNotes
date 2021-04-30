using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings;
using ContosoNotes.Common;
using ContosoNotes.Models;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoNotes
{
    public class StorageManager
    {
        private IObjectSerializer _serializer;
        private LocalObjectStorageHelper _localStorageHelper;
        private RoamingSettingsHelper _roamingStorageHelper;

        public StorageManager()
        {
            _serializer = new JsonObjectSerializer();
            _localStorageHelper = new LocalObjectStorageHelper(_serializer);

            ProviderManager.Instance.ProviderUpdated += (s, e) => InitRoamingSettingsHelper();
            InitRoamingSettingsHelper();
        }

        public async Task SaveNotesListAsync(NotesListModel notesList)
        {
            if (notesList != null && notesList.Items.Count > 0)
            {
                const string notesListFileName = "notesList.json";

                await _localStorageHelper.SaveFileAsync(notesListFileName, notesList);

                if (_roamingStorageHelper != null)
                {
                    await _roamingStorageHelper.SaveFileAsync(notesListFileName, notesList);
                }
            }
        }

        public async Task SaveFileAsync<T>(string fileName, T value)
        {
            await _localStorageHelper.SaveFileAsync(fileName, value);

            if (_roamingStorageHelper != null)
            {
                await _roamingStorageHelper.SaveFileAsync(fileName, value);
            }
        }

        public async Task<NotePageModel> GetCurrentNotePageAsync(NotesListModel notesList)
        {
            // Check the settings for an active note page.
            return _roamingStorageHelper != null
                ? await GetCurrentNotePage(_roamingStorageHelper, notesList)
                : await GetCurrentNotePage(_localStorageHelper, notesList);
        }

        public async Task<NotesListModel> GetNotesListAsync()
        {
            var notesList = new NotesListModel();
            var notesListItemsDict = new Dictionary<string, int>();

            // Get any remote notes.
            var remoteNotes = await GetNotesListAsync(_roamingStorageHelper);
            if (remoteNotes != null)
            {
                for (var i = 0; i < remoteNotes.Items.Count; i++)
                {
                    var notesListItem = remoteNotes.Items[i];
                    notesList.Items.Add(notesListItem);
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
                        notesList.Items.Add(notesListItem);

                        // Sync these notes back to the remote, if available.
                        updateNotesList = true;
                    }
                }

                if (updateNotesList)
                {
                    await SaveNotesListAsync(notesList);
                }
            }

            return notesList;
        }

        public async Task SaveCurrentNotePageAsync(NotePageModel currentNotePage)
        {
            if (currentNotePage != null && !currentNotePage.IsEmpty)
            {
                string notePageFileName = GetNotePageFileName(currentNotePage);

                await _localStorageHelper.SaveFileAsync(notePageFileName, currentNotePage);
                _localStorageHelper.Save("currentNotePageId", currentNotePage.Id);

                if (_roamingStorageHelper != null)
                {
                    await _roamingStorageHelper.SaveFileAsync(notePageFileName, currentNotePage);
                    _roamingStorageHelper.Save("currentNotePageId", currentNotePage.Id);
                }
            }
        }

        public async Task<NotePageModel> GetNotePageAsync(NotesListItemModel notesListItemModel)
        {
            var notePageFileName = GetNotePageFileName(notesListItemModel);

            return _roamingStorageHelper != null
                ? await _roamingStorageHelper.ReadFileAsync<NotePageModel>(notePageFileName)
                : await _localStorageHelper.ReadFileAsync<NotePageModel>(notePageFileName);
        }

        private async Task<NotePageModel> GetCurrentNotePage(IObjectStorageHelper storageHelper, NotesListModel notesList)
        {
            if (storageHelper == null)
            {
                return null;
            }

            string currentNotePageId = null;
            try
            {
                currentNotePageId = storageHelper.Read<string>("currentNotePageId");
            }
            catch
            {
                // Current note page id is empty.
            }

            if (currentNotePageId != null)
            {
                foreach (var notesListItem in notesList.Items)
                {
                    if (currentNotePageId == notesListItem.NotePageId)
                    {
                        string notePageFileName = GetNotePageFileName(notesListItem);
                        return await storageHelper.ReadFileAsync<NotePageModel>(notePageFileName);
                    }
                }
            }

            return null;
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

        private string GetNotePageFileName(NotePageModel notePage)
        {
            return notePage.Id + ".json";
        }

        private string GetNotePageFileName(NotesListItemModel notesListItem)
        {
            return notesListItem.NotePageId + ".json";
        }

        private async void InitRoamingSettingsHelper()
        {

            switch (ProviderManager.Instance?.GlobalProvider?.State)
            {
                case ProviderState.SignedIn:
                    var storageHelper = await RoamingSettingsHelper.CreateForCurrentUser(RoamingDataStore.OneDrive, false, true, _serializer);
                    await storageHelper.Sync();
                    _roamingStorageHelper = storageHelper;
                    break;

                case ProviderState.SignedOut:
                    _roamingStorageHelper = null;
                    break;
            }
        }
    }
}
