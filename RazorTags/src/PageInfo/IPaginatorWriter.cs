
namespace RazorTags
{
	public interface IPaginatorWriter
	{
		void WritePage(int page, bool isCurrent = false, bool isDisabled = false);

		void WritePrevNextPage(int page, bool isNext, bool isForChapter = false, bool isDisabled = false);

		void WriteGap();

		bool CanShowPrevious { get; }

		bool CanShowNext { get; }

		bool Chapters { get; }

		bool AlwaysShowPrevNext { get; }

		bool ShowGap { get; }

	}
}
