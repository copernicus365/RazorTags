
namespace RazorTags
{
	public class InputColItem
	{
		public InputColItem() { }
		public InputColItem(string name, object value, bool isSelected = false, string id = null)
		{
			Name = name;
			Value = value;
			IsSelected = isSelected;
			Id = id;
		}

		public string Name { get; set; }
		public object Value { get; set; }
		public bool IsSelected { get; set; }
		public string Id { get; set; }
	}
}