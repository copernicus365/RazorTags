using System;
using System.Collections.Generic;
using System.Linq;
using DotNetXtensions;

namespace RazorTags
{
	public class EnumInfo
	{
		public string TypeName { get; set; }
		public string Name { get; set; }
		public string[] Names { get; set; }
		public int[] NumericValues { get; set; }
		public object[] Values { get; set; }

		public static Dictionary<Type, EnumInfo> EnumInfoCachedDictionary = new Dictionary<Type, EnumInfo>();

		public static EnumInfo GetEnumInfo(Type type, bool tryGetUnderlyingTypeIfNullable = true)
		{
			if (type == null)
				return null;

			if (!type.IsEnum) {
				type = tryGetUnderlyingTypeIfNullable
					? type.GetUnderlyingTypeIfNullable()
					: null;
				if (type == null || !type.IsEnum)
					return null;
			}

			if (EnumInfoCachedDictionary.TryGetValue(type, out EnumInfo eval))
				return eval;

			object[] values = Enum.GetValues(type).OfType<object>().ToArray();
			eval = new EnumInfo() {
				TypeName = type.FullName,
				Name = type.Name,
				Names = Enum.GetNames(type),
				Values = values,
				NumericValues = values.Select(o => (int)o).ToArray()
			};
			EnumInfoCachedDictionary[type] = eval; // Don't do 'Add', bec first time hits invariably hit more than once (then double add excep). But no need for lock, just let those first times pass
			return eval;
		}
	}
}
