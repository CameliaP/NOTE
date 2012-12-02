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
}
