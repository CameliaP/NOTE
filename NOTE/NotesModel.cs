using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;

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
		protected IDictionary<string, Tag> tagDict;

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
		public void Update(Note note, INote newNote) {
			note.Title = newNote.Title;
			ListStore.SetValue(note.TreeIter, (int)NoteCols.Title, note.Title);

			note.Content = newNote.Content;

			//TODO: optimize this later.
			HashSet<string> intersectTags = new HashSet<string> (
					note.Tags.Intersect(newNote.Tags)
				);

			IEnumerable<string> obsoleteTags = note.Tags.Except(intersectTags);
			IEnumerable<string> newTags = newNote.Tags.Except(intersectTags);

			RemoveTagsFrom(note, obsoleteTags);
			AddTagsFrom(note, newTags);

			note.Tags = newNote.Tags;
			SaveToFile();
		}

		public void Add(INote note) {
			Note n = new Note(note);
			Add(n);
		}

		public void Add(Note note) {
			notes.Add(note);
			AddNoteToStore(note);
			AddTagsFrom(note);
			//TODO buffering or something?
			SaveToFile();
		}

		private void AddTagsFrom(Note note) {
			AddTagsFrom(note, note.Tags);
		}

		private void AddTagsFrom(Note note, IEnumerable<string> tags) {
			foreach(String tag in tags) {
				if(!tagDict.ContainsKey(tag)) {
					tagDict[tag] = new Tag(tag, 1, note);
				} else {
					//implies that tag has already been created
					tagDict[tag].Count++;
					tagDict[tag].Notes.Add(note);
				}
				AddTagToStore(tagDict[tag]);
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

		private void RemoveTagsFrom (Note note) {
			RemoveTagsFrom(note, note.Tags);
		}

		private void RemoveTagsFrom (Note note, IEnumerable<string> tags)
		{
			foreach (String tagStr in tags) {
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
		
		private void AddNoteToStore (Note note)
		{
			if (note.TreeIter.Equals (Gtk.TreeIter.Zero)) {
				note.TreeIter = ListStore.AppendValues (note.Title, note);
			} else {
				ListStore.SetValue(note.TreeIter, (int)NoteCols.Title, note.Title);
			}
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

