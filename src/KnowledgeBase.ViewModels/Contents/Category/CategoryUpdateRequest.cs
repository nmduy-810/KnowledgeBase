namespace KnowledgeBase.ViewModels.Contents.Category
{
    public class CategoryUpdateRequest
    {
        public string Name { get; set; }
        public string SeoAlias { get; set; }
        public string SeoDescription { get; set; }
        public int SortOrder { get; set; }
        public int? ParentId { get; set; }
    }
}