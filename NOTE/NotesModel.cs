using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace NOTE
{
	/// <summary>
	/// Holds a bunch of notes and provide convenience functions.
	/// TODO make this implement the TreeModel interface?
	/// TODO uncouple this class from ListStore?
	/// </summary>
	public class NotesModel
	{
		public Gtk.ListStore ListStore {get;private set;}
		private string dataFile = "notes.dat"; //TODO allow flexible path
		protected List<Note> notesList;

		public int Count {
			get { return notesList.Count; }
		}

		public Note this[int key] {
			get {
				return notesList[key];
			}
		}

		//TODO auto load from file?
		public NotesModel ()
		{
			if (!File.Exists (dataFile)) {
				notesList = new List<Note> ();
				MakeListStore ();
			} else {
				LoadFromFile();
				MakeListStore ();
			}
		}

		//TODO check if object is null
		public bool SaveToFile() {
			using(Stream s = new FileStream(dataFile, FileMode.Create)) {
				BinaryFormatter bf = new BinaryFormatter();
				try {
					bf.Serialize(s, notesList);
					return true;
				} catch(SerializationException) {
					//TODO better error mechanism.
					throw;
				}
			}
		}

		public bool LoadFromFile ()
		{
			using(Stream s = new FileStream(dataFile, FileMode.Open)) {
				BinaryFormatter bf = new BinaryFormatter();
				try {
					notesList = (List<Note>) bf.Deserialize(s); //check if object type is correct.
				} catch(SerializationException) {
					//TODO better error mechanism.
					throw;
				}
			}
			return true;
		}

		public void Add(INote note) {
			Note n = new Note(note);
			Add(n);
		}

		public void Add(Note note) {
			notesList.Add(note);
			AddToStore(note);
			//TODO buffering or something?
			SaveToFile();
		}

		public void Remove(Note note) {
			notesList.Remove(note);
		}

		//TODO allow reordering of ListStore (sort by date, etc)
		public void Remove(Gtk.TreePath tp) {
			int index = tp.Indices[0];
			notesList.RemoveAt(index);
			Gtk.TreeIter iter;
			ListStore.GetIter(out iter, tp);
			ListStore.Remove(ref iter);
			SaveToFile(); //TODO save backup? as in... if overwrite existing file, then let's make backup.
		}

		private void AddToStore(Note note) {
			ListStore.AppendValues(note.Title);
		}

		private Gtk.ListStore MakeListStore() {
			ListStore = new Gtk.ListStore(typeof(string));
			foreach(Note n in notesList) {
				AddToStore(n);
			}
			return ListStore;
		}
	}
}

