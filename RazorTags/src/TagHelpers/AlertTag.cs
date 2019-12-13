//Influenced by code from Rick Strahl 

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetXtensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	[HtmlTargetElement("alert")]
	public class AlertTag : TagHelper
	{
		/// <summary>
		/// the main message that gets displayed
		/// </summary>
		[HtmlAttributeName("message")]
		public string message { get; set; }

		/// <summary>
		/// Optional header that is displayed in big text. Use for 
		/// 'noisy' warnings and stop errors only please :-)
		/// The message is displayed below the header.
		/// </summary>
		[HtmlAttributeName("header")]
		public string header { get; set; }

		/// <summary>
		/// If true embeds the header text as HTML. Use this 
		/// flag if you need to display raw HTML text. If false
		/// the text is HtmlEncoded.
		/// </summary>
		[HtmlAttributeName("header-as-html")]
		public bool headerAsHtml { get; set; }

		/// <summary>
		/// Font-awesome icon name without the fa- prefix.
		/// Example: info, warning, lightbulb-o, 
		/// If none is specified - "warning" is used
		/// To force no icon use "none"
		/// </summary>
		[HtmlAttributeName("icon")]
		public string icon { get; set; }

		/// <summary>
		/// CSS class. Handled here so we can capture the existing
		/// class value and append the BootStrap alert class.
		/// </summary>
		[HtmlAttributeName("class")]
		public string cssClass { get; set; }

		[HtmlAttributeName("kind")]
		public BsKind AlertKind { get; set; } = BsKind.Info;

		/// <summary>
		/// If true embeds the message text as HTML. Use this 
		/// flag if you need to display HTML text. If false
		/// the text is HtmlEncoded.
		/// </summary>
		[HtmlAttributeName("message-as-html")]
		public bool messageAsHtml { get; set; }

		/// <summary>
		/// If true displays a close icon to close the alert.
		/// </summary>
		[HtmlAttributeName("dismissible")]
		public bool dismissible { get; set; } = true;

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			message = message.NotNulle()
				? message.HtmlEncode()
				: (await output.GetChildContentAsync()).GetContent();

			if (message.IsNulle() && header.IsNulle())
				return;

			output.TagName = "div";

			string bsTypeStr = AlertKind.Name();

			icon = icon.NullIfEmptyTrimmed();
			if (icon.IsNulle())
				icon = GetAlertIcon();

			if (dismissible)
				bsTypeStr += " alert-dismissible";

			cssClass = string.Concat(cssClass, cssClass.IsNulle() ? null : " ", "alert alert-", bsTypeStr);

			output.Attributes.Add("class", cssClass);
			output.Attributes.Add("role", "alert");

			StringBuilder sb = new StringBuilder();

			if (dismissible)
				sb.Append(
					"<button type =\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\">\r\n" +
					"   <span aria-hidden=\"true\">&times;</span>\r\n" +
					"</button>\r\n");

			if (header.IsNulle())
				sb.AppendLine($"<i class='fa fa-{icon}'></i> {message}");
			else {
				string headerText = !headerAsHtml ? header.HtmlEncode() : header;
				sb.Append(
					$"<h3><i class='fa fa-{icon}'></i> {headerText}</h3>\r\n" +
					"<hr/>\r\n" +
					$"{message}\r\n");
			}
			output.Content.SetHtmlContent(sb.ToString());
		}

		public string GetAlertIcon()
		{
			switch (AlertKind) {
				case BsKind.Info:
					return "info-circle";
				case BsKind.Danger:
				case BsKind.Warning:
					return "warning text-danger";
			}
			return null;
		}

	}
}