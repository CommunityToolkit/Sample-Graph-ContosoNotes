using Microsoft.Toolkit.Mvvm.ComponentModel;
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
            set => SetProperty(ref _noteItems, value);
        }

        public bool IsEmpty => _noteItems == null || _noteItems.Count == 0 || (_noteItems.Count == 1 && string.IsNullOrWhiteSpace(_noteItems[0].Text));

        public NotePageModel()
        {
            Id = Guid.NewGuid().ToString();
            NoteItems = new ObservableCollection<NoteItemModel>();
        }

        public NotePageModel(string id = null, string pageTitle = null, IEnumerable<NoteItemModel> noteItems = null)
        {
            Id = id ?? Guid.NewGuid().ToString();
            PageTitle = pageTitle;
            NoteItems = noteItems != null ? new ObservableCollection<NoteItemModel>(noteItems) : new ObservableCollection<NoteItemModel>();
        }
    }
}
