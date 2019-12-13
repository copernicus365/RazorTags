//using System;
//using System.Net;
//using System.Text;
//using DotNetXtensions;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.AspNetCore.Razor.TagHelpers;
//using Microsoft.AspNetCore.Razor;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Mvc;

//namespace RazorTags
//{
//	//[HtmlTargetElement("btn")]
//	//public class BtnTag : ButtonTag
//	//{
//	//	public BtnTag(IHtmlGenerator generator) : base(generator)
//	//	{
//	//		Kind = BsKind.Primary;

//	//	}

//	//}

//	/// <summary>
//	/// Adds options to the default button type. If no properties are set, this
//	/// should remain the default button type, with no changes made.
//	/// </summary>
//	[HtmlTargetElement("button")]
//	public class ButtonTag : TagHelper
//	{
//		public ButtonTag(IHtmlGenerator generator)
//		{
//			_generator = generator;

//			var vc = ViewContext;
//			//vc.HttpContext.
//			//UrlHelperExtensions.Action(Url, "Details", "Resellers", new { id = 1 })
//			//vc.RouteData
//		}

//		IHtmlGenerator _generator;

//		[HtmlAttributeNotBound]
//		[ViewContext]
//		public ViewContext ViewContext { get; set; }

//		/// <summary>
//		/// Sets the button to the given bootstrap style (appropriate classes etc will be set for this).
//		/// </summary>
//		[HtmlAttributeName("kind")]
//		public BsKind Kind { get; set; } = BsKind.None;

//		/// <summary>
//		/// True to ensure that the button generated is actually an anchor tag `a`,
//		/// while still allowing it to be styled like a button. This allows other things,
//		/// like allowing the anchor to have an href.
//		/// </summary>
//		[HtmlAttributeName("as-anchor")]
//		public bool AsAnchor { get; set; }

//		/// <summary>
//		/// True to display as a simple link, while still allowing
//		/// other button features to be used here. Note: this is
//		/// different from the <see cref="AsAnchor"/> property, which
//		/// indicates the actual button's tag should be an anchor `a`, but
//		/// that it should otherwise still be STYLED as a button. This
//		/// property on the other hand says to actually style this 
//		/// `button` as a link as well, and the tag may still be set to `button`.
//		/// </summary>
//		[HtmlAttributeName("link-style")]
//		public bool LinkStyle { get; set; }

//		public static bool OutlineStyleDef = false;

//		[HtmlAttributeName("outline")]
//		public bool OutlineStyle { get; set; } = OutlineStyleDef;

//		[HtmlAttributeName("disabled")]
//		public bool Disabled { get; set; }

//		[HtmlAttributeName("block")]
//		public bool IsBlock { get; set; }

//		public override int Order => -1;

//		[HtmlAttributeName("href")]
//		public string Href { get; set; }

//		[HtmlAttributeName("size")]
//		public BsSize Size { get; set; }

//		[HtmlAttributeName("action-url")]
//		public (string action, string controller, object routes)? ActionUrl { get; set; }

//		protected string _href;

//		//[HtmlAttributeName("href")]
//		//public string Href { get; set; }

//		//string __SetLink(string actionName, string controllerName, object routeValues)
//		//{
//		//	_generator.GenerateActionLink(
//		//		ViewContext,

//		//	//m_Href = m_Html.ActionUrl(actionName, controllerName, routeValues);
//		//	//SetAttribute("href", m_Href ?? "");
//		//}

//		public override void Process(TagHelperContext context, TagHelperOutput output)
//		{
//			string btnType = context.AllAttributes["type"]?.Value?.ToString();

//			if (AsAnchor || output.TagName == null || btnType.IsNulle() || Href.NotNulle())
//				output.TagName = "a";

//			AsAnchor = output.TagName == "a";
//			bool isButton = !AsAnchor && output.TagName == "button";

//			if (AsAnchor && Kind == BsKind.None) // if is 'a' tag, has already custom settings, so we'll have a default bd kind *only* for such custom types
//				Kind = BsKind.Primary;

//			if (AsAnchor) {
//				string _href = Href.IsNulle()
//					? "javascript:void(0);"
//					: Href;
//				output.Attributes.SetAttribute("href", _href); // note: bootstrap doesn't style right without this, and, an empty href click causes the page to reload (in Chrome at least)
//			}

//			// Nothing else is done if one of these 2 is not true... other properties depend on having a bs-styled button
//			if (Kind != BsKind.None || AsAnchor) {
//				string btnStyleName = LinkStyle
//					? "link"
//					: Kind.Name();

//				if (btnStyleName.NotNulle()) {

//					// ONLY add size if is actually a bsStyle btn in the end...
//					string sizeClass = Size.NameButton();

//					string btnClass = OutlineStyle
//						? $"btn btn-outline-{btnStyleName} {sizeClass}"
//						: $"btn btn-{btnStyleName} {sizeClass}";

//					output.Attributes
//						.AddOrAppendClass(
//							btnClass,
//							checkForDuplicates: true,
//							dontAddIfPred: classVal => classVal.NotNulle() && classVal.Contains("btn"));
//				}

//				if (Disabled) {
//					if (isButton) {
//						output.Attributes.Add("disabled", "");
//					}
//					else if (AsAnchor) {
//						output.Attributes.AddOrAppendClass("disabled");
//						output.Attributes.Add("aria-disabled", "true");
//						output.Attributes.Add("tabindex", "-1");
//					}
//				}

//				if (IsBlock) {
//					output.Attributes.AddOrAppendClass("btn-block");
//				}
//			}
//		}
//	}
//}


///*
//		public override void Process(TagHelperContext context, TagHelperOutput output)
//		{
//			string btnType = context.AllAttributes["type"]?.Value?.ToString();

//			if (AsAnchor || output.TagName == null || btnType.IsNulle() || Href.NotNulle())
//				output.TagName = "a";

//			AsAnchor = output.TagName == "a";
//			bool isButton = !AsAnchor && output.TagName == "button";

//			if (AsAnchor && Kind == BsKind.None) // if is 'a' tag, has already custom settings, so we'll have a default bd kind *only* for such custom types
//				Kind = BsKind.Primary;

//			if (AsAnchor) {
//				string _href = Href.IsNulle()
//					? "javascript:void(0);"
//					: Href;
//				output.Attributes.SetAttribute("href", _href); // note: bootstrap doesn't style right without this, and, an empty href click causes the page to reload (in Chrome at least)
//			}

//			// Nothing else is done if one of these 2 is not true... other properties depend on having a bs-styled button
//			if (Kind != BsKind.None || AsAnchor) {
//				string btnStyleName = LinkStyle
//					? "link"
//					: Kind.Name();

//				if (btnStyleName.NotNulle()) {

//					// ONLY add size if is actually a bsStyle btn in the end...
//					string sizeClass = Size.NameButton();

//					string btnClass = OutlineStyle
//						? $"btn btn-outline-{btnStyleName} {sizeClass}"
//						: $"btn btn-{btnStyleName} {sizeClass}";

//					output.Attributes
//						.AddOrAppendClass(
//							btnClass,
//							checkForDuplicates: true,
//							dontAddIfPred: classVal => classVal.NotNulle() && classVal.Contains("btn"));
//				}

//				if (Disabled) {
//					if (isButton) {
//						output.Attributes.Add("disabled", "");
//					}
//					else if (AsAnchor) {
//						output.Attributes.AddOrAppendClass("disabled");
//						output.Attributes.Add("aria-disabled", "true");
//						output.Attributes.Add("tabindex", "-1");
//					}
//				}

//				if (IsBlock) {
//					output.Attributes.AddOrAppendClass("btn-block");
//				}
//			}
//		}
//	 */
