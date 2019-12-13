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
	[HtmlTargetElement("textarea-grp", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class TextareaTag : TextBoxTag
	{
		public TextareaTag(IHtmlGenerator generator)
			: base(generator)
		{
			InputTypeName = null;
		}

		public override int Order => base.Order - 1;

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "textarea";
			InputTypeName = null;
			base.Process(context, output);
		}
	}
}