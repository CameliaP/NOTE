using System;
using System.Collections.Generic;

namespace NOTE
{
	public static class INoteExtensions
	{
		public static string TagSeparator = " ";

		public static string GetTagString(this INote note) {
			return TagStringFromArray(note.Tags);
		}

		public static string TagStringFromArray(ISet<string> tags) {
			return string.Join (TagSeparator, tags);
		}

		public static void SetTagsFromTagString(this INote note, string tags) {
			note.Tags = TagArrayFromTagString(tags);
		}

		public static ISet<string> TagArrayFromTagString(string tags) {
			return new HashSet<string> (tags.ToLower().Split (TagSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
		}
	}
}

