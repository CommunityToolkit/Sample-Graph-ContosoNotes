// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ContosoNotes.Models
{
    public class NotesListModel : ObservableObject
    {
        private ObservableCollection<NotesListItemModel> _items;
        public ObservableCollection<NotesListItemModel> Items 
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public NotesListModel()
        {
            _items = new ObservableCollection<NotesListItemModel>();
        }

        public NotesListModel(IEnumerable<NotesListItemModel> items)
        {
            _items = new ObservableCollection<NotesListItemModel>(items);
        }
    }
}
