using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetXtensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	[HtmlTargetElement("textbox", TagStructure = TagStructure.WithoutEndTag)]
	public class TextBoxTag : InputGroupTag
	{
		public TextBoxTag(IHtmlGenerator generator) : base(generator) { }

		public override int Order => base.Order + 1;

		/// <summary>
		/// `textarea` rows.
		/// </summary>
		public int Rows { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			InitFields();
			bool isTextArea = output.TagName == "textarea";
			if (!isTextArea) {
				// https://getbootstrap.com/docs/4.0/components/forms/
				output.TagName = "input";
				output.TagMode = TagMode.SelfClosing;
			}

			InputModelInfo modelInfo = new InputModelInfo(Generator, For, Format, InputTypeName, Value, Placeholder, LabelText, ViewContext);

			TagBuilder inputTbx = isTextArea
				? modelInfo.GetTextarea(Rows, 0)
				: modelInfo.ProcessTagBuilder(context, output);

			modelInfo.SetExtraTextInputValues(inputTbx, isTextArea, Size, Disabled);
			
			output.MergeAttributes(inputTbx);

			TagBuilder validMsgTag = modelInfo.GetValidationMessageTag(ValidationMessage.NullIfEmptyTrimmed());


			WriteStartGroupTag(output.PreElement, "textbox");

			WriteLabel_PlusPrePost_IfAvailable(GetFinalLabelText(modelInfo), modelInfo, output.PreElement); 

			output.PostElement.AppendHtml(validMsgTag);

			WriteEndGroupTag(output.PostElement);
		}
	}
}