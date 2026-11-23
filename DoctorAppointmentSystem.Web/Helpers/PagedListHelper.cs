using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorAppointmentSystem.Web.Helpers
{
    public interface IPagedList : IEnumerable
    {
        int PageNumber { get; }
        int PageSize { get; }
        int TotalItemCount { get; }
        int PageCount { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
    }

    public interface IPagedList<out T> : IPagedList, IEnumerable<T>
    {
    }

    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalItemCount { get; }
        public int PageCount { get; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < PageCount;
        public bool IsFirstPage => PageNumber == 1;
        public bool IsLastPage => PageNumber >= PageCount;

        public PagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize;
            TotalItemCount = source.Count();
            PageCount = (int)Math.Ceiling(TotalItemCount / (double)PageSize);
            if (PageCount == 0) PageCount = 1;

            var items = source.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
            AddRange(items);
        }

        public PagedList(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize;
            TotalItemCount = source.Count();
            PageCount = (int)Math.Ceiling(TotalItemCount / (double)PageSize);
            if (PageCount == 0) PageCount = 1;

            var items = source.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
            AddRange(items);
        }
    }

    public static class PagedListExtensions
    {
        public static IPagedList<T> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
            => new PagedList<T>(source, pageNumber, pageSize);

        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
            => new PagedList<T>(source, pageNumber, pageSize);
    }

    public static class HtmlHelperExtensions
    {
        public static IHtmlContent PagedListPager(this IHtmlHelper html, IPagedList list, Func<int, string> generatePageUrl)
        {
            if (list == null || list.PageCount <= 1) return HtmlString.Empty;

            var sb = new StringBuilder();
            sb.Append("<nav aria-label='Page navigation'><ul class='pagination justify-content-center mt-3'>");

            if (list.HasPreviousPage)
            {
                sb.Append($"<li class='page-item'><a class='page-link' href='{generatePageUrl(list.PageNumber - 1)}'>&laquo; Previous</a></li>");
            }
            else
            {
                sb.Append("<li class='page-item disabled'><span class='page-link'>&laquo; Previous</span></li>");
            }

            for (int i = 1; i <= list.PageCount; i++)
            {
                string active = i == list.PageNumber ? " active" : "";
                sb.Append($"<li class='page-item{active}'><a class='page-link' href='{generatePageUrl(i)}'>{i}</a></li>");
            }

            if (list.HasNextPage)
            {
                sb.Append($"<li class='page-item'><a class='page-link' href='{generatePageUrl(list.PageNumber + 1)}'>Next &raquo;</a></li>");
            }
            else
            {
                sb.Append("<li class='page-item disabled'><span class='page-link'>Next &raquo;</span></li>");
            }

            sb.Append("</ul></nav>");
            return new HtmlString(sb.ToString());
        }
    }
}
