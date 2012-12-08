using System;
using System.Collections.Generic;

namespace NOTE
{
	public interface INote
	{
		string Title {get;set;}
		string Content {get;set;}
		ISet<string> Tags {get;set;}
	}
}

