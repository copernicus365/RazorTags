// Alterations of original:
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags //Microsoft.AspNetCore.Mvc.TagHelpers
{
	/// <summary>
	/// See remarks.
	/// </summary>
	/// <remarks>
	/// Partial class, put big changes in this file as much as possible, while try to keep _src
	/// as virgin original as possible. The point is so _src can be replaced as easily
	/// as possible with framework changes, while keeping custom code here.
	///
	/// --- CHANGES ---
	/// 
	/// SOURCES:
	/// `InputTagHelper.cs`:   https://github.com/aspnet/AspNetCore/blob/90e89e970877a39cb048bb6f0e59551351f661c3/src/Mvc/Mvc.TagHelpers/src/InputTagHelper.cs
	/// `TemplateRenderer.cs`: https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/TemplateRenderer.cs
	/// OLD(archived) : https://github.com/aspnet/Mvc/tree/master/src/Microsoft.AspNetCore.Mvc.TagHelpers
	/// 
	/// --- namespace change: "Microsoft.AspNetCore.Mvc.TagHelpers" to "RazorTags"
	/// 
	/// --- private or internal to public members:
	/// Func: GetFormat,
	/// constants: ForAttributeName, FormatAttributeName
	/// 
	/// --- Type Load exceptions related ---
	/// 
	/// Example: "TypeLoadException: Could not load type 'Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.TemplateRenderer'
	/// from assembly 'Microsoft.AspNetCore.Mvc.ViewFeatures, Version=3.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'."
	/// 
	/// --:: GetInputTypeHints
	/// 
	/// `GetInputTypeHints()` calls `TemplateRenderer.GetTypeNames`, which was throwing exception
	/// THANKFULLY(WHEEWWW) didn't have to add a million things in the end (like it was looking at first:
	/// an endless dependency types chain) to get this, simple function below now: `GetTypeNames`, +
	/// const IEnumerableOfIFormFileName
	/// --:: GenerateTextBox
	/// GenerateTextBox: at one point checks: `FormatWeekHelper.GetFormattedWeek` but that was throwing:
	/// we just added the single static function without hitch(below)
	/// </remarks>
	public partial class InputTagHelperRT
	{
		public TagBuilder ProcessTagBuilder(TagHelperContext context, TagHelperOutput output)
		{
			if(context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			if(output == null) {
				throw new ArgumentNullException(nameof(output));
			}

			// Pass through attributes that are also well-known HTML attributes. Must be done prior to any copying
			// from a TagBuilder.

			// NP: Had to add && context.AllAttributes.ContainsName("type") because CopyHtmlAttribute expects value in context attributes
			// issue is that we already set the InputTypeName in the constructor (with our different usage). I THINK these values
			// are only set if THE USER sent in a value in the tags...
			if(InputTypeName != null && context.AllAttributes.ContainsName("type")) {
				output.CopyHtmlAttribute("type", context);
			}

			if(Value != null && context.AllAttributes.ContainsName(nameof(Value))) {
				output.CopyHtmlAttribute(nameof(Value), context);
			}

			// Note null or empty For.Name is allowed because TemplateInfo.HtmlFieldPrefix may be sufficient.
			// IHtmlGenerator will enforce name requirements.
			var metadata = For.Metadata;
			var modelExplorer = For.ModelExplorer;
			if(metadata == null) {
				ThrowExOnMetadataNull(ForAttributeName, For.Name);
			}

			string inputType;
			string inputTypeHint;
			if(string.IsNullOrEmpty(InputTypeName)) {
				// Note GetInputType never returns null.
				inputType = GetInputType(modelExplorer, out inputTypeHint);
			}
			else {
				inputType = InputTypeName.ToLowerInvariant();
				inputTypeHint = null;
			}

			// inputType may be more specific than default the generator chooses below.
			if(!output.Attributes.ContainsName("type")) {
				output.Attributes.SetAttribute("type", inputType);
			}

			TagBuilder tagBuilder;
			switch(inputType) {
				case "hidden":
					tagBuilder = GenerateHidden(modelExplorer, null);
					break;

				case "checkbox":
					tagBuilder = GenerateCheckBox(modelExplorer, output, null);
					break;

				case "password":
					tagBuilder = Generator.GeneratePassword(
						ViewContext,
						modelExplorer,
						For.Name,
						value: null,
						htmlAttributes: null);
					break;

				case "radio":
					tagBuilder = GenerateRadio(modelExplorer, null);
					break;

				default:
					tagBuilder = GenerateTextBox(modelExplorer, inputTypeHint, inputType, null);
					break;
			}
			return tagBuilder;
		}

		/// <inheritdoc />
		/// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown if <see cref="Format"/> is non-<c>null</c> but <see cref="For"/> is <c>null</c>.
		/// </exception>
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var tb = ProcessTagBuilder(context, output);
			SetTagBuilderToMainContent(tb, output);
		}

		public void SetTagBuilderToMainContent(TagBuilder tagBuilder, TagHelperOutput output)
		{
			if(tagBuilder != null) {
				// This TagBuilder contains the one <input/> element of interest.
				output.MergeAttributes(tagBuilder);
				if(tagBuilder.HasInnerHtml) {
					// Since this is not the "checkbox" special-case, no guarantee that output is a self-closing
					// element. A later tag helper targeting this element may change output.TagMode.
					output.Content.AppendHtml(tagBuilder.InnerHtml);
				}
			}
		}


		#region --- TemplateRenderer.GetTypeNames ---

		public const string IEnumerableOfIFormFileName = "IEnumerable`" + nameof(IFormFile);

		// from: https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/TemplateRenderer.cs

		public static IEnumerable<string> GetTypeNames(ModelMetadata modelMetadata, Type fieldType)
		{
			// Not returning type name here for IEnumerable<IFormFile> since we will be returning
			// a more specific name, IEnumerableOfIFormFileName.
			var fieldTypeInfo = fieldType.GetTypeInfo();

			if(typeof(IEnumerable<IFormFile>) != fieldType) {
				yield return fieldType.Name;
			}

			if(fieldType == typeof(string)) {
				// Nothing more to provide
				yield break;
			}
			else if(!modelMetadata.IsComplexType) {
				// IsEnum is false for the Enum class itself
				if(fieldTypeInfo.IsEnum) {
					// Same as fieldType.BaseType.Name in this case
					yield return "Enum";
				}
				else if(fieldType == typeof(DateTimeOffset)) {
					yield return "DateTime";
				}

				yield return "String";
				yield break;
			}
			else if(!fieldTypeInfo.IsInterface) {
				var type = fieldType;
				while(true) {
					type = type.GetTypeInfo().BaseType;
					if(type == null || type == typeof(object)) {
						break;
					}

					yield return type.Name;
				}
			}

			if(typeof(IEnumerable).IsAssignableFrom(fieldType)) {
				if(typeof(IEnumerable<IFormFile>).IsAssignableFrom(fieldType)) {
					yield return IEnumerableOfIFormFileName;

					// Specific name has already been returned, now return the generic name.
					if(typeof(IEnumerable<IFormFile>) == fieldType) {
						yield return fieldType.Name;
					}
				}

				yield return "Collection";
			}
			else if(typeof(IFormFile) != fieldType && typeof(IFormFile).IsAssignableFrom(fieldType)) {
				yield return nameof(IFormFile);
			}

			yield return "Object";
		}

		#endregion


		#region --- FormatWeekHelper.GetFormattedWeek ---

		// --- Microsoft.AspNetCore.Mvc.ViewFeatures.FormatWeekHelper (internal) ---
		// src: https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/FormatWeekHelper.cs

		public static string GetFormattedWeek(ModelExplorer modelExplorer)
		{
			var value = modelExplorer.Model;
			var metadata = modelExplorer.Metadata;

			if(value is DateTimeOffset dateTimeOffset) {
				value = dateTimeOffset.DateTime;
			}

			if(value is DateTime date) {
				var calendar = Thread.CurrentThread.CurrentCulture.Calendar;
				var day = calendar.GetDayOfWeek(date);

				// Get the week number consistent with ISO 8601. See blog post:
				// https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/
				if(day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) {
					date = date.AddDays(3);
				}

				var week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
				var year = calendar.GetYear(date);
				var month = calendar.GetMonth(date);

				// Last week (either 52 or 53) includes January dates (1st, 2nd, 3rd) 
				if(week >= 52 && month == 1) {
					year--;
				}

				// First week includes December dates (29th, 30th, 31st)
				if(week == 1 && month == 12) {
					year++;
				}

				return $"{year:0000}-W{week:00}";
			}

			return null;
		}

		#endregion

		#region --- Throw Exceptions (require Resources we don't have) ---

		public static void ThrowExOnMetadataNull(string forAttributeName, string forName)
		{
			throw new InvalidOperationException($"Metadata null: input - {forAttributeName} - {forName}");
			//throw new InvalidOperationException(Resources.FormatTagHelpers_NoProvidedMetadata(
			//	"<input>",
			//	ForAttributeName,
			//	nameof(IModelMetadataProvider),
			//	For.Name));
		}

		public static void ThrowExOnInvalidModelTypeShouldBeBool()
		{
			throw new InvalidOperationException($"Invalid model type, should be a boolean");
			//throw new InvalidOperationException(Resources.FormatInputTagHelper_InvalidExpressionResult(
			//	   "<input>",
			//	   ForAttributeName,
			//	   modelExplorer.ModelType.FullName,
			//	   typeof(bool).FullName,
			//	   typeof(string).FullName,
			//	   "type",
			//	   "checkbox"));
		}

		public static void ThrowExOnValueRequired()
		{
			throw new InvalidOperationException($"Value is required, must not be null for radio button.");
			//throw new InvalidOperationException(Resources.FormatInputTagHelper_ValueRequired(
			//	"<input>",
			//	nameof(Value).ToLowerInvariant(),
			//	"type",
			//	"radio"));
		}

		#endregion

	}
}

#region --- Orig `Process` (now calls `ProcessTagBuilder`) ---

///// <inheritdoc />
///// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
///// <exception cref="InvalidOperationException">
///// Thrown if <see cref="Format"/> is non-<c>null</c> but <see cref="For"/> is <c>null</c>.
///// </exception>
//public override void Process(TagHelperContext context, TagHelperOutput output)
//{
//	if(context == null) {
//		throw new ArgumentNullException(nameof(context));
//	}

//	if(output == null) {
//		throw new ArgumentNullException(nameof(output));
//	}

//	// Pass through attributes that are also well-known HTML attributes. Must be done prior to any copying
//	// from a TagBuilder.
//	if(InputTypeName != null) {
//		output.CopyHtmlAttribute("type", context);
//	}

//	if(Name != null) {
//		output.CopyHtmlAttribute(nameof(Name), context);
//	}

//	if(Value != null) {
//		output.CopyHtmlAttribute(nameof(Value), context);
//	}

//	// Note null or empty For.Name is allowed because TemplateInfo.HtmlFieldPrefix may be sufficient.
//	// IHtmlGenerator will enforce name requirements.
//	var metadata = For.Metadata;
//	var modelExplorer = For.ModelExplorer;
//	if(metadata == null) {
//		ThrowExOnMetadataNull(ForAttributeName, For.Name);
//	}

//	string inputType;
//	string inputTypeHint;
//	if(string.IsNullOrEmpty(InputTypeName)) {
//		// Note GetInputType never returns null.
//		inputType = GetInputType(modelExplorer, out inputTypeHint);
//	}
//	else {
//		inputType = InputTypeName.ToLowerInvariant();
//		inputTypeHint = null;
//	}

//	// inputType may be more specific than default the generator chooses below.
//	if(!output.Attributes.ContainsName("type")) {
//		output.Attributes.SetAttribute("type", inputType);
//	}

//	// Ensure Generator does not throw due to empty "fullName" if user provided a name attribute.
//	IDictionary<string, object> htmlAttributes = null;
//	if(string.IsNullOrEmpty(For.Name) &&
//		string.IsNullOrEmpty(ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix) &&
//		!string.IsNullOrEmpty(Name)) {
//		htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
//		{
//			{ "name", Name },
//		};
//	}

//	TagBuilder tagBuilder;
//	switch(inputType) {
//		case "hidden":
//			tagBuilder = GenerateHidden(modelExplorer, htmlAttributes);
//			break;

//		case "checkbox":
//			tagBuilder = GenerateCheckBox(modelExplorer, output, htmlAttributes);
//			break;

//		case "password":
//			tagBuilder = Generator.GeneratePassword(
//				ViewContext,
//				modelExplorer,
//				For.Name,
//				value: null,
//				htmlAttributes: htmlAttributes);
//			break;

//		case "radio":
//			tagBuilder = GenerateRadio(modelExplorer, htmlAttributes);
//			break;

//		default:
//			tagBuilder = GenerateTextBox(modelExplorer, inputTypeHint, inputType, htmlAttributes);
//			break;
//	}

//	if(tagBuilder != null) {
//		// This TagBuilder contains the one <input/> element of interest.
//		output.MergeAttributes(tagBuilder);
//		if(tagBuilder.HasInnerHtml) {
//			// Since this is not the "checkbox" special-case, no guarantee that output is a self-closing
//			// element. A later tag helper targeting this element may change output.TagMode.
//			output.Content.AppendHtml(tagBuilder.InnerHtml);
//		}
//	}
//}

#endregion
