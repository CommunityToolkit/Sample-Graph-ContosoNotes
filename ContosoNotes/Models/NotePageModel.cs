using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

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

        private IList<NoteItemModel> _noteItems;
        public IList<NoteItemModel> NoteItems 
        {
            get => _noteItems;
            set => SetProperty(ref _noteItems, value);
        }

        public bool IsEmpty => _noteItems == null || _noteItems.Count == 0 || (_noteItems.Count == 1 && string.IsNullOrWhiteSpace(_noteItems[0].Text));

        public NotePageModel()
        {
            Id = Guid.NewGuid().ToString();
            NoteItems = new List<NoteItemModel>();
        }

        public NotePageModel(string id = null, string pageTitle = null, IEnumerable<NoteItemModel> noteItems = null)
        {
            Id = id ?? Guid.NewGuid().ToString();
            PageTitle = pageTitle;
            NoteItems = noteItems != null ? new List<NoteItemModel>(noteItems) : new List<NoteItemModel>();
        }
    }
}
