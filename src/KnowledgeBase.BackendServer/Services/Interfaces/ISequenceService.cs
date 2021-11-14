using System.Threading.Tasks;

namespace KnowledgeBase.BackendServer.Services.Interfaces
{
    public interface ISequenceService
    {
        Task<int> GetKnowledgeNewId();
    }
}