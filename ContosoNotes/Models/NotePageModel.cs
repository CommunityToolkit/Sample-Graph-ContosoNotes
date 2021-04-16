using System;
using System.Collections.Generic;

namespace ContosoNotes.Models
{

    public class NotePageModel
    {
        public string Id { get; set; }

        public string PageTitle { get; set; }

        public IList<NoteItemModel> NoteItems { get; set; }

        public NotePageModel()
        {
            Id = Guid.NewGuid().ToString();
            NoteItems = new List<NoteItemModel>();
        }
    }
}
