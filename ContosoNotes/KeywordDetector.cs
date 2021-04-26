using ContosoNotes.Models;
using System;
using System.Collections.Generic;
using Windows.System;

namespace ContosoNotes
{
    /// <summary>
    /// KeywordDetector is a singleton used to evaluate key presses and text changes and emit events.
    /// </summary>
    public class KeywordDetector
    {
        public static readonly KeywordDetector Instance = new KeywordDetector();

        public EventHandler<KeywordDetectedEventArgs> KeywordDetected { get; set; }
        public EventHandler<KeyDetectedEventArgs> KeyDetected { get; set; }

        public IList<string> Keywords { get; }
        public IList<VirtualKey> Keys { get; }

        public KeywordDetector()
        {
            Keywords = new List<string>();
            Keys = new List<VirtualKey>();
        }

        public void RegisterKeyword(string keyword)
        {
            if (!Keywords.Contains(keyword))
            {
                Keywords.Add(keyword);
            }
        }

        public void RegisterKey(VirtualKey key)
        {
            if (!Keys.Contains(key))
            {
                Keys.Add(key);
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

        public void Analyse(NoteItemModel noteItem, VirtualKey key, int index)
        {
            if (Keys.Contains(key))
            {
                string text = noteItem.Text;
                string preText = text.Substring(0, index);
                string postText = string.Empty;
                if (index < text.Length)
                {
                    postText = text.Substring(index + 1);
                }

                KeyDetected?.Invoke(noteItem, new KeyDetectedEventArgs(key, preText, postText));
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

    public struct KeyDetectedEventArgs
    {
        public VirtualKey Key { get; }
        public string PreText { get; set; }
        public string PostText { get; set; }

        public KeyDetectedEventArgs(VirtualKey key, string preText, string postText)
        {
            Key = key;
            PreText = preText;
            PostText = postText;
        }
    }
}
