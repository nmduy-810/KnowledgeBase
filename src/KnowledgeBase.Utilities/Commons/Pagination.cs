using System.Collections.Generic;

namespace KnowledgeBase.Utilities.Commons
{
    public class Pagination<T>
    {
        public List<T> Items { get; set; }
        public int TotalRecords { get; set; }
    }
}