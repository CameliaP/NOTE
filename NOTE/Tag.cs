using System;
using System.Collections.Generic;

namespace NOTE
{
	[Serializable]
	public class Tag
	{
		public string Name {get;set;}
		public int Count {get {return Notes.Count;}}
		public ISet<Note> Notes {get;set;}
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

		public Tag (string name, Note note)
		{
			Name = name;
			Notes = new HashSet<Note> ();
			Notes.Add(note);
		}
	}
}

