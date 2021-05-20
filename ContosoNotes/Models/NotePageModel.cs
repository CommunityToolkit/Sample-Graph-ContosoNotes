// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ContosoNotes.Models
{
    public class NotePageModel : ObservableObject
    {
        public string Id { get; set; }

        private string _pageTitle;
        public string PageTitle 
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private ObservableCollection<NoteItemModel> _noteItems;
        public ObservableCollection<NoteItemModel> NoteItems 
        {
            get => _noteItems;
            private set => SetProperty(ref _noteItems, value);
        }

        [JsonIgnore]
        public bool IsEmpty => string.IsNullOrEmpty(_pageTitle) && (_noteItems.Count == 0 || (_noteItems.Count == 1 && string.IsNullOrWhiteSpace(_noteItems[0].Text)));

        public NotePageModel()
        {
            Id = Guid.NewGuid().ToString();
            _pageTitle = string.Empty;
            _noteItems = new ObservableCollection<NoteItemModel>();
        }

        public NotePageModel(string id = null, string pageTitle = null, IEnumerable<NoteItemModel> noteItems = null)
        {
            Id = id ?? Guid.NewGuid().ToString();
            _pageTitle = pageTitle;
            _noteItems = noteItems != null ? new ObservableCollection<NoteItemModel>(noteItems) : new ObservableCollection<NoteItemModel>();
        }
    }
}
