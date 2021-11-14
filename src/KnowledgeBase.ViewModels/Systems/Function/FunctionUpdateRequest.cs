namespace KnowledgeBase.ViewModels.Systems.Function
{
    public class FunctionUpdateRequest
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int SortOrder { get; set; }
        public string ParentId { get; set; }
    }
}