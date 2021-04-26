using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ContosoNotes.Models
{
    public class NotesListItemModel : ObservableObject
    {
        public string NotePageId { get; set; }

        private string _notePageTitle;
        public string NotePageTitle 
        {
            get => _notePageTitle;
            set => SetProperty(ref _notePageTitle, value);
        }
    }
}
