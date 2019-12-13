using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	public abstract class TagHelperHtmlContent : TagHelper, IHtmlContent
	{
		public void WriteTo(TextWriter writer, HtmlEncoder encoder)
		{
			var attributes = new TagHelperAttributeList();
			var output = new TagHelperOutput(null, attributes, _dummy_getChildContentAsync);
			
			this.Process(null, output);

			output.WriteTo(writer, encoder);
		}

		static Func<bool, HtmlEncoder, Task<TagHelperContent>> _dummy_getChildContentAsync = (flag, encoder) => null;
	}
}
