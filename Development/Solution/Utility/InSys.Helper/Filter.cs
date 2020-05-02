using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using z.Data;

namespace InSys.Helper
{
    public class FilterSchema
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public FilterType Type { get; set; }
    }
    public class Filter
    {
        public static bool Contains(int me, string[] co)
        {
            if (co.Contains(me.ToString())) return true;
            else return false;
        }
        public static string BuildStringFilter(List<FilterSchema> filters, int counter = 0)
        {
            string ret = "";
            List<string> conditions = new List<string>();
            foreach (var fs in filters)
            {
                string expression = "";
                switch (fs.Type)
                {
                    case FilterType.Equals:
                        expression = $"a.{fs.Name} = {{{counter.ToString()}}}"; break;
                    case FilterType.GreaterThan:
                        expression = $"a.{fs.Name} > {{{counter.ToString()}}}"; break;
                    case FilterType.GreaterThanOrEqual:
                        expression = $"a.{fs.Name} >= {{{counter.ToString()}}}"; break;
                    case FilterType.LessThan:
                        expression = $"a.{fs.Name} < {{{counter.ToString()}}}"; break;
                    case FilterType.LessThanOrEqual:
                        expression = $"a.{fs.Name} <= {{{counter.ToString()}}}"; break;
                    case FilterType.IN:
                        List<string> p = new List<string>();
                        int cc = counter;
                        foreach (string o in fs.Value.ToString().Split(","))
                        {
                            p.Add($"{{{cc}}}");
                            cc++;
                        }
                        counter = cc - 1;
                        expression = $"a.{fs.Name} IN ({string.Join(",", p)})"; break;
                    case FilterType.NotIN:
                        List<string> p2 = new List<string>();
                        int cc2 = counter;
                        foreach (string o in fs.Value.ToString().Split(","))
                        {
                            p2.Add($"{{{cc2}}}");
                            cc2++;
                        }
                        counter = cc2 - 1;
                        expression = $"a.{fs.Name} NOT IN ({string.Join(",", p2)})"; break;
                    case FilterType.Between:
                        var val = new List<string>();
                        foreach (var d in fs.Value.ToObject<List<string>>()) val.Add(d.IsNull("").ToString());
                        if (val[0] == "") val[0] = val[1];
                        if (val[1] == "") val[1] = val[0];
                        if (val[0] == "" && val[1] == "") break;

                        List<string> p3 = new List<string>();
                        p3.Add($"{{{counter}}}");
                        p3.Add($"{{{counter + 1}}}");
                        counter++;
                        expression = $"a.{fs.Name} BETWEEN {string.Join(" AND ", p3)}"; break;
                    case FilterType.Like:
                        expression = $"a.{fs.Name} like '%' + {{{counter.ToString()}}} + '%'"; break;
                    default:
                        expression = $"a.{fs.Name} = {{{counter.ToString()}}}"; break;
                }
                if (expression != "")
                {
                    conditions.Add(expression);
                    counter++;
                }
            }
            if (filters.Count > 0 && conditions.Count(x => x != "") > 0) ret = $"WHERE {string.Join(" AND ", conditions.Where(x => x != "").ToList())}";
            return ret;
        }
    }
    public enum FilterType : int
    {
        Equals = 1,
        GreaterThan = 2,
        LessThan = 3,
        GreaterThanOrEqual = 4,
        LessThanOrEqual = 5,
        Contains = 6,
        StartsWith = 7,
        EndsWith = 8,
        Like = 9,
        IN = 10,
        NotIN = 11,
        Between = 12
    }
}
