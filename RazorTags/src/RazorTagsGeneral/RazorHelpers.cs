using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using DotNetXtensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	public static class RazorHelpers
	{
		/// <summary>
		/// Generates the string behind this <see cref="IHtmlContent"/> instance 
		/// (which might be a <see cref="TagBuilder"/>, among other things).
		/// Note that in many cases, the framework will directly take an 
		/// <see cref="IHtmlContent"/> instance, in which case that is probably more
		/// performant than getting the string this way, since this way requires generating
		/// a new <see cref="StringWriter"/> instance.
		/// </summary>
		public static string GetString(this IHtmlContent content, TextWriter writer = null, HtmlEncoder encoder = null)
		{
			if(writer == null)
				writer = new StringWriter();
			content.WriteTo(writer, encoder ?? HtmlEncoder.Default);
			return writer.ToString();
		}

		public static TagHelperOutput ToOutput(this TagHelper th, TagHelperAttributeList attributes = null)
		{
			if(attributes == null)
				attributes = new TagHelperAttributeList();
			var output = new TagHelperOutput(null, attributes, _dummy_getChildContentAsync);
			th.Process(null, output);
			return output;
		}

		public static TagHelperOutput ToOutput(this IHtmlHelper hh, TagHelper th)
			=> th?.ToOutput();

		public static IHtmlContent c<T>(this IHtmlHelper<T> hh, Func<object, HelperResult> f)
		{
			var hc = f?.Invoke(null);
			return hc;
		}

		static Func<bool, HtmlEncoder, Task<TagHelperContent>> _dummy_getChildContentAsync = (flag, encoder) => null;


		public static string HtmlEncode(this string str, bool undoApostropheEsc = true)
		{
			if (str.NotNulle()) {
				string val = WebUtility.HtmlEncode(str);
				if (undoApostropheEsc 
					&& val != str 
					&& val.NotNulle()) {
					if (val.IndexOf('&') >= 0) {
						val = val.Replace("&#39;", "'");
					}
				}
				return val;
			}
			return str;
		}

		public static TagHelperAttributeList AddOrAppendClass(
			this TagHelperAttributeList tagAttributesList,
			string classValue,
			bool appendFirst = false,
			bool checkForDuplicates = false,
			Func<string, bool> dontAddIfPred = null)
			=> tagAttributesList.AddOrAppendAttribute("class", classValue, appendFirst, checkForDuplicates, dontAddIfPred);

		public static TagHelperAttributeList AddOrAppendAttribute(
			this TagHelperAttributeList tagAttributesList,
			string name,
			string attrValue,
			bool appendFirst = false,
			bool checkForDuplicates = false,
			Func<string, bool> dontAddIfPred = null)
		{
			if (name.IsNulle()) throw new ArgumentNullException();
			if (tagAttributesList == null) throw new ArgumentNullException();

			if (attrValue == null)
				return tagAttributesList;

			TagHelperAttribute attr = tagAttributesList.IsNulle()
				? null
				: tagAttributesList.FirstN(t => t.Name == name);

			if (attr != null) {
				string attrValStr = attr.Value?.ToString();
				if (attrValStr.IsNulle())
					attr = null;
				else {
					if (dontAddIfPred != null && dontAddIfPred.Invoke(attrValStr))
						return tagAttributesList;

					attrValue = appendFirst
						? $"{attrValue} {attrValStr}"
						: $"{attrValStr} {attrValue}";
				}
			}

			attrValue = attrValue.TrimIfNeeded();

			if (checkForDuplicates && attr != null) {
				// so only have to do this check if attribute already exists, that single check will GREATLY improv perf, removing a needless case
				attrValue = attrValue
					.SplitAndRemoveWhiteSpaceEntries(_whiteSpSeparators)
					.Distinct()
					.JoinToString(" ");
			}

			tagAttributesList.SetAttribute(name, attrValue);

			return tagAttributesList;
		}


		public static TagBuilder AddDisabledAttribute(this TagBuilder tb, bool? disabled)
		{
			if (disabled == true && !tb.Attributes.ContainsKey("disabled"))
				tb.Attributes.Add("disabled", "true");
			return tb;
		}

		static char[] _whiteSpSeparators = { ' ', '\t' };

		public static TagHelperContent AppendHtmlIf(this TagHelperContent thc, bool condition, string val)
		{
			if (condition && thc != null)
				thc.AppendHtml(val);
			return thc;
		}

		public static TagHelperContent AppendHtmlIf(this TagHelperContent thc, bool condition, params string[] vals)
		{
			if (condition)
				thc.AppendHtml(vals);
			return thc;
		}
		public static TagHelperContent AppendHtml(this TagHelperContent thc, params string[] vals)
		{
			if (thc != null && vals != null) {
				for (int i = 0; i < vals.Length; i++) {
					string val = vals[i];
					if (val.NotNulle())
						thc.AppendHtml(vals[i]);
				}
			}
			return thc;
		}

	}
}