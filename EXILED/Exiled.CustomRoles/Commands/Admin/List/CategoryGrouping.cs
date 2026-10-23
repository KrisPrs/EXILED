namespace Exiled.CustomRoles.Commands.Admin.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class CategoryGrouping
    {
        private const string OtherCategoryName = "Другое";

        private const string EntryIndent = "   ";

        public static StringBuilder AppendByCategories<T>(this StringBuilder builder, IEnumerable<T> entries, Func<T, string> getCategory, Action<StringBuilder, T, string> appendEntry)
        {
            List<T> entryList = entries.ToList();

            if (entryList.All(entry => string.IsNullOrEmpty(getCategory(entry))))
            {
                foreach (T entry in entryList)
                    appendEntry(builder, entry, string.Empty);

                return builder;
            }

            foreach (IGrouping<string?, T> group in entryList
                         .GroupBy(entry => string.IsNullOrEmpty(getCategory(entry)) ? null : getCategory(entry))
                         .OrderBy(group => group.Key is null))
            {
                builder.Append(group.Key ?? OtherCategoryName).AppendLine(":");

                foreach (T entry in group)
                    appendEntry(builder, entry, EntryIndent);
            }

            return builder;
        }
    }
}
