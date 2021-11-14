namespace KnowledgeBase.Utilities.Helpers
{
    public class ApiNotFoundResponse : ApiResponse
    {
        public ApiNotFoundResponse(string message = null) : base(404, message)
        {
        }
    }
}