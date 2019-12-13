using System;
using System.ComponentModel.DataAnnotations;

namespace RazorTags.WebDemo.Models
{
	public class PlaygroundVM
	{
		[StringLength(21)]
		[Required]
		public string Name { get; set; }

		public string Title { get; set; }

		[StringLength(40)]
		public string Description { get; set; }

		[StringLength(40, MinimumLength = 8)]
		[DataType(DataType.Password)]
		public string PWord { get; set; }

		[Range(1, 99)]
		[Required]
		public int Age { get; set; }

		public bool IsCool { get; set; }

		public bool IsSanctified { get; set; } = true;

		[Required]
		public bool? RunToStore { get; set; }

		[DataType(DataType.PhoneNumber)]
		public string Phone { get; set; }

		[StringLength(42)]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		public ColorType? Color { get; set; }

		public int Page { get; set; }

		//[DataType(DataType.DateTime)]
		public DateTime PublishedTime { get; set; }


    }

	public enum ColorType
	{
		White = 0,
		Black,
		Grey,
		Red,
		Orange,
		Yellow,
		Green,
		Blue,
		Purple,
	}

}