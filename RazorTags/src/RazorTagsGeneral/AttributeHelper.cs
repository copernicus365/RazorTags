using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
//using System.Web;
using DotNetXtensions;

namespace RazorTags
{
	/// <summary>
	/// Could call this: CssClassHelper, as 'class' attribute
	/// is the main one we have a problem with.
	/// </summary>
	public class AttributeHelper
	{
		static readonly char[] spaceSeparator = { ' ' };
		public const string clss = "class";
		const int NotFound = -1;

		public static bool MultiAttributeHasValue(string value, string attributes)
			=> IndexOfMultiAttribute(value, attributes) >= 0;

		public static int IndexOfMultiAttribute(string value, string attributes)
		{
			if (value == null || attributes == null)
				return NotFound;

			int valLen = value.Length;
			int allLen = attributes.Length;

			if (valLen == 0 || allLen == 0 || valLen > allLen)
				return NotFound;

			int idx = attributes.IndexOf(value);
			if (idx < 0)
				return NotFound;

			int lastStartIdx = allLen - valLen;
			
			while (idx >= 0 && idx <= lastStartIdx) {
				
				// every pass within here means we HAVE a match, problem is,
				// is it with whitespace on either side, and we don't want a slow regex,
				// treat this as hotspot, so give simplest possible and most blazing speed

				if ((idx == 0 || attributes[idx - 1] == ' ') && (idx == lastStartIdx || attributes[idx + valLen] == ' '))
					return idx;

				idx = attributes.IndexOf(value, idx + 1);
			}
			return NotFound;
		}

		/// <summary>
		/// ALERT! ALERT! HOTSPOT potential warning. 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="attributes"></param>
		/// <param name="append"></param>
		/// <param name="checkForDuplicates"></param>
		public static string AddToMultiAttribute(string value, string attributes, bool append = true, bool checkForDuplicates = false)
		{
			if (!append || attributes.IsNulle())
				return value ?? "";
			
			if (value.IsNulle() || (checkForDuplicates && IndexOfMultiAttribute(value, attributes) >= 0))
				return attributes;
			
			return attributes + " " + value;
		}

		public static string AddToMultiAttribute(string attributes, bool append, bool checkForDuplicates, params string[] addValues)
		{
			if (addValues == null || addValues.Length == 0)
				return attributes;

			if (!append || attributes.IsNulle())
				return string.Join(" ", addValues);

			if (!checkForDuplicates)
				return string.Join(" ", addValues.Concat(attributes, itemFirst: true));

			// keep null until needed, this is very important, allows less expensive usage
			// in cases where no new attributes get saved
			StringBuilder sb = null; 

			for (int i = 0; i < addValues.Length; i++) {
				string value = addValues[i];
				if (value.IsNulle())
					continue;

				if (!MultiAttributeHasValue(value, attributes)) {
					if (sb == null) {
						sb = new StringBuilder(addValues.Sum(s => s.Length + 1) + attributes.Length + 3);
						sb.Append(attributes);
					}
					sb.Append(" ");
					sb.Append(value);
				}
			}
			return sb == null ? attributes : sb.ToString();
		}

		public static string[] SplitMultiAttributes(string attributes)
		{
			if (attributes == null || attributes.Length == 0)
				return null;

			string[] vals = attributes.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
			if (vals == null || vals.Length == 0)
				return null;
			return vals;
		}

		public static string DeleteFromMultiAttribute(string value, string values)
		{
			if(value.IsNulle())
				return values;

			int foundIdx = IndexOfMultiAttribute(value, values);
			if (foundIdx < 0)
				return values;

			return values.Remove(foundIdx, value.Length);
		}


		// IDictionary

		public static void AddToClass<TValue>(IDictionary<string, TValue> dict, string value, bool append = true, bool checkForDuplicates = false) where TValue : class
		{
			AddToMultiAttribute(dict, clss, value, append, checkForDuplicates);
		}

		public static void AddToMultiAttribute<TValue>(IDictionary<string, TValue> dict, string key, string value, bool append = true, bool checkForDuplicates = false) where TValue : class
		{
			dict[key] = AddToMultiAttribute(value, AttributeValue(dict, key), append, checkForDuplicates) as TValue;
		}

		public static string AttributeValue<TValue>(IDictionary<string, TValue> dict, string key)
		{
			if (dict == null) throw new ArgumentNullException();
			if (dict.Count == 0)
				return null;
			if (!dict.TryGetValue(key, out TValue val))
				return null;
			return val as string;
		}

		//public static IDictionary<string, object> ObjectToHtmlAttributesDictionary(object htmlAttributes)
		//{
		//	if (htmlAttributes == null)
		//		return null;

		//	IDictionary<string, object> dictionary = htmlAttributes as IDictionary<string, object>;
		//	if (dictionary == null)
		//		dictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

		//	return dictionary;
		//}

		public static IDictionary<string, object> MergeAttributes(IDictionary<string, object> rootDict, IDictionary<string, object> dict2)
		{
			if (rootDict == null) throw new ArgumentNullException();
			if (dict2 == null || dict2.Count == 0)
				return rootDict;
			
			foreach (var kv in dict2) {
				string val = kv.Value as string;
				if (val.NotNulle() && kv.Key.Equals(clss, StringComparison.OrdinalIgnoreCase)) {
					if (rootDict.TryGetValue(clss, out object _classValObj) && _classValObj != null) {
						string classVal = _classValObj as string;
						if (!classVal.IsNulle()) {
							string[] vals = AttributeHelper.SplitMultiAttributes(val);
							rootDict[clss] = AttributeHelper.AddToMultiAttribute(classVal, true, true, vals);
						}
					}
				}
				else
					rootDict[kv.Key] = val;
			}
			return rootDict;
		}


		public static string AttributesToString<TKey, TValue>(IDictionary<TKey, TValue> attributes)
		{
			if (attributes == null || attributes.Count == 0)
				return "";

			StringBuilder sb = new StringBuilder(160);
			foreach (KeyValuePair<TKey, TValue> pair in attributes) {
				sb.Append(pair.Key.ToString());
				sb.Append("=\"");
				sb.Append(pair.Value.ToString());
				sb.Append("\" "); // add whitespace, final attribute space will be excluded below
			}
			if (sb.Length == 0)
				return "";
			return sb.ToString(0, sb.Length - 1); // cut off the final extra white space
		}

		/// <summary>
		/// Copied this with Reflector from TagBuilder.AppendAttributes, 
		/// which unfortunatley is a private method, and we needed it for efficiency's sake
		/// within HtmlWidget.
		/// </summary>
		public static string GetAttributesString(IDictionary<string, object> m_Tags)
		{
			if (m_Tags == null || m_Tags.Count < 1)
				return null;

			StringBuilder sb = new StringBuilder(128);
			foreach (KeyValuePair<string, object> pair in m_Tags) {
				string key = pair.Key;
				string value = pair.Value.ToString();

				if (key != "id" || value.NotNulle()) {
					string attr = WebUtility.HtmlEncode(value); // System.Web.HttpUtility.HtmlAttributeEncode(value);
					if (sb.Length > 0)
						sb.Append(' ');
					sb.Append(key).Append("=\"").Append(attr).Append('"');
				}
			}
			string result = sb.ToString();
			if (result.IsNullOrWhiteSpace())
				return null;
			return result;
		}

	}
}
