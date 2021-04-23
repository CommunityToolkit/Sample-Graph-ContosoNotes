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

        public NotesListModel(IEnumerable<NotesListItemModel> items = null)
        {
            _items = items != null
                ? new ObservableCollection<NotesListItemModel>(items)
                : new ObservableCollection<NotesListItemModel>();
        }
    }
}
