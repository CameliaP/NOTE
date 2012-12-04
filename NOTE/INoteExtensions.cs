using System;

namespace NOTE
{
	public static class INoteExtensions
	{
		public static string TagSeparator = " ";

		public static string GetTagString(this INote note) {
			return TagStringFromArray(note.Tags);
		}

		public static string TagStringFromArray(string[] tags) {
			return string.Join (TagSeparator, tags);
		}

		public static void SetTagsFromTagString(this INote note, string tags) {
			note.Tags = TagArrayFromTagString(tags);
		}

		public static string[] TagArrayFromTagString(string tags) {
			return tags.ToLower().Split (TagSeparator.ToCharArray());
		}
	}
}

