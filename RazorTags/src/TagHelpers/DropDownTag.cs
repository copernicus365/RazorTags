using System;
using System.Collections.Generic;
using System.Linq;
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

namespace RazorTags.Bootstrap
{
	[HtmlTargetElement("dropdown", TagStructure = TagStructure.WithoutEndTag)]
	public class DropDownTag : InputGroupTag
	{
		public DropDownTag(IHtmlGenerator generator)
			: base(generator)
		{
			InputTypeName = "radio";
		}

		[HtmlAttributeName("items")]
		public IEnumerable<InputColItem> Items { get; set; }

		[HtmlAttributeName("multiple")]
		public bool Multiple { get; set; }

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

		[HtmlAttributeName("option-label")]
		public string OptionLabel { get; set; }

		public override int Order => base.Order - 1;

		[HtmlAttributeName("bool-labels")]
		public (string trueName, string falseName, string notSetName) BoolLabels { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			InitFields();

			InputModelInfo modelInfo = new InputModelInfo(Generator, For, Format, InputTypeName, Value, null, LabelText, ViewContext);

			(string optionLabel, InputColItem[] kvs) = modelInfo.GetInputCollectionItems(
				true,
				OptionLabel?.HtmlEncode(),
				BoolNames.GetBoolNames(BoolLabels.trueName, BoolLabels.falseName, BoolLabels.notSetName),
				Items,
				EnumNames);

			if (kvs.IsNulle()) {
				output.SuppressOutput();
				return;
			}

			var selTb = modelInfo.GetSelect(optionLabel, Multiple, kvs);

			if (selTb == null) {
				output.SuppressOutput();
				return;
			}

			selTb.AddCssClass(Custom ? "custom-select" : "form-control");
			output.TagName = null;

			WriteStartGroupTag(output.PreElement, "dropdown");
			
			WriteLabel_PlusPrePost_IfAvailable(GetFinalLabelText(modelInfo), modelInfo, output.PreElement);

			output.PostElement.AppendHtml(selTb);

			WriteEndGroupTag(output.PostElement);
		}
	}
}
