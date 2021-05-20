// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ContosoNotes.Models;
using System;
using System.Collections.Generic;

namespace ContosoNotes
{
    /// <summary>
    /// KeywordDetector is a singleton used to evaluate key presses and text changes and emit events.
    /// </summary>
    public class KeywordDetector
    {
        public static readonly KeywordDetector Instance = new KeywordDetector();

        public EventHandler<KeywordDetectedEventArgs> KeywordDetected { get; set; }

        public IList<string> Keywords { get; }

        public KeywordDetector()
        {
            Keywords = new List<string>();
        }

        public void RegisterKeyword(string keyword)
        {
            if (!Keywords.Contains(keyword))
            {
                Keywords.Add(keyword);
            }
        }

        public void Analyse(NoteItemModel noteItem)
        {
            string text = noteItem.Text;

            foreach (string keyword in Keywords)
            {
                if (text.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))
                {
                    string preText = text.Substring(0, text.IndexOf(keyword));
                    string postText = text.Substring(text.IndexOf(keyword) + keyword.Length);

                    KeywordDetected?.Invoke(noteItem, new KeywordDetectedEventArgs(keyword, preText, postText));
                }
            }
        }
    }

    public struct KeywordDetectedEventArgs
    {
        public string Keyword { get; }
        public string PreText { get; set; }
        public string PostText { get; set; }

        public KeywordDetectedEventArgs(string keyword, string preText, string postText)
        {
            Keyword = keyword;
            PreText = preText;
            PostText = postText;
        }
    }
}
