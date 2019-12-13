using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	[HtmlTargetElement(Attributes = "popup")]
	public class PopupTag : TooltipTag
	{
		public override int Order => base.Order - 1;

		/// <summary>
		/// The popup message.
		/// </summary>
		/// <remarks>Internally this just sets <see cref="TooltipTag.Message"/>
		/// as <see cref="TooltipTag.AsPopup"/> to true.
		/// </remarks>
		[HtmlAttributeName("popup")]
		public string PopupMessage {
			get => Message;
			set {
				AsPopup = true;
				Message = value;
			}
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			AsPopup = true;
			base.Process(context, output);
		}
	}
}