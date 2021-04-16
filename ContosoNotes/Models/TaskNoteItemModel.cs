namespace ContosoNotes.Models
{
    public class TaskNoteItemModel : NoteItemModel
    {
        public bool IsCompleted { get; set; }

        public bool TodoTaskId { get; set; }

        public TaskNoteItemModel(string text = "") : base(text)
        {

        }
    }
}
