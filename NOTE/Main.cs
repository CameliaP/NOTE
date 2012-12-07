using System;
using Gtk;

//TODO add two-step history to allow for revert of accidental deletions.

namespace NOTE
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			//Test();
			Application.Run ();
		}

		public static void Test() {
			NotesModel notes = new NotesModel();
			Note n = new Note {
				Title = "Testing",
				Content = "Works"
			};
			//notes.Add(n);
			//notes.SaveToFile();
			notes.LoadFromFile();
			//Note loaded = notes.[0];
			//Console.WriteLine (loaded.Title);
		}
	}
}
