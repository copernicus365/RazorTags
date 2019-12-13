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
	[HtmlTargetElement("password", TagStructure = TagStructure.WithoutEndTag)]
	public class PasswordTag : TextBoxTag
	{
		public PasswordTag(IHtmlGenerator generator)
			: base(generator)
		{
			InputTypeName = "password";
		}

		public override int Order => base.Order - 1;

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			InputTypeName = "password";
			base.Process(context, output);
		}
	}
}