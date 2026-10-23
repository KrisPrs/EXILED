// -----------------------------------------------------------------------
// <copyright file="CategoryGrouping.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.CustomRoles.Commands.Admin.List
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

        private const string EntryIndent = "   ";

#pragma warning disable SA1600
        public static StringBuilder AppendByCategories<T>(this StringBuilder builder, IEnumerable<T> entries, Func<T, string> getCategory, Action<StringBuilder, T, string> appendEntry)
#pragma warning restore SA1600
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
