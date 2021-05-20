// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

        public NoteItemModel()
        {
        }

        public NoteItemModel(string text = null)
        {
            _text = text;
        }
    }
}
