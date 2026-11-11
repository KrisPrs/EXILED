// -----------------------------------------------------------------------
// <copyright file="CategoryGrouping.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomItems.Commands.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

#pragma warning disable SA1600
    internal static class CategoryGrouping
#pragma warning restore SA1600
    {
        private const string OtherCategoryName = "Другое";

        private const string EntryIndent = "";

        private const string CategoryPrefix = "// ";

#pragma warning disable SA1600
        public static StringBuilder AppendByCategories<T>(this StringBuilder builder, IEnumerable<T> entries, Func<T, string> getCategory, Action<StringBuilder, T, string> appendEntry)
#pragma warning disable SA1600
        {
            List<T> entryList = entries.ToList();

            if (builder.Length > 0 && builder[builder.Length - 1] != '\n')
                builder.AppendLine();

            if (entryList.All(entry => string.IsNullOrEmpty(getCategory(entry))))
            {
                foreach (T entry in entryList)
                    appendEntry(builder, entry, string.Empty);

                return builder;
            }

            bool isFirstGroup = true;
            foreach (IGrouping<string?, T> group in entryList
                         .GroupBy(entry => string.IsNullOrEmpty(getCategory(entry)) ? null : getCategory(entry))
                         .OrderBy(group => group.Key is null))
            {
                if (!isFirstGroup)
                    builder.AppendLine();

                isFirstGroup = false;
                builder.Append(CategoryPrefix).Append(group.Key ?? OtherCategoryName).AppendLine();

                foreach (T entry in group)
                    appendEntry(builder, entry, EntryIndent);
            }

            return builder;
        }
    }
}
