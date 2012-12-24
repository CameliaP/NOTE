using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace NOTE
{
	/// <summary>
	/// Holds a bunch of notes and provide convenience functions.
	/// TODO make this implement the TreeModel interface?
	/// TODO uncouple this class from ListStore?
	/// </summary>
	public class NotesModel
	{
		public Gtk.ListStore NotesStore {get;private set;}
		public Gtk.ListStore TagStore {get;private set;}

		private ISet<Note> notes;
		private IDictionary<string, Tag> tagDict;
		private IDictionary<string, Tag> searchTagDict;
		private IDictionary<string, Tag> TagDict {
			get {
				return InSearchMode ? searchTagDict : tagDict;
			}
			set {
				if(InSearchMode) {
					searchTagDict = value;
				} else {
					tagDict = value;
				}
			}
		}

		public bool InSearchMode {get;set;}

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
			TagStore = MakeTagStore();
			NotesStore = MakeNotesStore();
			InSearchMode = false;
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
			using(Stream s = File.OpenRead (dataFile)) {
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
			NotesStore.SetValue(note.TreeIter, (int)NoteCols.Title, note.Title);

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
			AddTagsFrom(note, tags, tagDict);
		}

		private void AddTagsFrom(Note note, IEnumerable<string> tags, IDictionary<String,Tag> tagDict) {
			AddTagsFrom(note, tags, tagDict, TagStore);
		}

		private void AddTagsFrom(Note note, IEnumerable<string> tags, IDictionary<String,Tag> tagDict,
		                         Gtk.ListStore tagStore) {
			foreach(String tag in tags) {
				if(!tagDict.ContainsKey(tag)) {
					tagDict[tag] = new Tag(tag, note);
				} else {
					//implies that tag has already been created
					tagDict[tag].Notes.Add(note);
				}
				AddTagToStore(tagDict[tag], tagStore);
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
					tag.Notes.Remove (note);
					TagStore.SetValue(tag.TreeIter, (int)TagCols.Count, tag.Count);
				}
			}
		}

		public void Remove(Note note) {
			notes.Remove(note);
			Gtk.TreeIter treeIter = note.TreeIter;
			NotesStore.Remove(ref treeIter);
			RemoveTagsFrom(note);

			//TODO buffering or something?
			SaveToFile();
		}
		
		private void AddNoteToStore (Note note)
		{
			AddNoteToStore(note,NotesStore);
		}

		private void AddNoteToStore (Note note, Gtk.ListStore notesStore)
		{
			if (note.TreeIter.Equals (Gtk.TreeIter.Zero)) {
				note.TreeIter = notesStore.AppendValues (note.Title, note);
				Debug.Assert(!note.TreeIter.Equals(Gtk.TreeIter.Zero));
			} else {
				notesStore.SetValue(note.TreeIter, (int)NoteCols.Title, note.Title);
			}
		}

		private void AddTagToStore(Tag tag) {
			AddTagToStore(tag, TagStore);
		}

		private void AddTagToStore(Tag tag, Gtk.ListStore tagStore) {
			if(tag.TreeIter.Equals (Gtk.TreeIter.Zero)) {
				tag.TreeIter = tagStore.AppendValues(tag.Name, tag.Count);
				Debug.Assert(!tag.TreeIter.Equals(Gtk.TreeIter.Zero));
			} else {
				tagStore.SetValue(tag.TreeIter, (int)TagCols.Count, tag.Count);
			}
		}
		#endregion

		#region GtkStores
		public void MakeSearchStore(String term,
		                            out Gtk.ListStore notesStore, out Gtk.ListStore tagStore) {
			Gtk.ListStore notesStoreOut, tagStoreOut;
			MakeSearchStore(term, this.notes, out notesStoreOut, out tagStoreOut);
			notesStore = notesStoreOut;
			tagStore = tagStoreOut;
		}

		//For search, we have to make separate list and tag stores corresponding to found notes
		//Function goes through Notes List, find notes, add them to search tag dict
		//Then make a GtkListStore.
		//TODO what if a tag is already selected?
		public void MakeSearchStore(String term, IEnumerable<Note> notes,
		                            out Gtk.ListStore notesStore, out Gtk.ListStore tagStore) {
			searchTagDict = new Dictionary<String,Tag>();
			tagStore = new Gtk.ListStore(typeof(string), typeof(int));
			notesStore = MakeNotesStore(SearchNotes(term, notes, tagStore)); //after this, the searchTagDict should be done
			//tagStore = MakeTagStore(searchTagDict);
		}

		/// <summary>
		/// Goes through notes collection -- if note contains search term
		/// Then we add it to the searchTagDict.
		/// </summary>
		/// <returns> Notes that were gone through </returns>
		private IEnumerable<Note> SearchNotes(String term, IEnumerable<Note> notes, Gtk.ListStore tagStore) {
			foreach(Note note in notes) {
				if(note.Contains(term)) {
					AddTagsFrom(note, note.Tags, searchTagDict, tagStore);
					yield return note;
				}
			}
		}

		private Gtk.ListStore MakeTagStore() {
			return MakeTagStore(TagDict);
		}

		private Gtk.ListStore MakeTagStore(IDictionary<String,Tag> tagDict) {
			Gtk.ListStore tagStore = new Gtk.ListStore(typeof(string), typeof(int));
			foreach(Tag tag in tagDict.Values) {
				AddTagToStore(tag, tagStore);
			}
			return tagStore;
		}

		public Gtk.ListStore MakeNotesStore (string tag)
		{
			return MakeNotesStore(TagDict[tag].Notes);
		}

		private Gtk.ListStore MakeNotesStore ()
		{
			return MakeNotesStore(this.notes);
		}

		private Gtk.ListStore MakeNotesStore(IEnumerable<Note> notes) {
			Gtk.ListStore notesStore = new Gtk.ListStore(typeof(string), typeof(Note));
			foreach(Note n in notes) {
				n.TreeIter = Gtk.TreeIter.Zero;
				AddNoteToStore(n, notesStore);
			}
			return notesStore;
		}
		#endregion
	}
}

