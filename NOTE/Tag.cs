using System;
using System.Collections.Generic;

namespace NOTE
{
	[Serializable]
	public class Tag
	{
		public string Name {get;set;}
		public int Count {get;set;}
		public HashSet<Note> Notes {get;set;}
		/// <summary>
		/// For linking back to ListStore
		/// </summary>
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

		public Tag (string name, int count, Note note)
		{
			Name = name;
			Count = count;
			Notes = new HashSet<Note> ();
			Notes.Add(note);
		}
	}
}

