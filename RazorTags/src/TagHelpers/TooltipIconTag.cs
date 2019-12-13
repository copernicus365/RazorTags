using DotNetXtensions;
using FontIcons;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	[HtmlTargetElement("tooltip-icon", TagStructure = TagStructure.WithoutEndTag)]
	public class TooltipIconTag : TooltipTag
	{
		public static string IconClassDefault = FA.InfoCircle;

		public static string TagNameDefault = "i";

		public TooltipIconTag(string message = null, bool isHtml = false, bool isPopover = false, string popOverTitle = null)
		{
			this.Message = message;
			this.IsHtml = isHtml;
			PopupTitle = popOverTitle;
			AsPopup = isPopover;
		}

		public string TagName { get; set; }

		[HtmlAttributeName("icon")]
		public string IconClass { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			base.Process(null, output);

			output.TagName = TagName ?? TagNameDefault;
			output.TagMode = TagMode.StartTagAndEndTag; // 'i' MUST have end tag, browsers go bonkers otherwise!

			string cssClass = IconClass ?? IconClassDefault;

			string _currCssClass = context?.AllAttributes?["class"]?.Value?.ToString();

			if (_currCssClass.NotNulle())
				cssClass = cssClass + " " + _currCssClass;

			output.Attributes.SetAttribute("class", cssClass);
		}

	}
}