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

		protected ISet<Note> notes;
		protected Dictionary<string, Tag> tagDict;

		private string dataFile = "notes.dat"; //TODO allow flexible path
		public string DataFile {get;set;}

		public int Count {
			get { return notes.Count; }
		}

		public enum TagCols {
			Name, Count
		}

		public enum NoteCols {
			Title, NoteRef
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
			notes = new HashSet<Note> ();
			tagDict = new Dictionary<string, Tag> ();
		}
		#endregion

		#region Serialization
		//TODO check if object is null
		public bool SaveToFile() {
			using(Stream s = new FileStream(dataFile, FileMode.Create)) {
				BinaryFormatter bf = new BinaryFormatter();
				try {
					bf.Serialize(s, notes);
					bf.Serialize(s, tagDict);
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
					//TODO: WE NEED TO HANDLE SERIALIZATION OF WRONG OBJECTS.
					notes = bf.Deserialize(s) as HashSet<Note>; //check if object type is correct.
					tagDict = (Dictionary<string, Tag>) bf.Deserialize(s);
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

		#region Addition/Removal
		public void Add(INote note) {
			Note n = new Note(note);
			Add(n);
		}

		public void Add(Note note) {
			notes.Add(note);
			//NOTE order is important here.
			AddToTagRecord(note);
			AddToStore(note);
			//TODO buffering or something?
			SaveToFile();
		}

		public void AddToTagRecord(Note note) {
			foreach(String tag in note.Tags) {
				if(!tagDict.ContainsKey(tag)) {
					tagDict[tag] = new Tag(tag, 1, note);
				} else {
					//implies that tag has already been created
					tagDict[tag].Count++;
				}
			}
		}

		/// <summary>
		/// Make sure all references to obsolete Tag object is removed.
		/// </summary>
		/// <param name='tag'>
		/// Tag.
		/// </param>
		private void DeleteTag (Tag tag) {
			Gtk.TreeIter treeIter = tag.TreeIter;
			TagStore.Remove(ref treeIter);
			//tag.TreeIter = Gtk.TreeIter.Zero; //might not be necessary -- our only source to tag is tagDict
			tagDict.Remove(tag.Name); //also damn important
		}

		private void RemoveTagsFrom (Note note)
		{
			foreach (String tagStr in note.Tags) {
				Tag tag = tagDict[tagStr];

				if(tag.Count == 1) {
					DeleteTag(tag);
				} else {
					TagStore.SetValue(tag.TreeIter, (int)TagCols.Count, --tag.Count);
					tag.Notes.Remove (note);
				}
			}
		}

		public void Remove(Note note) {
			notes.Remove(note);
			Gtk.TreeIter treeIter = note.TreeIter;
			ListStore.Remove(ref treeIter);
			RemoveTagsFrom(note);

			//TODO buffering or something?
			SaveToFile();
		}



		private void AddToStore(Note note) {
			//Title store
			AddNoteToStore(note);
			//Tags store
			foreach(string tag in note.Tags) {
				AddTagToStore(tagDict[tag]);
			}
		}

		private void AddNoteToStore(Note note) {
			note.TreeIter = ListStore.AppendValues(note.Title, note);
		}

		private void AddTagToStore(Tag tag) {
			if(tag.TreeIter.Equals (Gtk.TreeIter.Zero)) {
				tag.TreeIter = TagStore.AppendValues(tag.Name, tag.Count);
			} else {
				TagStore.SetValue(tag.TreeIter, (int)TagCols.Count, tag.Count);
			}
		}
		#endregion

		#region GtkStores
		private Gtk.ListStore MakeTagStore() {
			if(TagStore == null)
				TagStore = new Gtk.ListStore(typeof(string), typeof(int));
			foreach(Tag tag in tagDict.Values) {
				AddTagToStore(tag);
			}
			return TagStore;
		}

		private Gtk.ListStore MakeListStore() {
			ListStore = new Gtk.ListStore(typeof(string), typeof(Note));
			foreach(Note n in notes) {
				AddNoteToStore(n);
			}
			return ListStore;
		}
		#endregion
	}
}

