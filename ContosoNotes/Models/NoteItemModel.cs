namespace ContosoNotes.Models
{
    public class NoteItemModel
    {
        public string Text { get; set; }

        public NoteItemModel(string text = "")
        {
            Text = text;
        }
    }
}
