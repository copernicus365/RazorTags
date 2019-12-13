using System.Collections.Generic;
using DotNetXtensions;

namespace FontIcons.FAHelper
{
	/// <summary>
	/// Note: this all may be quite obsolete now, and it was written (by myself, NP) in 2015. Not going to 
	/// erase it yet, but...
	/// </summary>
	public static class Obsolete_FAGroups
	{
		public const string con_HeaderLineVal = @"<h2 class=""page-header"">";
		public const string con_FontLineVal = @"<div class=""fa-hover col-md-3 col-sm-4""><a href=""../icon/";

		/// <summary>
		/// 
		/// Parses this page (https://fortawesome.github.io/Font-Awesome/icons/) for meaningful
		/// categorization and ordering of its fonts.
		/// </summary>
		public static List<WebIconGroup> GetIconGroupsFromFAHomeIconPage(string iconsHtmlPage)
		{
			if (iconsHtmlPage.IsNulle())
				return null;

			string[] arr = iconsHtmlPage.SplitLines(trimLines: true, removeEmptyLines: true);
			if (arr.IsNulle())
				return null;

			var groups = new List<WebIconGroup>();
			WebIconGroup currGrp = null;

			for (int i = 0; i < arr.Length; i++) {
				string l = arr[i].TrimN();

				if (l.IsNulle())
					continue;

				if (l.StartsWith(con_HeaderLineVal)) {
					l = l.Substring(con_HeaderLineVal.Length);
					int idx = l.IndexOf("</h2>");
					if (idx > 2) {
						l = l.Substring(0, idx);
						currGrp = new WebIconGroup() { Title = l };
						groups.Add(currGrp);
					}
				}
				else if (l.StartsWith(con_FontLineVal)) {
					l = l.Substring(con_FontLineVal.Length);
					int idx = l.IndexOf('"');
					if (idx > 1) {
						string name = l.Substring(0, idx);
						bool isAlias = l.Contains("(alias)");
                        currGrp.Icons.Add(new WebIcon() { Name = name, IsAlias = isAlias });
					}
				}
			}
			return groups;
        }

	}

	public class WebIconGroup
	{
		public string Title { get; set; }

		public List<WebIcon> Icons { get; set; } = new List<WebIcon>();

	}

	public class WebIcon
	{
		public string Name { get; set; }

		public string FullName { get; set; }

		public string Hex { get; set; }

		public bool IsAlias { get; set; }

	}
}
