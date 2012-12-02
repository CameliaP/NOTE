/// <summary>
/// Custom note event arguments.
/// </summary>

using System;

namespace NOTE
{
	public class NewNotesEventArgs: EventArgs
	{
		public Gtk.ListStore Store {get;set;}

		public NewNotesEventArgs (Gtk.ListStore s = null)
		{
			Store = s;
		}
	}
}

