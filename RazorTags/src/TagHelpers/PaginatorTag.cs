using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetXtensions;
using FontIcons;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorTags
{
	[HtmlTargetElement("paginator", TagStructure = TagStructure.WithoutEndTag)]
	public class PaginatorTag : TagHelperHtmlContent, IPaginatorWriter
	{
		// --- DEFAULTS ---

		public static bool DefaultAjaxLinks = false;
		public static bool DefaultAlwaysShowPrevNext = false;
		public static bool DefaultShowFirstLast = true;
		public static bool DefaultNoLinkOnCurrentPage = false;
		public static string DefaultPrevious = "Previous"; //"«"; // "Previous"
		public static string DefaultNext = "Next"; //"»"; // "Next"
		public static string DefaultPreviousChapter = "«";
		public static string DefaultNextChapter = "»";

		/// <summary>
		/// CSS class the paginator gap (e.g. an ellipsis: '...') is marked with.
		/// </summary>
		public static string GapClass = "paginator-gap";

		/// <summary>
		/// CSS class the current page is marked with.
		/// </summary>
		public static string ActivePageClass = "active";

		public static string DefaultGap = "…";
		public static bool DefaultShowGap = false;

		[HtmlAttributeName("ajax-links")]
		public bool AjaxLinks { get; set; } = DefaultAjaxLinks;

		[HtmlAttributeName("always-show-prev-next")]
		public bool AlwaysShowPrevNext { get; set; } = DefaultAlwaysShowPrevNext;

		[HtmlAttributeName("prev-next")]
		public bool PrevNext { get; set; } = DefaultShowFirstLast;

		[HtmlAttributeName("no-link-on-current")]
		public bool NoLinkOnCurrentPage { get; set; } = DefaultNoLinkOnCurrentPage;

		[HtmlAttributeName("gap")]
		public string Gap { get; set; } = DefaultGap;

		[HtmlAttributeName("show-gap")]
		public bool ShowGap { get; set; } = DefaultShowGap;

		[HtmlAttributeName("aria-label")]
		public string AriaLabel { get; set; } = "Page navigation";

		[HtmlAttributeName("prev")]
		public string Previous { get; set; } = DefaultPrevious;

		[HtmlAttributeName("next")]
		public string Next { get; set; } = DefaultNext;

		[HtmlAttributeName("jump-links")]
		public bool Chapters { get; set; } = true;

		[HtmlAttributeName("prev-jump")]
		public string PreviousChapter { get; set; } = DefaultPreviousChapter;

		[HtmlAttributeName("next-jump")]
		public string NextChapter { get; set; } = DefaultNextChapter;

		[HtmlAttributeName("ajax-target")]
		public string AjaxTarget { get; set; }

		[HtmlAttributeName("page-info")]
		public PageInfo PageInfo { get; set; }

		[HtmlAttributeName("size")]
		public BsSize Size { get; set; }

		[HtmlAttributeName("page-to-href")]
		public Func<int, string> PageToHref { get; set; }

		// hide these for now, we were never using them currently anyways...
		//Func<int, string> NumberToLI { get; set; }
		Func<int, string> PageToLabel { get; set; }


		public bool CanShowPrevious => !Previous.IsNullOrEmpty();

		public bool CanShowNext => !Next.IsNullOrEmpty();

		public static string ajax_url_attribute_name = "data-ajax-url"; // AJAX.url -- is a constant value...

		public void WriteLink(
			int page,
			bool isCurrentPage,
			bool isDisabled = false,
			Func<int, string> pageToHref = null,
			Func<int, string> pageToLabel = null)
		{
			if (page == 1 && _sbLnks.IsNulle())
				_sbLnks.AppendLine();
			_sbLnks.Append("		<li class=\"page-item")
				.AppendIf(isCurrentPage, " active")
				.AppendIf(isDisabled, " disabled")
				.Append(@"""><a class=""page-link""");

			bool noHref = !PageInfo.IsPageInRange(page) || isDisabled || (isCurrentPage && NoLinkOnCurrentPage);

			string href = pageToHref != null && !noHref ? pageToHref(page) : null;
			string hrefAttrName = AjaxLinks ? ajax_url_attribute_name : "href";

			_sbLnks.AppendIf(href.NotNulle(), " ", hrefAttrName, "=\"", href, "\"");

			_sbLnks.Append(">")
				.Append(pageToLabel == null ? page.ToString() : pageToLabel(page) ?? "")
				.Append("</a></li>");
			_sbLnks.AppendLine();
		}

		public void WriteGap()
		{
			string gapVal = Gap.IsNulle() ? null : $@"<li class=""page-item""><a class=""page-link {GapClass}"">{Gap}</a></li>";
			_sbLnks.AppendLine(gapVal);
		}

		public void WritePrevNextPage(int page, bool isNext, bool isForChapter = false, bool isDisabled = false)
		{
			WriteLink(
				page,
				false,
				isDisabled,
				PageToHref,
				p => isNext
					? (isForChapter ? NextChapter : Next)
					: (isForChapter ? PreviousChapter : Previous));
		}

		public void WritePage(int page, bool isCurrent = false, bool isDisabled = false)
		{
			WriteLink(
				page, 
				isCurrent, 
				isDisabled, 
				PageToHref, 
				PageToLabel);
		}

		StringBuilder _sbLnks;

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			Process(output);
		}

		public void Process(TagHelperOutput output)
		{
			PageInfo pi = PageInfo;
			if (pi == null || pi.TotalItemCount == 0 || pi.TotalPageCount <= 1) {
				output.SuppressOutput();
				return;
			}
			
			output.PreElement.AppendHtml($@"<nav aria-label=""{AriaLabel}"">
	");

			output.TagName = "ul";
			output.TagMode = TagMode.StartTagAndEndTag;

			output.Attributes.AddOrAppendClass($"pagination {Size.NamePagination()}");

			_sbLnks = new StringBuilder(256);
			
			bool success = pi.WritePages(this);
			if (!success) {
				output.SuppressOutput();
				return;
			}

			string linksHtml = _sbLnks.ToString();

			if (linksHtml.IsNulle()) {
				output.SuppressOutput();
				return;
			}

			output.Content.AppendHtml(linksHtml);

			output.PostElement.AppendHtml($@"
</nav>");
		}

	}
}