using System;
using DotNetXtensions;

namespace RazorTags
{
	public class PageInfo
	{
		#region FIELDS

		/// <summary>
		/// Maximum number of pages to display.
		/// It's best for this to be an ODD number, that way spans on each side will be equal.
		/// </summary>
		public static int DefaultMaxPagesCount = 13; // including 1rst and last 
		public static bool FixPageOutOfRange = true;

		int[] _pages = new int[0]; // helps for count==0 ?
		public readonly int CurrentPage;
		public readonly int CurrentPageStartIndex;
		public readonly int ItemsPerPage;

		/// <summary>
		/// Maximum number of pages to display.
		/// It's best for this to be an ODD number, 
		/// that way spans on each side will be equal.
		/// </summary>
		public readonly int MaxPagesCount = DefaultMaxPagesCount; // including 1rst and last 
		public readonly int TotalItemCount;
		public readonly int TotalPageCount;
		public readonly int ItemsOnThisPage;
		public readonly bool ShowFirstLastPages = true;

		/// <summary>
		/// The array of pages for display, the number of which is limited by 
		/// <see cref="MaxPagesCount"/>.
		/// </summary>
		public int[] Pages => _pages;

		/// <summary>
		/// The count of the final display <see cref="Pages"/>. This is different from <see cref="TotalPageCount"/>.
		/// </summary>
		public int PagesCount => _pages?.Length ?? 0;

		#endregion

		#region CONSTRUCTOR and INIT

		/// <summary>
		/// Constructor, which recieves the readonly numeric values that will dictate this page info.
		/// </summary>
		/// <param name="totalItemCount">Total number of items in the source collection.</param>
		/// <param name="itemsPerPage">Number of items from source to display per page.</param>
		/// <param name="currentPage">Current *page* (this will often have been dictated by e.g. a user clicking a given page to navigate to).</param>
		/// <param name="maxDisplayPages">The number of pages you want to generate (links or what not) for. 
		/// This sets the maximum length of the <see cref="Pages"/> array of numbers. The final number can be lower
		/// when not enough items from input values made up that many pages.</param>
		public PageInfo(int totalItemCount, int itemsPerPage, int currentPage, int? maxDisplayPages = null, bool? showFirstLastPages = null)
		{
			TotalPageCount = GetTotalPageCountAndValidateParams(totalItemCount, itemsPerPage, ref currentPage);

			TotalItemCount = totalItemCount;
			ItemsPerPage = Math.Min(itemsPerPage, TotalItemCount);
			CurrentPage = currentPage;
			MaxPagesCount = Math.Max(maxDisplayPages ?? DefaultMaxPagesCount, 3);
			if (showFirstLastPages != null)
				ShowFirstLastPages = showFirstLastPages.Value;

			//if (MaxDisplayPages < 3) -- just set a max above and be done with it...
			//	throw new ArgumentOutOfRangeException(nameof(maxDisplayPages), "Max pages to display cannot be less than 3");
			if (TotalPageCount < 0)
				throw new ArgumentOutOfRangeException();
			else if (TotalPageCount > 0) {
				CurrentPageStartIndex = ItemsPerPage * (CurrentPage - 1);
				SetPageNumbers();
			}

			ItemsOnThisPage = TotalPageCount < 2 || CurrentPage < TotalPageCount
					? ItemsPerPage
					: ItemsPerPage - ((TotalPageCount * ItemsPerPage) - TotalItemCount);
		}

		static int GetTotalPageCountAndValidateParams(int totalItemCount, int itemsPerPage, ref int currentPage)
		{
			if (totalItemCount == 0)
				return 0;

			if (currentPage < 1 && FixPageOutOfRange)
				currentPage = 1;

			if (totalItemCount < 1 || itemsPerPage < 1 || currentPage < 1)
				return -1;

			int totalPageCount = (int)Math.Ceiling((decimal)totalItemCount / itemsPerPage);

			if (currentPage > totalPageCount) {
				if (!FixPageOutOfRange || totalPageCount < 1)
					return -1;
				currentPage = totalPageCount;
			}

			return totalPageCount;
		}

		public static PageInfo GetPagingInfo(int totalItemCount, int itemsPerPage, int currentPage, int? maxDisplayPages = null)
		{
			// oops! maxDisplayPages not being used!
			if (GetTotalPageCountAndValidateParams(totalItemCount, itemsPerPage, ref currentPage) < 0)
				return null;

			return new PageInfo(totalItemCount, itemsPerPage, currentPage, maxDisplayPages);
		}

		#endregion

		public bool IsLastPage
			=> TotalPageCount < 1 || CurrentPage >= TotalPageCount;

		public bool IsPageInRange(int page)
			=> page > 0 && page <= TotalPageCount;

		public bool GapAfterStart
			=> PagesCount > 2 && _pages[1] > 2; // let's just read page 2, bypass if first page blah blah

		public bool GapBeforeEnd
			=> PagesCount > 2 && _pages[_pages.Length - 2] < (TotalPageCount - 1);




		// --- CORE FUNCTIONS ---

		public void SetPageNumbers()
		{
			if (TotalPageCount == 0)
				return;

			// AFTER this point, some pages ARE going to be Cut...!
			int cnt = Math.Min(MaxPagesCount, TotalPageCount);
			_pages = new int[cnt];

			if (TotalPageCount <= MaxPagesCount) {
				for (int i = 0; i < cnt; i++)
					_pages[i] = i + 1;
				return;
			}

			int sides = cnt / 2; // division of a odd never rounds up, even if > .5, so (5 / 2) = 2 

			int start = Math.Max(CurrentPage - sides, 1); // note: `start` is 1 based (it is in fact a page #), not index based
			if (start + cnt > TotalPageCount)
				start = TotalPageCount - cnt + 1;

			for (int i = 0; i < cnt; i++)
				_pages[i] = i + start;

			if (ShowFirstLastPages) {
				// REMEMBER: After this point, 
				_pages[0] = 1;
				_pages[cnt - 1] = TotalPageCount;

				//cnt = MaxDisplayPages - 2;
			}




			//bool _isBeginningRun = CurrentPage < middleCnt; //5;
			//bool _isEndRun = CurrentPage > TotalPageCount - 4;

			//if (_isBeginningRun)
			//	for (int i = 1; i <= cnt; i++)
			//		_displayPages[i] = i + 1;
			//else if (_isEndRun)
			//	for (int i = cnt, start = TotalPageCount - 1; i > 0; i--, start--)
			//		_displayPages[i] = start;
			//else { // _isMidRun
			//	int _start = CurrentPage - middleCnt;
			//	for (int i = 1, start = _start; i <= cnt; i++, start++) //CurrentPage - 3
			//		_displayPages[i] = start;
			//}
		}


		public bool WritePages(IPaginatorWriter writer)
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));
			var w = writer;

			if (TotalItemCount == 0 || TotalPageCount <= 1)
				return false;

			int[] pages = Pages;
			int page = pages[0];
			int currPg = CurrentPage;
			int pagesCountMinusFirstLast = PagesCount - (GapAfterStart ? 1 : 0) - (GapBeforeEnd ? 1 : 0);

			bool hasPrev = w.CanShowPrevious && currPg - 1 > 0;
			bool hasNext = w.CanShowNext && currPg + 1 <= TotalPageCount;

			//  --- Write Prev Chapt ---
			if (w.Chapters && GapAfterStart) {
				int prevChJumpPage = Math.Max(CurrentPage - Math.Max(pagesCountMinusFirstLast, 3), 1);

				w.WritePrevNextPage(
					prevChJumpPage,
					isNext: false,
					isForChapter: true);
			}

			//  --- Write Prev ---
			if (w.AlwaysShowPrevNext || hasPrev) {
				w.WritePrevNextPage(
					currPg - 1,
					isNext: false,
					isDisabled: !hasPrev);
			}

			//  --- Write Main Links ---
			int lastIdx = pages.Length - 1;
			bool showGap = w.ShowGap;

			for (int i = 0; i < pages.Length; i++) {
				page = pages[i];

				if (showGap && i == lastIdx && GapBeforeEnd)
					w.WriteGap();

				w.WritePage(
					page,
					isCurrent: page == currPg,
					isDisabled: false);

				if (showGap && i == 0 && GapAfterStart)
					w.WriteGap();
			}

			//  --- Write Next ---
			if (w.AlwaysShowPrevNext || hasNext) {
				w.WritePrevNextPage(
					currPg + 1,
					isNext: true,
					isDisabled: !hasNext);
			}

			//  --- Write Next Chapt ---
			if (w.Chapters && GapBeforeEnd) {
				int nextChJumpPage = Math.Min(CurrentPage + Math.Max(pagesCountMinusFirstLast, 3), TotalPageCount);
				w.WritePrevNextPage(
					nextChJumpPage,
					isNext: true,
					isForChapter: true);
			}
			return true;
		}

	}
}
