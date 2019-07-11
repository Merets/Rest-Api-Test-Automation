using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace RestApiTestAutomation.Tools
{
    public static class Utils
    {
        public static bool DoesIncludeList<T>(this IEnumerable<T> list, IEnumerable<T> expectedList)
        {
            return list.Intersect(expectedList).Count() == expectedList.Count();
        }

        public static void PrintAllList<T>(this IEnumerable<T> list)
        {
            list.ToList().ForEach(u => Console.WriteLine(u.ToString()));
        }



    }
}
