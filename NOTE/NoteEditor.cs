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
		public string[] Tags {
			get { return INoteExtensions.TagArrayFromTagString(entryTags.Text); }
			set {
				entryTags.Text = INoteExtensions.TagStringFromArray(value);
			}
		}
		/*
		public string Tags {
			get { return entryTags.Text; }
			set { entryTags.Text = value; }
		}
		*/

		public void Clear() {
			entryTitle.Text = String.Empty;
			textviewContent.Buffer.Text = String.Empty;
			entryTags.Text = String.Empty;
		}

		public NoteEditor ()
		{
			this.Build ();
			buttonSave.Activated += (object sender, EventArgs e) => {
				Console.WriteLine("Yay");
			};
			//delegate void printer();
			//printer = (void)=> Console.WriteLine ("Test");
			Gtk.AccelGroup aG = new Gtk.AccelGroup();
			Gtk.AccelKey aKey= new Gtk.AccelKey(Gdk.Key.S, Gdk.ModifierType.ControlMask, Gtk.AccelFlags.Visible);
			buttonSave.AddAccelerator("activate", aG, aKey);

		}

		public void LoadNote(INote n) {
			Title = n.Title;
			Content = n.Content;
			Tags = n.Tags;
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

