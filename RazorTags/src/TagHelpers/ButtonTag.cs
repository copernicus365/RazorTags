using System;
using System.Net;
using System.Text;
using DotNetXtensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace RazorTags
{
	[HtmlTargetElement("a", Attributes = "kind")] // very important perf wise we don't run this for every anchor! must have 'kind' attribute
	public class AnchorTag : ButtonTag
	{
		public override int Order => base.Order - 1;

		[HtmlAttributeName("href")]
		public string Href { get => _Href; set => _Href = value; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Kind == BsKind.None)
				output.SuppressOutput();
			else {
				output.TagName = "a";
				base.Process(context, output);
			}
		}
	}

	[HtmlTargetElement("button-link")] // very important perf wise we don't run this for every anchor! must have 'kind' attribute
	public class ButtonLinkTag : ButtonTag
	{
		public override int Order => base.Order - 1;

		[HtmlAttributeName("href")]
		public string Href { get => _Href; set => _Href = value; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "a";
			if(Kind == BsKind.None)
				Kind = BsKind.Primary;
			base.Process(context, output);
		}
	}

	[HtmlTargetElement("submit-button")] // very important perf wise we don't run this for every anchor! must have 'kind' attribute
	public class SubmitButtonTag : ButtonTag
	{
		public static string ContentDefault = "Submit";

		public override int Order => base.Order - 1;

		[HtmlAttributeName("href")]
		public string Href { get => _Href; set => _Href = value; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "button";

			if (Kind == BsKind.None)
				Kind = BsKind.Primary;

			string btnType = context.AllAttributes["type"]?.Value?.ToString();
			if (btnType.IsNulle())
				output.Attributes.SetAttribute("type", "submit");

			if (output.Content.IsEmptyOrWhiteSpace && ContentDefault.NotNulle())
				output.Content.AppendHtml(ContentDefault);

			base.Process(context, output);
		}
	}


	/// <summary>
	/// Adds options to the default button type. If no properties are set, this
	/// should remain the default button type, with no changes made.
	/// </summary>
	[HtmlTargetElement("button", Attributes = "kind")]
	public class ButtonTag : TagHelper
	{
		//public ButtonTag(IHtmlGenerator generator)
		//{
		//	this.generator = generator;
		//	/*
		//	tagBuilder = Generator.GenerateActionLink(
		//		ViewContext,
		//		linkText: string.Empty,
		//		actionName: Action,
		//		controllerName: Controller,
		//		protocol: Protocol,
		//		hostname: Host,
		//		fragment: Fragment,
		//		routeValues: routeValues,
		//		htmlAttributes: null);
				
		//	output.MergeAttributes(tagBuilder);
		//	*/
		//}
		//IHtmlGenerator generator;

		public static bool OutlineStyleDef = false;

		/// <summary>
		/// Sets the button to the given bootstrap style (appropriate classes etc will be set for this).
		/// </summary>
		[HtmlAttributeName("kind")]
		public BsKind Kind { get; set; } = BsKind.None;

		/// <summary>
		/// True to display as a simple link, while still allowing
		/// other button features to be used here. Note: this is
		/// different from the <see cref="_isAnchor"/> property, which
		/// indicates the actual button's tag should be an anchor `a`, but
		/// that it should otherwise still be STYLED as a button. This
		/// property on the other hand says to actually style this 
		/// `button` as a link as well, while the tag may still be set to `button`.
		/// </summary>
		[HtmlAttributeName("link-style")]
		public bool LinkStyle { get; set; }

		[HtmlAttributeName("outline")]
		public bool OutlineStyle { get; set; } = OutlineStyleDef;

		[HtmlAttributeName("disabled")]
		public bool Disabled { get; set; }

		[HtmlAttributeName("block")]
		public bool IsBlock { get; set; }

		public override int Order => -1;

		protected string _Href { get; set; }

		[HtmlAttributeName("size")]
		public BsSize Size { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Kind == BsKind.None) {
				output.SuppressOutput();
				return;
			}

			if (output.TagName == null) // || btnType.IsNulle() || Href.NotNulle())
				output.TagName = "a";
			bool isAnchor = output.TagName == "a";
			bool isButton = !isAnchor && output.TagName == "button";

			if (isButton) {
				string btnType = 
					output.Attributes["type"]?.Value?.ToString() ?? // submit-button or another child class may have already set this to output
					context.AllAttributes["type"]?.Value?.ToString();

				if (btnType.IsNulle())
					output.Attributes.SetAttribute("type", "button");
			}
			else if (isAnchor) {
				string __href = _Href;//.FirstNotNulle("javascript:void(0);");
				if(_Href != null)
					output.Attributes.SetAttribute("href", __href);
				// note: bootstrap doesn't style right without this, an empty href click causes the page to reload 
				// (in Chrome at least), this seems to work perfect, and note it doesn't stop other things, like onclick event...
			}

			string btnStyleName = LinkStyle ? "link" : Kind.Name();

			if (btnStyleName.NotNulle()) {
				string sizeClass = Size.NameButton(); // ONLY add size if is actually a bsStyle btn in the end...

				string btnClass = OutlineStyle
					? $"btn btn-outline-{btnStyleName} {sizeClass}"
					: $"btn btn-{btnStyleName} {sizeClass}";

				output.Attributes
					.AddOrAppendClass(
						btnClass,
						checkForDuplicates: true,
						dontAddIfPred: classVal => classVal.NotNulle() && classVal.Contains("btn"));
			}

			if (Disabled) {
				if (isButton) {
					output.Attributes.Add("disabled", "");
				}
				else if (isAnchor) {
					output.Attributes.AddOrAppendClass("disabled");
					output.Attributes.Add("aria-disabled", "true");
					output.Attributes.Add("tabindex", "-1");
				}
			}

			if (IsBlock && !LinkStyle) {
				output.Attributes.AddOrAppendClass("btn-block");
			}
		}

	}
}


//public override void Process(TagHelperContext context, TagHelperOutput output)
//{
//	string btnType = context.AllAttributes["type"]?.Value?.ToString();

//	if (AsAnchor || output.TagName == null || btnType.IsNulle() || Href.NotNulle())
//		output.TagName = "a";

//	AsAnchor = output.TagName == "a";
//	bool isButton = !AsAnchor && output.TagName == "button";

//	if (AsAnchor && Kind == BsKind.None) // if is 'a' tag, has already custom settings, so we'll have a default bd kind *only* for such custom types
//		Kind = BsKind.Primary;

//	if (AsAnchor) {
//		string _href = Href.IsNulle()
//			? "javascript:void(0);"
//			: Href;
//		output.Attributes.SetAttribute("href", _href); // note: bootstrap doesn't style right without this, and, an empty href click causes the page to reload (in Chrome at least)
//	}

//	// Nothing else is done if one of these 2 is not true... other properties depend on having a bs-styled button
//	if (Kind != BsKind.None || AsAnchor) {
//		string btnStyleName = LinkStyle
//			? "link"
//			: Kind.Name();

//		if (btnStyleName.NotNulle()) {

//			// ONLY add size if is actually a bsStyle btn in the end...
//			string sizeClass = Size.NameButton();

//			string btnClass = OutlineStyle
//				? $"btn btn-outline-{btnStyleName} {sizeClass}"
//				: $"btn btn-{btnStyleName} {sizeClass}";

//			output.Attributes
//				.AddOrAppendClass(
//					btnClass,
//					checkForDuplicates: true,
//					dontAddIfPred: classVal => classVal.NotNulle() && classVal.Contains("btn"));
//		}

//		if (Disabled) {
//			if (isButton) {
//				output.Attributes.Add("disabled", "");
//			}
//			else if (AsAnchor) {
//				output.Attributes.AddOrAppendClass("disabled");
//				output.Attributes.Add("aria-disabled", "true");
//				output.Attributes.Add("tabindex", "-1");
//			}
//		}

//		if (IsBlock) {
//			output.Attributes.AddOrAppendClass("btn-block");
//		}
//	}
//}
