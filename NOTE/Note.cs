using System;
using System.Collections.Generic;

namespace NOTE
{
	[Serializable]
	public class Note : INote
	{
		public string Title {get;set;}
		public string Content {get;set;}
		public ISet<string> Tags {get;set;}

		[NonSerialized]
		private Gtk.TreeIter treeIter;
		public Gtk.TreeIter TreeIter {
			get {
				return treeIter;
			}
			set {
				treeIter = value;
			}
		}

		public Note (INote note = null)
		{
			if (note != null) {
				Title = note.Title; //is this a copy or a reference?
				Content = note.Content;
				Tags = note.Tags;
			}
		}

		public bool Contains (String term) {
			if (Title.Contains(term) || Content.Contains(term))
				return true;
			return false;
		}
	}
}

