using DotNetXtensions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	[HtmlTargetElement(Attributes = "tooltip")]
	public class TooltipTag : TagHelperHtmlContent
	{
		protected string Message { get; set; }

		[HtmlAttributeName("tooltip")]
		public string TooltipMsg { get => Message; set => Message = value; }

		/* ---- NAMING SCHEME: ttip- notes:
		 * Since we are selecting based only on a single attribute (tooltip)
		 * so I think it is preferrable to have the other properties show up only
		 * under a prefix like this. In looking at the razor in practise, it sure looks
		 * clearer this way, and is not all that onerous. It is true that in the case
		 * of 'is-html' that the generated form is still just `html="true"`, but 
		 * still, this is for our own experience.
		 */


		/// <summary>
		/// True to make the tooltip a popover instead of a tooltip.
		/// Note that for this you can simply use the <see cref="PopupTag"/>
		/// instead.
		/// </summary>
		[HtmlAttributeName("ttip-as-popover")]
		public bool AsPopup { get; set; }

		/// <summary>
		/// True to have the tooltip or popover text interpreted 
		/// and displayed as html. Otherwise the input message will
		/// be html encoded (escaped).
		/// </summary>
		[HtmlAttributeName("ttip-is-html")] // ttip-html
		public bool IsHtml { get; set; }

		/// <summary>
		/// Tooltip placement.
		/// </summary>
		[HtmlAttributeName("ttip-placement")]
		public Placement TpPlacement { get; set; } //= Placement.Top;

		/// <summary>
		/// Popover title, if any.
		/// </summary>
		[HtmlAttributeName("pup-title")]
		public string PopupTitle { get; set; }

		/// <summary>
		/// True to have the popover close as soon as you click another element. 
		/// According to BS docs, this does not work on buttons (for button look, do an
		/// anchor instead styled as btn).
		/// </summary>
		[HtmlAttributeName("pup-focus")]
		public bool PopupFocus { get; set; } = true;

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			string tagName = output.TagName;
			var atts = output.Attributes;

			/*
			 title="msg" --> content for tooltip, or for popover is actually the title
			 data-content="msg" --> content for popover only
			 data-toggle="popover|tooltip"
			 data-html="true"
			 data-placement="left"
			 */

			if (Message.IsNulle()) {
				output.SuppressOutput();
				return;
			}

			string msg = IsHtml ? Message : Message.HtmlEncode(true);

			atts.SetAttribute("data-toggle", AsPopup ? "popover" : "tooltip");

			if (AsPopup && PopupTitle.NotNulle())
				atts.SetAttribute("title", PopupTitle);

			atts.SetAttribute(AsPopup ? "data-content" : "title", msg);

			if (TpPlacement != Placement.None)
				atts.SetAttribute("data-placement", TpPlacement.Name());

			if (IsHtml)
				atts.SetAttribute("data-html", "true");

			if (AsPopup && PopupFocus && tagName != "button") {
				atts.SetAttribute("tabindex", "0");
				atts.SetAttribute("data-trigger", "focus");
			}

			if (!AsPopup) {
				var currAtts = context?.AllAttributes;
				if (currAtts.NotNulle()) {
					var ddd = currAtts["pup-focus"];
					if (ddd != null) {
						ddd.Print();
					}
				}
			}
		}
	}
}