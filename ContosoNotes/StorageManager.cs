// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Helpers.RoamingSettings;
using ContosoNotes.Common;
using ContosoNotes.Models;
using Microsoft.Toolkit.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoNotes
{
    public class StorageManager
    {
        private readonly Microsoft.Toolkit.Helpers.IObjectSerializer _serializer;
        private readonly ApplicationDataStorageHelper _localStorage;
        private IFileStorageHelper _remoteFileStorage;
        private ISettingsStorageHelper<string> _remoteSettingsStorage;

        public StorageManager()
        {
            _serializer = new JsonObjectSerializer();
            _localStorage = Microsoft.Toolkit.Uwp.Helpers.ApplicationDataStorageHelper.GetCurrent(_serializer);

            ProviderManager.Instance.ProviderStateChanged += (s, e) => _ = InitRoamingSettingsHelperAsync();
            _ = InitRoamingSettingsHelperAsync();
        }

        public async Task SaveNotesListAsync(NotesListModel notesList)
        {
            if (notesList != null && notesList.Items.Count > 0)
            {
                const string notesListFileName = "notesList.json";

                await _localStorage.CreateFileAsync(notesListFileName, notesList);

                if (_remoteFileStorage != null)
                {
                    await _remoteFileStorage.CreateFileAsync(notesListFileName, notesList);
                }
            }
        }

        public async Task SaveFileAsync<T>(string fileName, T value)
        {
            await _localStorage.CreateFileAsync(fileName, value);

            if (_remoteFileStorage != null)
            {
                await _remoteFileStorage.CreateFileAsync(fileName, value);
            }
        }

        public async Task<NotePageModel> GetCurrentNotePageAsync(NotesListModel notesList)
        {
            // Check the settings for an active note page.
            return _remoteFileStorage != null && _remoteSettingsStorage != null
                ? await GetCurrentNotePage(_remoteSettingsStorage, _remoteFileStorage, notesList)
                : await GetCurrentNotePage(_localStorage, _localStorage, notesList);
        }

        public async Task<NotesListModel> GetNotesListAsync()
        {
            var notesListItems = new List<NotesListItemModel>();
            var notesListItemsDict = new Dictionary<string, int>();

            await InitRoamingSettingsHelperAsync();

            bool updateNotesList = false;

            // Get any remote notes.
            var remoteNotes = await GetNotesListAsync(_remoteFileStorage);
            if (remoteNotes != null)
            {
                for (var i = 0; i < remoteNotes.Items.Count; i++)
                {
                    var notesListItem = remoteNotes.Items[i];
                    notesListItems.Add(notesListItem);
                    notesListItemsDict.Add(notesListItem.NotePageId, i);
                }
            }

            // Get any local notes.
            var localNotes = await GetNotesListAsync(_localStorage);
            if (localNotes != null)
            {
                foreach (var notesListItem in localNotes.Items)
                {
                    if (!notesListItemsDict.ContainsKey(notesListItem.NotePageId))
                    {
                        notesListItems.Add(notesListItem);

                        // Sync these notes back to the remote, if available.
                        updateNotesList = true;
                    }
                }
            }

            NotesListModel notesList = null;

            if (notesListItems.Count > 0)
            {
                notesList = new NotesListModel(notesListItems);

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

                await _localStorage.CreateFileAsync(notePageFileName, currentNotePage);
                _localStorage.Save("currentNotePageId", currentNotePage.Id);

                if (_remoteFileStorage != null)
                {
                    await _remoteFileStorage.CreateFileAsync(notePageFileName, currentNotePage);
                    _remoteSettingsStorage.Save("currentNotePageId", currentNotePage.Id);
                }
            }
        }

        public async Task<NotePageModel> GetNotePageAsync(NotesListItemModel notesListItemModel)
        {
            var notePageFileName = GetNotePageFileName(notesListItemModel);

            if (_remoteFileStorage != null)
            {
                try
                {
                    return await _remoteFileStorage.ReadFileAsync<NotePageModel>(notePageFileName);
                }
                catch
                {
                }
            }

            return await _localStorage.ReadFileAsync<NotePageModel>(notePageFileName);
        }

        private async Task<NotePageModel> GetCurrentNotePage(ISettingsStorageHelper<string> settingsStorage, IFileStorageHelper fileStorage, NotesListModel notesList)
        {
            if (fileStorage == null || settingsStorage == null)
            {
                return null;
            }

            if (settingsStorage.TryRead<string>("currentNotePageId", out string currentNotePageId) && currentNotePageId != null)
            {
                foreach (var notesListItem in notesList.Items)
                {
                    if (currentNotePageId == notesListItem.NotePageId)
                    {
                        string notePageFileName = GetNotePageFileName(notesListItem);
                        return await fileStorage.ReadFileAsync<NotePageModel>(notePageFileName);
                    }
                }
            }

            return null;
        }

        private async Task<NotesListModel> GetNotesListAsync(IFileStorageHelper storageHelper)
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
            catch (System.Exception e)
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

        private async Task InitRoamingSettingsHelperAsync()
        {
            switch (ProviderManager.Instance?.GlobalProvider?.State)
            {
                case ProviderState.SignedIn:
                    if (_remoteFileStorage == null)
                    {
                        _remoteFileStorage = await OneDriveStorageHelper.CreateForCurrentUserAsync(_serializer);
                    }
                    if (_remoteSettingsStorage == null)
                    {
                        _remoteSettingsStorage = await UserExtensionStorageHelper.CreateForCurrentUserAsync("ContosoNotes.json", _serializer);
                    }
                    break;

                case ProviderState.SignedOut:
                    _remoteFileStorage = null;
                    _remoteSettingsStorage = null;
                    break;
            }
        }
    }
}
