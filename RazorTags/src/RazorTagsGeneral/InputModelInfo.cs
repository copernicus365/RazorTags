using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using DotNetXtensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	/// <summary>
	/// Holds extensive metadata and useful functions for input types.
	/// This is the backbone of much of RazorTags for input group types.
	/// Typically you will generate an instance of this class in the Process
	/// method of a TagHelper, and use it from there to help generate the rest
	/// of your control (if it has input or otherwise form type fields).
	/// </summary>
	public class InputModelInfo : InputTagHelperRT
	{
		// ValidationMessageTagHelper.cs: https://github.com/aspnet/Mvc/blob/7e4a8fe479e3573173443a28dca089e2148ddb6d/src/Microsoft.AspNetCore.Mvc.TagHelpers/ValidationMessageTagHelper.cs

		public string InputTypeHint { get; set; }

		public string FullName { get; set; }

		//public string Name { get; set; }

		public string FullNameId { get; set; }

		public ModelExplorer ModelExplorer => For?.ModelExplorer;

		public object ModelValue => this.ModelExplorer?.Model;

		public Type ModelType { get; set; }

		public Type NullableBaseType { get; set; }

		public Type MainType => NullableBaseType ?? ModelType;

		public bool ModelTypeIsNullable => NullableBaseType != null;

		public bool TypeAllowsStringBasedEqualityComparison { get; set; }

		public ModelMetadata Meta => For?.Metadata;

		public string PropertyName { get; set; }

		public string LabelText { get; set; }

		public bool SetPlaceholder { get; set; } = true;

		public string Placeholder { get; set; }

		public string HtmlFieldPrefix { get; set; }

		public InputModelInfo(
			IHtmlGenerator generator,
			ModelExpression forModelExp,
			string format,
			string inputTypeName, // <- [HtmlAttributeName("type")]
			string value,
			string placeholder,
			string labelText,
			ViewContext viewContext)
			: base(generator)
		{
			base.For = forModelExp;
			base.Format = format;
			base.InputTypeName = inputTypeName;
			base.Value = value;
			base.ViewContext = viewContext;
			HtmlFieldPrefix = ViewContext?.ViewData?.TemplateInfo?.HtmlFieldPrefix;

			Name = For.Name;
			FullName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(Name);
			PropertyName = Meta.PropertyName;
			FullNameId = TagBuilder.CreateSanitizedId(FullName, "_");

			if (placeholder.NotNulle())
				Placeholder = placeholder.HtmlEncode(undoApostropheEsc: true); // important for placeholder

			LabelText = labelText.NotNulle() && labelText.IsNullOrWhiteSpace()
				? "&#160;"
				: labelText.FirstNotNulle(GetLabelText()).HtmlEncode();

			// THIS allows them to send in " " (space) if they want no label, but we can't treat empty as that, 
			// because e.g. label="" sends in empty not null, but what about label="@Model.Prop", want to allow 
			// a default to fall back if default is not set

			if (InputTypeName.IsNulle()) {
				InputTypeName = GetInputType(For.ModelExplorer, out string inputTypeHint);
				// Note GetInputType never returns null. 
				// Nick's note: If none defaults to "text", so do NOT set that default yourself anywhere else, this will handle
				InputTypeHint = inputTypeHint;
			}
			else {
				InputTypeName = InputTypeName.ToLowerInvariant();
				InputTypeHint = null;
			}

			if (For.Metadata == null)
				ThrowExOnMetadataNull(ForAttributeName, Name);

			if (Format.IsNulle())
				Format = GetFormat(For.ModelExplorer, InputTypeHint, InputTypeName);

			Type t = ModelType = this.ModelExplorer?.ModelType;
			if (t != null) {
				if (!t.IsPrimitive)
					NullableBaseType = t.GetUnderlyingTypeIfNullable();

				t = MainType;
				TypeAllowsStringBasedEqualityComparison =
					t.IsPrimitive || t.IsEnum || ExtraPrimitiveTypesAllowingStringBasedEqualityChecks.ContainsKey(t);
			}
		}



		public static Dictionary<Type, bool> ExtraPrimitiveTypesAllowingStringBasedEqualityChecks = new Dictionary<Type, bool>() {
			{ typeof(DateTime), false },
			{ typeof(DateTimeOffset), false },
			{ typeof(decimal), false },
			{ typeof(Enum), false },
			{ typeof(Guid), false },
			{ typeof(string), false },
			{ typeof(TimeSpan), false },
		};

		public static bool AllowStringBasedEqualityComparison(Type t, bool getUnderlyingTypesForNullables = true)
		{
			//Nullable.GetUnderlyingType(t);
			if (t == null)
				return false;

			if (t.IsPrimitive)
				return true;

			if (t.IsEnum)
				return true;

			if (getUnderlyingTypesForNullables) {
				var underlyTyp = t.GetUnderlyingTypeIfNullable();
				if (underlyTyp != null)
					return AllowStringBasedEqualityComparison(underlyTyp);
			}

			if (ExtraPrimitiveTypesAllowingStringBasedEqualityChecks.ContainsKey(t))
				return true;

			return false;
		}

		public static bool ObjectIsEqual(object modelValue, string modelValueStr, object val, bool countNullAsIsCheckedMatch, bool allowStringEquality)
		{
			if (val == null || modelValue == null) {
				if (countNullAsIsCheckedMatch && val == null && modelValue == null)
					return true;
			}
			else {
				if (modelValue.Equals(val))
					return true;

				if (allowStringEquality) {

					if (modelValueStr == null)
						modelValueStr = modelValue.ToString();

					string valStr = val?.ToString();
					if (valStr == modelValueStr)
						return true;
				}
			}
			return false;
		} //

		public static BoolNames DefaultBoolNames { get; set; } = new BoolNames("Yes", "No", "(Not Set)");

		public (string optionLabel, InputColItem[]) GetInputCollectionItems(
			bool winnowNullSelectOption,
			string optionLabel = null,
			BoolNames boolNames = null,
			IEnumerable<InputColItem> items = null,
			IEnumerable<string> enumNames = null,
			object selectedValueForModelCollection = null)
		{
			InputModelInfo modelInfo = this;
			if (items != null) {
				InputColItem[] _items = items.ToArray();
				if (_items.NotNulle()) {
					if (!_items.Any(kv => kv.IsSelected)) {
						throw new NotImplementedException();
						// RUN to find first selected, if any...
					}
					return (optionLabel, _items);
				}
			}

			Type mainTyp = modelInfo.MainType;
			bool typeIsNullable = modelInfo.ModelTypeIsNullable;

			bool isBool = mainTyp.IsPrimitive && mainTyp == typeof(bool);
			bool isEnum = !isBool && mainTyp.IsEnum;

			if (isBool) {
				bool? modelValBool = (bool?)ModelValue;

				BoolNames bn = boolNames ?? DefaultBoolNames;
				if (boolNames != null)
					bn.HtmlEncodeValues().Merge(DefaultBoolNames);

				var nameValues = new InputColItem[] {
					new InputColItem(bn.TrueName, true, modelValBool == true),
					new InputColItem(bn.FalseName, false, modelValBool == false),
					modelInfo.ModelTypeIsNullable
						? new InputColItem(bn.NotSetName, null, modelValBool == null)
						: null,
				};
				var _items = _alterCollForSelectOption(nameValues, winnowNullSelectOption, ref optionLabel);
				return (optionLabel, _items); // see note below, can't call with ref setter in return line
			}
			else if (isEnum) {
				InputColItem[] _items = GetInputEnumValues(enumNames, values: null, countNullAsIsChecked: true);
				_items = _alterCollForSelectOption(_items, winnowNullSelectOption, ref optionLabel); 
				// interesting! - the ref set won't set if called within the return line! so must put this line above
				return (optionLabel, _items);
			}
			else {
				object modelValue = modelInfo.ModelValue;
				if (modelValue != null) {

					if (modelValue is IEnumerable<object> arr) {
						List<InputColItem> kvlist = new List<InputColItem>();

						bool _allowStringEquality = selectedValueForModelCollection != null
							&& AllowStringBasedEqualityComparison(selectedValueForModelCollection.GetType());

						foreach (object value in arr) {
							if (value != null) {
								string valueName = value.ToString();
								if (valueName.NotNulle()) {
									bool isSel = selectedValueForModelCollection == null
										? false
										: ObjectIsEqual(selectedValueForModelCollection, valueName, value, countNullAsIsCheckedMatch: true, allowStringEquality: _allowStringEquality);

									var _item = new InputColItem(valueName, value, isSel);
									kvlist.Add(_item);
								}
							}
						}
						return (optionLabel, kvlist.Where(kv => kv != null).ToArray());
					}
				}
			}
			return (optionLabel, null);
		}

		InputColItem[] _alterCollForSelectOption(IEnumerable<InputColItem> items, bool winnowNullSelectOption, ref string optionLabel)
		{
			InputColItem nullValueItem = null;
			if (winnowNullSelectOption && ModelTypeIsNullable) { // since at this moment, we're only interested in enums and bools (value types), must be nullable for us to try to extract a pre-existing option value 
				bool isBool = MainType == typeof(bool);
				if ((MainType.IsEnum || isBool)) {

					// find if there is already a nullable item (but ONLY 1), IF so, 
					// get the optionLabel from that if needed, but also delete it from the list...
					nullValueItem = items.SingleOrDefault(itm => itm.Value == null);

					// not going to mess with other types at this time for this, so by this point, it IS a enum or bool list

					string nullItemOptionLabel = nullValueItem?.Name;

					if (optionLabel.IsNulle())
						optionLabel = nullItemOptionLabel ?? (isBool ? "(Not Set)" : "--Select--");
				}
			}
			return items.Where(k => k != null && k != nullValueItem).ToArray();
		}

		public TagBuilder ProcessInputTagBuilder(
			TagHelperContext context,
			TagHelperOutput output)
		{
			TagBuilder tb = base.ProcessTagBuilder(context, output);

			tb.AddCssClass("form-control");

			if (SetPlaceholder) {
				string placeholder = Placeholder.FirstNotNulle(LabelText).NullIfEmptyTrimmed();
				if (placeholder.NotNulle())
					tb.Attributes["placeholder"] = placeholder;
			}

			return tb;
		}

		public TagBuilder SetExtraTextInputValues(TagBuilder tb, bool isTextArea, BsSize size, bool? disabled)
		{
			if (size != BsSize.Normal) // annoying, but AddCssClass adds to the START of the class, instead of last one is last in the class, so put this before "form-control"
				tb.AddCssClass(size.NameFormControl());

			tb.AddCssClass("form-control");

			if (SetPlaceholder) {
				string placeholder = Placeholder.FirstNotNulle(LabelText).NullIfEmptyTrimmed();
				if (placeholder.NotNulle())
					tb.Attributes["placeholder"] = placeholder;
			}

			//if (Rows > 0)
			//	inputTbx.Attributes["rows"] = Rows.ToString();

			if (disabled == true)
				tb.AddDisabledAttribute(disabled);

			return tb;
		}

		public TagBuilder GetValidationMessageTag(string message = null, object htmlAttributes = null)
		{
			TagBuilder validationMsgTB = Generator.GenerateValidationMessage(
				ViewContext,
				For.ModelExplorer,
				Name,
				message: message,
				tag: null,
				htmlAttributes: htmlAttributes);

			return validationMsgTB;
		}

		//public TagBuilder GetLabelTag(bool noLabel, string labelText = null, object htmlAttributes = null)
		//	=> noLabel ? null : GetLabelTag(labelText.FirstNotNulle(LabelText), htmlAttributes);

		public TagBuilder GetLabelTag(string labelText, object htmlAttributes = null)
		{
			if (labelText == null) // ALLOW empty or whitespace, tho empty returns null as well...
				return null;
			TagBuilder labelTh = Generator.GenerateLabel(
				ViewContext,
				For.ModelExplorer,
				Name,
				labelText: labelText,
				htmlAttributes: htmlAttributes);
			return labelTh;
		}

		public TagBuilder GetTextarea(int rows, int columns, object htmlAttributes = null)
		{
			TagBuilder tb = Generator.GenerateTextArea(
				ViewContext,
				For.ModelExplorer,
				Name,
				rows,
				columns,
				htmlAttributes: htmlAttributes);
			return tb;
		}

		public TagBuilder GetRadioButton(object value, bool? isChecked, string id, object htmlAttributes = null)
		{
			TagBuilder tb = Generator.GenerateRadioButton(
				ViewContext,
				For.ModelExplorer,
				FullName, // Name, -- Name or FullName?
				value ?? "",
				isChecked,
				htmlAttributes: htmlAttributes);

			if(id.NotNulle())
				tb.Attributes["id"] = id;

			if (isChecked == true && value == null) {
				// even though we entered true for isChecked, it seems the framework generator here is not setting if value is null
				// bug? anyways, we'll do it ourselves then:
				tb.Attributes["checked"] = "checked";
				//tb.MergeAttribute("checked", "checked" && tb.Attributes.con);
			}
			return tb;
		}

		public TagBuilder GetSelect(string optionLabel, bool allowMultiple, InputColItem[] items, object htmlAttributes = null)
		{
			SelectListItem[] selItems = items?
				.Select(itm => new SelectListItem() {
					Text = itm.Name,
					Value = itm.Value?.ToString(),
					Selected = itm.IsSelected
				})
				.ToArray();

			TagBuilder tb = Generator.GenerateSelect(
				ViewContext,
				For.ModelExplorer,
				optionLabel,
				FullName,
				selItems,
				null,
				allowMultiple,
				htmlAttributes: htmlAttributes);
			return tb;
		}

		public string GetLabelText()
		{
			string val = Meta.Description ?? Meta.DisplayName;
			//?? meta.ShortDisplayName; //?? meta.Watermark; // ended up not being good

			if (val == null) {
				val = FullName ?? Name ?? FullNameId ?? For.Metadata.PropertyName; // info.FullName ?? info.Name ?? info.Id ?? info.Meta.PropertyName;
				if (val != null) {
					if (val.IndexOf('.') >= 0)
						val = val.Split(__PeriodSplit).Last();
					val = SplitUpperCaseAndNonAlphaNumeric(val);
				}
			}

			return val;
		}

		static char[] __PeriodSplit = { '.' };

		public static string SplitUpperCaseAndNonAlphaNumeric(string input)
		{
			//return rx.Replace(input, " $1").Trim();

			if (input == null || input.Length < 3)
				return input;

			// Basic Check if is single word (assuming uppercase first letter)
			if (input[0].IsAsciiUpper() && input[1].IsAsciiLower()) {
				int i = 2;
				for (; i < input.Length; i++) {
					if (!input[i].IsAsciiLower())
						break;
				}
				if (i >= input.Length)
					return input;
			}

			StringBuilder sb = new StringBuilder(input.Length + 10);

			//bool lastUpper = input[0].IsAsciiUpper();
			int len = input.Length;

			sb.Append(input[0]);

			for (int i = 1; i < len; i++) {
				// LOWERCASE - 80%
				if (input[i].IsAsciiLower()) {
					sb.Append(input[i]);
				}
				//UPPERCASE - 18%
				else if (input[i].IsAsciiUpper()) {
					if (!input[i - 1].IsAsciiUpper() || (i + 1 < len && !input[i + 1].IsAsciiUpper()))
						sb.Append(' ');
					sb.Append(input[i]);
				}
				// NUMERIC. ABOVE is the vast majority though 
				else if (input[i].IsAsciiDigit()) {
					if (!input[i - 1].IsAsciiDigit())
						sb.Append(' ');
					sb.Append(input[i]);
					++i;
					for (; i < len; i++) {
						if (!input[i].IsAsciiDigit()) {
							--i; break;
						}
						sb.Append(input[i]);
					}
					if (i + 1 < len)
						sb.Append(' ');
				}
				// NON alpha-numeric, insert space if last wasn't space
				else if (sb[sb.Length - 1] != ' ')
					sb.Append(' ');
			}
			return sb.ToString();
		}



		public InputColItem[] GetInputEnumValues(
			IEnumerable<string> enumNames = null,
			IEnumerable values = null,
			bool countNullAsIsChecked = true)
		{
			return GetInputEnumValuesStatic(
					ModelValue,
					ModelType,
					HtmlFieldPrefix,
					PropertyName,
					enumNames,
					values,
					countNullAsIsChecked);
		}

		public static InputColItem[] GetInputEnumValuesStatic(
			object modelValue,
			Type modelType,
			string htmlFieldPrefix,
			string propertyName,
			IEnumerable<string> enumNames = null,
			IEnumerable values = null,
			bool countNullAsIsChecked = true)
		{
			EnumInfo enumInfo = EnumInfo.GetEnumInfo(modelType);

			if (enumInfo == null)
				return null;

			string[] _enumNames = enumNames?.Select(nm => nm?.HtmlEncode()).ToArray();
			object[] _values = values?.Cast<object>().ToArray();

			if (_enumNames.IsNulle())
				_enumNames = enumInfo.Names;

			if (_values.IsNulle())
				_values = enumInfo.Values; // _enumNames; // need to change, why not use the raw enum values themselves and do object comparisons, but oh well, for now, using string comparison

			// we've got our internal arr based versions, null these two now:
			enumNames = null;
			values = null;

			if (_enumNames.IsNulle() && _values.IsNulle())
				return null;
			else if (_enumNames == null || _values == null || _enumNames.Length != _values.Length)
				throw new ArgumentOutOfRangeException("Enum names and values must be of equal length.");

			string idPart1 = htmlFieldPrefix + "_" + propertyName + "_";
			bool allowStringEquality = false; // hmmm: this will NEVER be true, given above that requires an enum... modelValue is string;
			string modelValueStr = null; // see note just above: modelValue?.ToString();

			var arr = new InputColItem[_enumNames.Length];

			for (int i = 0; i < _enumNames.Length; i++) {
				object val = _values[i];
				var item = new InputColItem() {
					Name = _enumNames[i],
					Value = val,
					Id = idPart1 + val,
					IsSelected = ObjectIsEqual(modelValue, modelValueStr, val, countNullAsIsChecked, allowStringEquality: allowStringEquality)
				};
				arr[i] = item;
			}
			return arr;
		}

	}

	public class BoolNames
	{
		public string TrueName { get; private set; }
		public string FalseName { get; private set; }
		public string NotSetName { get; private set; }

		public BoolNames(string trueName = null, string falseName = null, string notSetName = null)
		{
			TrueName = trueName; //?.HtmlEncode();
			FalseName = falseName; //?.HtmlEncode();
			NotSetName = notSetName; //?.HtmlEncode();
		}

		public static BoolNames GetBoolNames(string trueName = null, string falseName = null, string notSetName = null)
		{
			return (trueName == null && falseName == null && notSetName == null)
				? null
				: new BoolNames(trueName, falseName, notSetName);
		}

		public BoolNames HtmlEncodeValues()
		{
			TrueName = TrueName?.HtmlEncode();
			FalseName = FalseName?.HtmlEncode();
			NotSetName = NotSetName?.HtmlEncode();
			return this;
		}

		public BoolNames Merge(BoolNames sec)
		{
			if (TrueName.IsNulle())
				TrueName = sec?.TrueName;
			if (FalseName.IsNulle())
				FalseName = sec?.FalseName;
			if (NotSetName.IsNulle())
				NotSetName = sec?.NotSetName;
			return this;
		}
	}
}