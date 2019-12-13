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
	[HtmlTargetElement("checkbox", TagStructure = TagStructure.WithoutEndTag)]
	public class CheckboxTag : InputGroupTag
	{
		public CheckboxTag(IHtmlGenerator generator)
			: base(generator)
		{
			InputTypeName = "checkbox";
		}

		public override int Order => base.Order - 1;

		[HtmlAttributeName("group-label")]
		public string GroupLabel { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			InitFields();

			output.TagMode = TagMode.SelfClosing;
			output.TagName = "input";
			InputTypeName = "checkbox";

			InputModelInfo modelInfo = new InputModelInfo(Generator, For, Format, InputTypeName, Value, null, LabelText, ViewContext);

			TagBuilder cboxTb = modelInfo.ProcessTagBuilder(context, output);

			cboxTb.AddCssClass("custom-control-input");
			cboxTb.AddDisabledAttribute(Disabled);

			WriteStartGroupTag(output.PreElement, "checkbox"); //, formGroupClassName: "form-check");

			WriteLabel_PlusPrePost_IfAvailable(GroupLabel?.HtmlEncode(), modelInfo, output.PreElement);

			bool doNew = true;

			if(_isBS43 && doNew) {
				output.PreElement
					.AppendHtml("<div class=\"custom-control custom-checkbox\">")
					.AppendHtml("\r\n\t");

				output.MergeAttributes(cboxTb);

				output.PostElement
					//.AppendHtml(cboxTb)
					.AppendHtml("\r\n\t")
					.AppendHtml("<label class=\"custom-control-label\" for=\"")
					.AppendHtml(modelInfo.FullNameId)
					.AppendHtml("\">")
					.AppendHtml(modelInfo.LabelText)
					.AppendHtml("</label>\r\n</div>\r\n")
					;

				/*
<div class="custom-control custom-checkbox">
	<input type="checkbox" class="custom-control-input" id="customCheck1">
	<label class="custom-control-label" for="customCheck1">Check this custom checkbox</label>
</div>
*/
			}
			else {

				/*
<div class="form-check">
	<label class="custom-control custom-checkbox">
		<input type="checkbox" class="custom-control-input" data-val="true" data-val-required="The IsCool field is required." id="IsCool" name="IsCool" value="true">
		<span class="custom-control-indicator"></span>
		<span class="custom-control-description">Hey, are you cool??!!</span>
	</label>
</div>*/

				output.PreElement.AppendHtml($@"
	<div class=""form-check"">
		<label class=""custom-control custom-checkbox"">
	");

				output.MergeAttributes(cboxTb);

				output.PostElement.AppendHtml($@"
			<span class=""custom-control-indicator""></span>
			<span class=""custom-control-description"">{modelInfo.LabelText}</span>
		</label>
	</div>
");
			}
			WriteEndGroupTag(output.PostElement);
		}
	}
}