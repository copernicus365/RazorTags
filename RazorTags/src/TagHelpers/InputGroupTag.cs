using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DotNetXtensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	public abstract class InputGroupTag : TagHelper
	{
		protected static bool _isBS43 = true; // https://getbootstrap.com/docs/4.3/components/forms/#custom-forms

		public InputGroupTag(IHtmlGenerator generator)
		{
			Generator = generator;
		}

		protected IHtmlGenerator Generator { get; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName("for")]
		public ModelExpression For { get; set; }

		/// <summary>
		/// The format string (see https://msdn.microsoft.com/en-us/library/txafckwd.aspx) used to format the
		/// <see cref="For"/> result. Sets the generated "value" attribute to that formatted string.
		/// </summary>
		/// <remarks>
		/// Not used if the provided (see <see cref="InputTypeName"/>) or calculated "type" attribute value is
		/// <c>checkbox</c>, <c>password</c>, or <c>radio</c>. That is, <see cref="Format"/> is used when calling
		/// <see cref="IHtmlGenerator.GenerateTextBox"/>.
		/// </remarks>
		[HtmlAttributeName(InputTagHelperRT.FormatAttributeName)]
		public string Format { get; set; }

		/// <summary>
		/// The type of the &lt;input&gt; element.
		/// </summary>
		/// <remarks>
		/// Passed through to the generated HTML in all cases. Also used to determine the <see cref="IHtmlGenerator"/>
		/// helper to call and the default <see cref="Format"/> value. A default <see cref="Format"/> is not calculated
		/// if the provided (see <see cref="InputTypeName"/>) or calculated "type" attribute value is <c>checkbox</c>,
		/// <c>hidden</c>, <c>password</c>, or <c>radio</c>.
		/// </remarks>
		[HtmlAttributeName("type")]
		public string InputTypeName { get; set; }

		/// <summary>
		/// True to not include the outer form-group.
		/// </summary>
		[HtmlAttributeName("nogroup")]
		public bool NoGroup { get; set; }

		#region --- Label Properties ---

		[HtmlAttributeName("label")]
		public string LabelText { get; set; }

		public string GetFinalLabelText(InputModelInfo modelInfo)
			=> LabelText.FirstNotNulle(modelInfo.LabelText);

		[HtmlAttributeName("no-label")]
		public bool NoLabel { get; set; }

		// The 4 fields below condense to one of these 2 objects,
		// later we will detect the type (string or IHtmlContent), 
		// this way only 2 final fields instead of 4 or more
		protected object _preLabel;
		protected object _postLabel;

		[HtmlAttributeName("pre-label-html")]
		public IHtmlContent PreLabelHtml {
			get => null;
			set => _preLabel = value;
		}

		[HtmlAttributeName("post-label-html")]
		public IHtmlContent PostLabelHtml {
			get => null;
			set => _postLabel = value;
		}

		[HtmlAttributeName("pre-label-tooltip-icon")]
		public string PreLabelTooltipIcon {
			get => null;
			set => _preLabel = value; // we'll set it as a string, at write time, if detects string, interpret as tooltip-icon message
		}

		[HtmlAttributeName("post-label-tooltip-icon")]
		public string PostLabelTooltipIcon {
			get => null;
			set => _postLabel = value;
		}

		#endregion

		public bool? Disabled { get; set; }

		/// <summary>
		/// If set, this will override the default generated validation message.
		/// </summary>
		[HtmlAttributeName("valid-message")]
		public string ValidationMessage { get; set; }

		[HtmlAttributeName("placeholder")]
		public string Placeholder { get; set; }

		[HtmlAttributeName("group-css")]
		public string GroupCss { get; set; }

		[HtmlAttributeName("inline")]
		public bool Inline { get; set; }

		/// <summary>
		/// The value of the &lt;input&gt; element.
		/// </summary>
		/// <remarks>
		/// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute
		/// if <see cref="InputTypeName"/> is "radio". Must not be <c>null</c> in that case.
		/// </remarks>
		public string Value { get; set; }

		/// <summary>
		/// Set this to specify the input size. Only applicable on some input types, 
		/// such as textboxes and dropdowns (text / select).
		/// </summary>
		[HtmlAttributeName("size")]
		public BsSize Size { get; set; }

		/// <summary>
		/// Call this before or at the start of processing tag. 
		/// Currently only does one thing, html escapes the input <see cref="LabelText"/>
		/// if it is NotNulle, but other things can be done here, bettter than each
		/// <see cref="InputGroupTag"/> child having to do and remember these tasks every time.
		/// </summary>
		public void InitFields()
		{
			if (LabelText.NotNulle())
				LabelText = LabelText.HtmlEncode();
		}

		public void WriteStartGroupTag(TagHelperContent content, string extraClass) //, string formGroupClassName = "form-group")
		{
			if (!NoGroup) {
				const string formGroupClassName = "form-group";
				content
					.AppendHtml(@"<div class=""", formGroupClassName)
					.AppendHtmlIf(GroupCss.NotNulle(), " ", GroupCss)
					.AppendHtmlIf(Inline, " inline") // formGroupClassName == "form-check" ? null : " inline") 
													 // if "form-check" - for now we'll apply the bs4 way to every single individual form-check group child within, 
													 // though this way would have been a lot prettier and all
					.AppendHtmlIf(extraClass.NotNulle(), " ", extraClass)
					.AppendHtmlLine(@""">");
			}
		}

		public void WriteEndGroupTag(TagHelperContent content)
		{
			if (!NoGroup) {
				content
					.AppendHtml(@"</div>");
			}
		}

		/// <summary>
		/// Writes a label using <paramref name="labelTxt"/> (uses the input argument ONLY
		/// for this) and writes within the label any pre and post label fields.
		/// Writes nothing and returns if <see cref="NoLabel"/> or <see cref="NoGroup"/> are true,
		/// or if <paramref name="labelTxt"/> is null (with no pre-post), if so returns. 
		/// If <paramref name="labelTxt"/> is EMPTY we replace it with a single space, 
		/// because the framework label generator returns null if the label text is null or empty.
		/// </summary>
		/// <param name="labelTxt">
		/// Label text. We do NOT read other properties / fields to get this,
		/// but require you to send it in, this is the cleaner option, because for instance checkbox group
		/// doesn't use the main <see cref="LabelText"/> fields. Call <see cref="GetFinalLabelText(InputModelInfo)"/> 
		/// for this purposes. The caller is also responsibile for html-encoding this value (or not).
		/// </param>
		/// <param name="modelInfo"></param>
		/// <param name="contentToAppendTo">The tag content to AppendHtml the final label (if any) to.
		/// For most this will be <see cref="TagHelperOutput.PreElement"/> (is all callers currently in fact).</param>
		public void WriteLabel_PlusPrePost_IfAvailable(string labelTxt, InputModelInfo modelInfo, TagHelperContent contentToAppendTo)
		{
			bool hasExtras = _preLabel != null || _postLabel != null;

			if (NoLabel || NoGroup || (!hasExtras && labelTxt == null))
				return;
			
			if (labelTxt == "") // -- caller of this method MUST send in NULL for this to be ignored. Proble
				labelTxt = " ";

			object d = null;
			bool setLabelClass = false;
			if(setLabelClass)
				d = vvv;

			TagBuilder labelTag = modelInfo.GetLabelTag(_preLabel != null ? " " : labelTxt, d); // " " - Nulle will return null for label

			if (labelTag == null)
				return;

			if (hasExtras) {
				IHtmlContentBuilder bld = labelTag.InnerHtml;

				if (_preLabel != null) {
					bld.Clear(); // clear the input label text, we'll readd when needed

					_writeLabelPrePostContent(_preLabel, bld);

					bld.AppendHtml(" ");
					// if so, we had to clear the label content to add precontent, now readd the label text itself
					bld.AppendHtml(labelTxt);
				}

				if (_postLabel != null) {
					bld.AppendHtml(" ");
					_writeLabelPrePostContent(_postLabel, bld);
				}
			}

			contentToAppendTo
			//output.PreElement
				.AppendHtml(labelTag); // THANKFULLY, the framework allowed null, no ex (sheesh)
		}

		static object vvv = new { @class = "coolbeans" };

		IHtmlContent _writeLabelPrePostContent(object prePostLabel, IHtmlContentBuilder builder)
		{
			IHtmlContent htmlContent = prePostLabel as IHtmlContent;
			if (htmlContent == null) {
				string ttipMsg = prePostLabel as string;
				if (ttipMsg.NotNulle()) {
					var tt = new TooltipIconTag(ttipMsg) { IsHtml = true };
					htmlContent = tt;
				}
			}
			if (htmlContent != null) {
				builder.AppendHtml(htmlContent);
			}
			return htmlContent;
		}

	}
}