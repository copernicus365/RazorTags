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
using Microsoft.AspNetCore.Html;

namespace RazorTags
{
	[HtmlTargetElement("radiobuttons", TagStructure = TagStructure.WithoutEndTag)]
	public class RadioButtonsTag : InputGroupTag
	{
		public RadioButtonsTag(IHtmlGenerator generator)
			: base(generator)
		{
			InputTypeName = "radio";
			Inline = true; // set as default for radiobuttons...
		}

		[HtmlAttributeName("items")]
		public IEnumerable<InputColItem> Items { get; set; }

		/// <summary>
		/// If set, this will replace the list of enum-names. Must be of equal length
		/// as the collection. 
		/// </summary>
		[HtmlAttributeName("enum-names")]
		public IEnumerable<string> EnumNames { get; set; }

		/// <summary>
		/// True to use the bootstrap 4 custom radio buttons. True by default.
		/// </summary>
		[HtmlAttributeName("custom")]
		public bool Custom { get; set; } = true;

		public override int Order => base.Order - 1;

		[HtmlAttributeName("bool-labels")]
		public (string trueName, string falseName, string notSetName) BoolLabels { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			InitFields();

			InputModelInfo modelInfo = new InputModelInfo(Generator, For, Format, InputTypeName, Value, null, LabelText, ViewContext);

			/* --- Long note about nullable bool or nullable enum for radio buttons ---
			 I've seen some people claim: "Radio buttons, by definition, do not have a null value."
			 that seems to be true. But unlike the select list, the first item is NOT selected by default,
			 just NO radio button is selected, whereas select list needs a deliberately sent in option-label
			 for the first item to not be selected. Further, on POST, confirmed that if they never clicked
			 one of the radio buttons, that the form posts that value as null (as you wanted, the only point 
			 of this note is for a nullable bool or nullable enum, otherwise of course one should be selected, 
			 unless you were tampering with values in debug). SO: No, we don't want to winnow out values here,
			 nor, and here's an important point, would we want the enum values generator (the underlying one,
			 not just the call below that handles things above it) to automatically add
			 an extra value for nullable, as we thought of doing. Rather, that should be only done for select list,
			 */
			bool winnowNullSelectOption = false;

			(string optionLabel, InputColItem[] kvs) = modelInfo.GetInputCollectionItems(
				winnowNullSelectOption,
				null,
				BoolNames.GetBoolNames(BoolLabels.trueName, BoolLabels.falseName, BoolLabels.notSetName),
				Items,
				EnumNames);

			if(kvs.IsNulle()) {
				output.SuppressOutput();
				return;
			}

			bool customRadios = Custom;

			var arr = GetRadioButtons(kvs, modelInfo, customRadios);

			if(arr.IsNulle()) {
				output.SuppressOutput();
				return;
			}
			output.TagMode = TagMode.SelfClosing;
			output.TagName = null;
			// setting to null seems to work to simply ignore the main Content (output.Content)
			// which is what we want, everything written to pre and post elements (ELEMENTS!),
			// as we don't have a single input (like is true with textbox), around
			// which everything else can be constructed (in fact, textbox is a pain, because the framework
			// works on output value to get other values...)

			WriteStartGroupTag(output.PreElement, "radios"); //, formGroupClassName: "form-check");

			WriteLabel_PlusPrePost_IfAvailable(GetFinalLabelText(modelInfo), modelInfo, output.PreElement);

			//if (!NoGroup && !NoLabel && modelInfo.LabelText.NotNulle()) { // `!NoGroup`: consider this outer label as part of the form-group!
			//	output.PreElement.AppendHtml($"<label>{modelInfo.LabelText}</label>");
			//}

			for(int i = 0; i < arr.Length; i++) {
				(TagBuilder rdBtn, string rdLabelTxt, string id) = arr[i];

				if(rdBtn == null)
					continue;

				if(_isBS43) {
					if(!customRadios) throw new NotImplementedException();
					output.PreElement
						.AppendHtml("<div class=\"custom-control custom-radio")
						.AppendHtmlIf(Inline, " custom-control-inline")
						.AppendHtml("\">\r\n  ")
						.AppendHtml(rdBtn)
						.AppendHtml("\r\n  ")
						.AppendHtml("<label class=\"custom-control-label\" for=\"") //customRadio1\">Toggle this custom radio</label>")
						.AppendHtml(id)
						.AppendHtml("\">")
						.AppendHtml(rdLabelTxt ?? "")
						.AppendHtml("</label>\r\n</div>\r\n")
						;

					/*
<div class="custom-control custom-radio">
<input type="radio" id="customRadio1" name="customRadio" class="custom-control-input">
<label class="custom-control-label" for="customRadio1">Toggle this custom radio</label>
</div>					*/
				}
				else {

					output.PreElement.AppendHtml($@"	<div class=""form-check{(Inline ? " form-check-inline" : null)}"">
		<label class=""{(!customRadios ? "form-check-label" : "custom-control custom-radio")}"">
			");
					output.PreElement.AppendHtml(rdBtn);

					if(!customRadios) {
						output.PreElement.AppendHtml(rdLabelTxt);
					}
					else {
						output.PreElement.AppendHtml($@"		
		<span class=""custom-control-indicator""></span>
		<span class=""custom-control-description"">{rdLabelTxt}</span>");
					}
					output.PreElement.AppendHtml(@"
		</label>
	</div>
");
				}
			}

			WriteEndGroupTag(output.PostElement);
		}

		public static (TagBuilder rdBtn, string label, string id)[] GetRadioButtons(
			InputColItem[] items,
			InputModelInfo modelInfo,
			bool customRadios)
		{
			var radioButtonsList = new List<(TagBuilder rdBtn, string label, string id)>();

			for(int i = 0; i < items.Length; i++) {
				InputColItem itm = items[i];
				if(itm == null)
					continue;
				TagBuilder radioTag = modelInfo.GetRadioButton(itm.Value, itm.IsSelected, itm.Id);
				//, id == null ? null : new { id = id });
				radioTag.AddCssClass(!customRadios ? "form-check-input" : "custom-control-input");
				radioButtonsList.Add((radioTag, itm.Name, itm.Id));
			}
			return radioButtonsList.ToArray();
		}

	}
}
