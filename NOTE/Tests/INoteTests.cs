using NUnit.Framework;
using System;
using System.Reflection;

namespace NOTE
{
	[TestFixture()]
	public class INoteTests
	{
		[Test()]
		public void TestNote ()
		{
			Note note = new Note();
			note.Title = "here";
			note.Content = "we";
			//note.Tags = "go";
			INote iNote = note;
			Note copyNote = new Note(iNote);
			Assert.AreNotSame(note, copyNote);
			Assert.AreEqual(note.Title, copyNote.Title);
			Assert.AreEqual(note.Content, copyNote.Content);
			Assert.AreEqual(note.Tags, copyNote.Tags);
		}

		//TODO fix test so it works
		[Test]
		public void TestNote2() {
			Note note = new Note();
			note.Title = "here";
			note.Content = "we";
			//note.Tags = "go";
			INote iNote = note;
			Note copyNote = new Note(iNote);

			Type noteType = typeof(INote);
			foreach(PropertyInfo pInfo in noteType.GetProperties(System.Reflection.BindingFlags.GetProperty)) {
				object o = pInfo.GetValue (copyNote);
				Assert.AreNotEqual(o, null);
				if (o is string) {
					Assert.AreEqual(note.Title, (string) o);
				}
			}
		}
	}
}

