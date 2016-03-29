﻿using Lisa.Common.WebApi;
using System.Collections.Generic;
using System.Linq;

namespace Lisa.Breakpoint.Api
{
    public static class ReportSorter
    {
        public static List<DynamicModel> Sort(IEnumerable<DynamicModel> reports, string sort, string order)
        {
            foreach (string sortableField in sortableFields)
            {
                if (sort == sortableField)
                {
                    if (order.ToLower() == "asc")
                    {
                        return reports.AsQueryable().OrderBy(r => r[sort]).ToList();
                    }
                    else if (order.ToLower() == "desc")
                    {
                        return reports.AsQueryable().OrderByDescending(r => r[sort]).ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public static string[] sortableFields = new string[4] {
            "id",
            "title",
            "project",
            "reported"
        };
    }
}