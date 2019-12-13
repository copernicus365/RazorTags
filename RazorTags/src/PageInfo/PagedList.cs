using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorTags
{

	public class PagedList<T> : IList<T> where T : class
	{
		public PageInfo PagingInfo { get; set; }

		public IList<T> Items { get; set; }

		public PagedList() { }

		#region --- GetPagedList ---

		public static PagedList<T> GetPagedList(
			int totalItemsCount,
			Func<int, int, IList<T>> getRange,
			int page,
			int itemsPerPage,
			int? maxDisplayPages = null)
		{
			var list = _getPagedListBase(totalItemsCount, page, itemsPerPage, maxDisplayPages);
			if (list == null || totalItemsCount == 0)
				return list;

			list.Items = getRange(list.PagingInfo.CurrentPageStartIndex, list.PagingInfo.ItemsOnThisPage);
			return list;
		}

		public static async Task<PagedList<T>> GetPagedListAsync(
			int totalItemsCount,
			Func<int, int, Task<IList<T>>> getRange,
			int page,
			int itemsPerPage,
			int? maxDisplayPages = null)
		{
			var list = _getPagedListBase(totalItemsCount, page, itemsPerPage, maxDisplayPages);
			if (list == null || totalItemsCount == 0)
				return list;

			list.Items = await getRange(list.PagingInfo.CurrentPageStartIndex, list.PagingInfo.ItemsOnThisPage);
			return list;
		}

		public static async Task<PagedList<T>> GetPagedListAsync(
			int totalItemsCount,
			Func<int, int, Task<T[]>> getRange,
			int page,
			int itemsPerPage,
			int? maxDisplayPages = null)
		{
			var list = _getPagedListBase(totalItemsCount, page, itemsPerPage, maxDisplayPages);
			if (list == null || totalItemsCount == 0)
				return list;

			list.Items = await getRange(list.PagingInfo.CurrentPageStartIndex, list.PagingInfo.ItemsOnThisPage);
			return list;
		}

		static PagedList<T> _getPagedListBase(
			int totalItemsCount,
			int page,
			int itemsPerPage,
			int? maxDisplayPages = null)
		{
			var list = new PagedList<T>();
			list.PagingInfo = PageInfo.GetPagingInfo(totalItemsCount, itemsPerPage, page, maxDisplayPages);
			if (list.PagingInfo == null)
				return null;

			if (totalItemsCount == 0)
				list.Items = new T[0];

			return list;
		}

		#endregion

		// IList

		public int IndexOf(T item)
		{
			return Items.IndexOf(item);
		}

		public T this[int index]
		{
			get { return Items[index]; }
			set { Items[index] = value; }
		}

		public bool Contains(T item)
		{
			return Items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Items.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return Items.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)(Items)).GetEnumerator();
		}

		#region --- *Not Implemented* IList Members ---

		void ICollection<T>.Add(T item) => throw new NotImplementedException();

		void ICollection<T>.Clear() => throw new NotImplementedException();

		void IList<T>.Insert(int index, T item) => throw new NotImplementedException();

		bool ICollection<T>.Remove(T item) => throw new NotImplementedException();

		void IList<T>.RemoveAt(int index) => throw new NotImplementedException();

		#endregion

	}
}
