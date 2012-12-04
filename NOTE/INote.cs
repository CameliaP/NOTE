using System;

namespace NOTE
{
	public interface INote
	{
		string Title {get;set;}
		string Content {get;set;}
		string[] Tags {get;set;} //TODO Convert this to HashSet next
	}
}

