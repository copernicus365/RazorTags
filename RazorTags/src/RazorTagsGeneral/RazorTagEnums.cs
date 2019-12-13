using System;
using System.Net;
using System.Text;
using DotNetXtensions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	public enum Placement
	{
		None = 0,
		Top = 1,
		Right = 2,
		Bottom = 3,
		Left = 4
	}

	public enum BsSize
	{
		Normal = 0,
		Small = 1,
		Large = 2
	}

	public enum SizeVal
	{
		Normal = 0,
		Mini,
		Small,
		Medium,
		Large,
		XLarge,
		XXLarge
	}

	public enum BsKind
	{
		None = 0,
		Primary = 1,
		Secondary = 2,
		Success = 3,
		Danger = 4,
		Warning = 5,
		Info = 6,
		Light = 7,
		Dark = 8
	}

	public enum Align
	{
		None = 0,
		Left = 1,
		Center = 2,
		Right = 3
	}

	public enum ButtonType
	{
		Button = 0,
		Submit = 1,
		Link = 2,
		Action = 3,
	}

	public enum BasicColor
	{
		None = 0,
		Red,
		RedLight,
		Pink,
		PinkLight,
		Orange,
		OrangeLight,
		Yellow,
		YellowLight,
		Green,
		GreenLight,
		Blue,
		BlueLight,
		BlueDark,
		Purple,
		PurpleLight
	}

	public static class RazorTagsX
	{
		public static string Name(this Placement val)
		{
			switch (val) {
				case Placement.Top: return "top";
				case Placement.Right: return "right";
				case Placement.Bottom: return "bottom";
				case Placement.Left: return "left";
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public static string NameFormControl(this BsSize val)
		{
			switch (val) {
				case BsSize.Normal: return null;
				case BsSize.Small: return "form-control-sm";
				case BsSize.Large: return "form-control-lg";
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public static string NameButton(this BsSize val)
		{
			switch (val) {
				case BsSize.Normal: return null;
				case BsSize.Small: return "btn-sm";
				case BsSize.Large: return "btn-lg";
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public static string NamePagination(this BsSize val)
		{
			switch (val) {
				case BsSize.Normal: return null;
				case BsSize.Small: return "pagination-sm";
				case BsSize.Large: return "pagination-lg";
				default: throw new ArgumentOutOfRangeException();
			}
		}


		public static string TextColor(this BsKind val)
		{
			string color = val.Name();
			if (color.NotNulle()) {
				return $"text-{color}";
			}
			return null;
		}
		public static string BgColor(this BsKind val)
		{
			string color = val.Name();
			if (color.NotNulle()) {
				return $"bg-{color}";
			}
			return null;
		}

		public static string Name(this BsKind val)
			=> val == BsKind.None ? null : XEnum<BsKind>.Name((int)val).ToLower();

	}
}