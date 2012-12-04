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
		public Gtk.ListStore TagStore {get;private set;}

		protected List<Note> notesList;
		//TODO Given a tag, we can just sync the TagStore with the HashTag!
		protected Dictionary<string, HashSet<Note>> tagRecord;
		protected Dictionary<string, int> tagCount;

		private string dataFile = "notes.dat"; //TODO allow flexible path

		public int Count {
			get { return notesList.Count; }
		}

		public Note this[int key] {
			get {
				return notesList[key];
			}
		}

		#region Initialization
		//TODO auto load from file?
		public NotesModel ()
		{
			if (!File.Exists (dataFile)) {
				InitializeNew();
			} else {
				LoadFromFile();
			}
			MakeTagStore();
			MakeListStore();
		}

		private void InitializeNew() {
			notesList = new List<Note> ();
			tagRecord = new Dictionary<string, HashSet<Note>> ();
			tagCount = new Dictionary<string, int> ();
		}
		#endregion

		#region Serialization
		//TODO check if object is null
		public bool SaveToFile() {
			using(Stream s = new FileStream(dataFile, FileMode.Create)) {
				BinaryFormatter bf = new BinaryFormatter();
				try {
					bf.Serialize(s, notesList);
					bf.Serialize(s, tagRecord);
					bf.Serialize(s, tagCount);
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
					tagRecord = (Dictionary<string, HashSet<Note>>) bf.Deserialize(s);
					tagCount = (Dictionary<string, int>) bf.Deserialize(s);
				} catch(SerializationException) {
					//TODO better error mechanism.
					throw;
				} catch(ArgumentException) {
					//TODO handle older versions of database!
					InitializeNew();
					Console.Error.WriteLine("Cannot convert");
				}
			}
			return true;
		}
		#endregion

		#region Native stuff
		public void Add(INote note) {
			Note n = new Note(note);
			Add(n);
		}

		public void Add(Note note) {
			notesList.Add(note);
			//NOTE order is important here.
			AddToTagRecord(note);
			AddToStore(note);
			//TODO buffering or something?
			SaveToFile();
		}

		public void AddToTagRecord(Note note) {
			foreach(String tag in note.Tags) {
				if(!tagRecord.ContainsKey(tag)) {
					tagRecord[tag] = new HashSet<Note> ();
				}
				tagRecord[tag].Add(note);

				if(!tagCount.ContainsKey(tag)) {
					tagCount[tag] = 1;
				} else {
					tagCount[tag]++;
				}
			}
		}

		public void RemoveFromTagRecord (Note note)
		{
			foreach (String tag in note.Tags) {
				tagRecord[tag].Remove (note);
				tagCount[tag]--;
			}
		}

		public void Remove(Note note) {
			notesList.Remove(note);
			RemoveFromTagRecord(note);
		}
		#endregion

		#region Gtk stuff
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
			//Title store
			ListStore.AppendValues(note.Title);
			//Tags store
			//AddTagToStore
			foreach(string tag in note.Tags) {
				AddTagToStore(tag);
			}
		}

		private void AddNoteTitleToStore(Note note) {
			ListStore.AppendValues(note.Title);
		}
	
		private void AddTagToStore(string tag) {
			if(tagCount[tag] == 1)
				TagStore.AppendValues(tag);
		}

		private void AddTagToNewStore(string tag) {
			TagStore.AppendValues(tag);
		}

		//TODO remove tags zeroed...
		private Gtk.ListStore MakeTagStore() {
			if(TagStore == null)
				TagStore = new Gtk.ListStore(typeof(string));
			foreach(string tag in tagRecord.Keys) {
				AddTagToNewStore(tag);
			}
			return TagStore;
		}

		private Gtk.ListStore MakeListStore() {
			ListStore = new Gtk.ListStore(typeof(string));
			foreach(Note n in notesList) {
				AddNoteTitleToStore(n);
			}
			return ListStore;
		}
		#endregion
	}
}

