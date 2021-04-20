using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ContosoNotes.Models
{
    public class NoteItemModel : ObservableObject
    {
        private string _text;
        public string Text 
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public NoteItemModel(string text = "")
        {
            _text = text;
        }
    }
}
