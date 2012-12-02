using System;

//TODO Make a common interface for NOTES!

namespace NOTE
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NoteEditor : Gtk.Bin, INote
	{
		public event EventHandler SaveEvent;
		public event EventHandler OverallKeyPressEvent;

		//TODO check whether this returns a copy or a reference?
		public string Title {
			get { return entryTitle.Text; }
			set { entryTitle.Text = value; }
		}
		public string Content {
			get { return textviewContent.Buffer.Text; }
			set { textviewContent.Buffer.Text = value; }
		}

		public NoteEditor ()
		{
			this.Build ();
		}

		protected void OnSave (object sender, EventArgs e)
		{
			if(SaveEvent != null)
				SaveEvent(this, null);
		}

		protected void OnTextviewContentKeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			OverallKeyPressEvent(this, args);
		}

		protected void OnEntryTitleKeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			OverallKeyPressEvent(this, args);
		}

		protected void OnButton1KeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			OverallKeyPressEvent(this, args);
		}
	}
}

