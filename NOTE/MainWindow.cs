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

		notes = new NotesModel();
		treeviewNotes.Model = notes.ListStore;

		//Tags
		TreeViewColumn tagsCol = new TreeViewColumn();
		tagsCol.Title = "Tags";
		TreeViewColumn tagsCountCol = new TreeViewColumn();
		tagsCountCol.SortIndicator = true;
		tagsCountCol.Title = "Count";
		treeviewTags.AppendColumn(tagsCol);
		treeviewTags.AppendColumn(tagsCountCol);
		treeviewTags.Model = notes.TagStore;

		CellRendererText tagTitleCell = new CellRendererText();
		tagsCol.PackStart(tagTitleCell, true);
		tagsCol.AddAttribute(tagTitleCell, "text", 0);

		CellRendererText tagCountCell = new CellRendererText();
		tagsCountCol.PackStart(tagCountCell, true);
		tagsCountCol.AddAttribute(tagCountCell, "text", 1);
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void SaveNote ()
	{
		Note newNote = MakeNote();

		TreeSelection selection = (treeviewNotes as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		if (selection.CountSelectedRows() == 1 && selection.GetSelected (out model, out iter)) {
			Note oldNote = model.GetValue(iter, (int)NotesModel.NoteCols.NoteRef) as Note;
			notes.Update(oldNote, newNote);
		} else {
			notes.Add (newNote);
		}
	}

	protected void OnTreeviewNotesCursorChanged (object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		
		TreeModel model;
		TreeIter iter;
		
		// The iter will point to the selected row
		if(selection.GetSelected(out model, out iter)) {
			Note note = model.GetValue (iter, 1) as Note;
			LoadNote(note);
		} 
	}
	protected void OnDeleteNoteAction1Activated (object sender, EventArgs e)
	{
		if(treeviewNotes.HasFocus)
			DeleteCurrentSelection();
	}

	private void DeleteCurrentSelection() {
		TreeSelection selection = (treeviewNotes as TreeView).Selection;
		
		TreeModel model;
		TreeIter iter;
		
		// The iter will point to the selected row
		if(selection.GetSelected(out model, out iter)) {
			Note note = model.GetValue (iter, (int)NotesModel.NoteCols.NoteRef) as Note;
			notes.Remove(note);
			ClearEditor();
		} 

		// TODO: move cursor to next selection?
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

	protected void OnSaveNoteAction2Activated (object sender, EventArgs e)
	{
		SaveNote();
	}
	protected void OnNewActionActivated (object sender, EventArgs e)
	{
		TreeSelection selection = (treeviewNotes as TreeView).Selection;
		selection.UnselectAll();

		ClearEditor();
		entryTitle.GrabFocus();
	}

	public void ClearEditor() {
		entryTitle.Text = String.Empty;
		textviewContent.Buffer.Text = String.Empty;
		entryTags.Text = String.Empty;
	}

	public Note MakeNote() {
		Note note = new Note();
		note.Title = entryTitle.Text;
		note.Content = textviewContent.Buffer.Text;
		note.Tags = INoteExtensions.TagArrayFromTagString(entryTags.Text);
		return note;
	}

	public void LoadNote(INote n) {
		entryTitle.Text = n.Title;
		textviewContent.Buffer.Text = n.Content;
		entryTags.Text = INoteExtensions.TagStringFromArray(n.Tags);
	}

	protected void OnButtonSaveReleased (object sender, EventArgs e)
	{
		SaveNote();
	}
}



