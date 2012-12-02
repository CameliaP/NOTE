using System;
using Gtk;
using NOTE;

public partial class MainWindow: Gtk.Window
{	
	private NotesModel notes;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		//Notes
		TreeViewColumn notesCol = new TreeViewColumn();
		notesCol.Title = "Notes";
		treeviewNotes.AppendColumn(notesCol);
//		ListStore nStore = new ListStore(typeof (string));
//		treeviewNotes.Model = nStore;
//		nStore.AppendValues("HAHAHA");

		CellRendererText noteTitleCell = new CellRendererText();
		notesCol.PackStart(noteTitleCell, true);
		notesCol.AddAttribute(noteTitleCell, "text", 0);
		/*
		TreeSelection ts = treeviewNotes.Selection;
		ts.Mode = SelectionMode.Multiple;
		treeviewNotes.RubberBanding = true;
		*/
		//TODO if multiple selected disable NoteEditor


		//Event handler for notes
		notes = new NotesModel();
		treeviewNotes.Model = notes.ListStore;

		//Tags
		TreeViewColumn tagsCol = new TreeViewColumn();
		tagsCol.Title = "Tags";
		treeviewTags.AppendColumn(tagsCol);
		ListStore tagsStore = new ListStore(typeof (string));
		treeviewTags.Model = tagsStore;
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnNoteeditor1SaveEvent (object sender, EventArgs e)
	{
		if(!(sender is NoteEditor)) return;

		NoteEditor editor = (NoteEditor) sender;
		notes.Add(editor);
	}

	protected void OnNoteeditor1OverallKeyPressEvent (object sender, EventArgs e)
	{
		KeyPressEventArgs kpe = (KeyPressEventArgs) e;
		Console.WriteLine (kpe.Event.KeyValue);
	}

	protected void OnSaveActionActivated (object sender, EventArgs e)
	{
		Console.WriteLine("Absolutely.");
	}

	protected void OnTreeviewNotesCursorChanged (object sender, EventArgs e)
	{
		TreeView tv = (TreeView) sender;
		int index = GetSelectedIndex(tv);
		noteeditor1.LoadNote(notes[index]);
	}
	
	protected void OnDeleteNoteAction1Activated (object sender, EventArgs e)
	{
		DeleteCurrentSelection();
	}

	private void DeleteCurrentSelection() {
		Gtk.TreePath treePath;
		Gtk.TreeViewColumn treeColumn;
		treeviewNotes.GetCursor(out treePath, out treeColumn);

		notes.Remove(treePath);
		noteeditor1.Clear();
		//TODO make this less hackish
		if(treePath.Indices[0] < notes.Count)
			treeviewNotes.SetCursor(treePath, treeColumn, false);
		else if(treePath.Indices[0] == notes.Count){
			treePath.Prev();
			treeviewNotes.SetCursor(treePath, treeColumn, false);
		}
	}

	private Gtk.TreePath GetTreePath(TreeView tv) {
		Gtk.TreePath treePath;
		Gtk.TreeViewColumn treeColumn;
		tv.GetCursor(out treePath, out treeColumn);
		return treePath;
	}

	private int GetSelectedIndex(TreeView tv) {
		return GetTreePath(tv).Indices[0];
	}

	protected void OnTreeviewNotesKeyPressEvent (object o, KeyPressEventArgs args)
	{
		if(args.Event.Key == Gdk.Key.Delete)
			DeleteCurrentSelection();
	}

}

