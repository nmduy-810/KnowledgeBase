namespace KnowledgeBase.ViewModels.Contents.Knowledge
{
    public class KnowledgeQuickVm
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string SeoAlias { get; set; }
        public string Description { get; set; }
        public int? ViewCount { get; set; }
    }
}