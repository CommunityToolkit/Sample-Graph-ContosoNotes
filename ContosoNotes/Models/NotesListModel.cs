using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ContosoNotes.Models
{
    public class NotesListModel : ObservableObject
    {
        private IList<NotesListItemModel> _items;
        public IList<NotesListItemModel> Items 
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public NotesListModel()
        {
            _items = new List<NotesListItemModel>();
        }

        public NotesListModel(IEnumerable<NotesListItemModel> items)
        {
            _items = new List<NotesListItemModel>(items);
        }
    }
}
