using DotNetXtensions;
using FontIcons;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	[HtmlTargetElement("icon", TagStructure = TagStructure.WithoutEndTag)]
	public class IconTag : TagHelper
	{
		public static string IconClassDefault = FA.InfoCircle;

		public static string TagNameDefault = "i";

		public IconTag(string icon = null, BsKind? color = null)
		{
			IconClass = icon;
			if (color != null)
				Color = color.Value;
		}

		public string TagName { get; set; }

		/// <summary>
		/// Icon class style(s).
		/// </summary>
		[HtmlAttributeName("icon")]
		public string IconClass { get; set; }

		[HtmlAttributeName("color")]
		public BsKind Color { get; set; }


		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			base.Process(null, output);

			output.TagName = TagName ?? TagNameDefault;
			output.TagMode = TagMode.StartTagAndEndTag; // 'i' MUST have end tag, browsers go bonkers otherwise!

			string cssClass = IconClass ?? IconClassDefault;

			string _currCssClass = context?.AllAttributes?["class"]?.Value?.ToString();

			if (_currCssClass.NotNulle())
				cssClass = cssClass + " " + _currCssClass;

			if (Color != BsKind.None) {
				string txtColor = Color.TextColor();
				if (txtColor.NotNulle()) {
					cssClass += $" {txtColor}";
				}
			}

			output.Attributes.SetAttribute("class", cssClass);
		}

	}
}