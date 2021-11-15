using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.Utilities.Helpers;
using KnowledgeBase.ViewModels.Contents.Vote;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Controllers
{
    public partial class KnowledgesController
    {
        #region Votes

        [HttpGet("{knowledgeId}/votes")]
        public async Task<IActionResult> GetVotes(int knowledgeId)
        {
            var votes = await _context.Votes
                .Where(x => x.KnowledgeId == knowledgeId)
                .Select(x => new VoteVm()
                {
                    UserId = x.UserId,
                    KnowledgeId = x.KnowledgeId,
                    CreateDate = x.CreateDate,
                    LastModifiedDate = x.LastModifiedDate
                }).ToListAsync();
            return Ok(votes);
        }
        
        [HttpPost("{knowledgeId}/votes")]
        [ApiValidationFilter]
        public async Task<IActionResult> PostVote(int knowledgeId, [FromBody] VoteCreateRequest request)
        {
            var vote = await _context.Votes.FindAsync(knowledgeId, request.UserId);
            if (vote != null)
                return BadRequest(new ApiBadRequestResponse("This user has been voted for this knowledge"));

            vote = new Vote()
            {
                KnowledgeId = knowledgeId,
                UserId = request.UserId
            };
            _context.Votes.Add(vote);

            var knowledge = await _context.Knowledges.FindAsync(knowledgeId);
            if (knowledge == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge with id {knowledgeId}"));

            knowledge.NumberOfVotes = knowledge.NumberOfVotes.GetValueOrDefault(0) + 1;
            _context.Knowledges.Update(knowledge);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
                return NoContent();
            
            return BadRequest(new ApiBadRequestResponse($"Vote failed"));
        }
        
        [HttpDelete("{knowledgeId}/votes/{userId}")]
        public async Task<IActionResult> DeleteVote(int knowledgeId, string userId)
        {
            var vote = await _context.Votes.FindAsync(knowledgeId, userId);
            if (vote == null)
                return NotFound(new ApiNotFoundResponse("Cannot found vote"));

            var knowledge = await _context.Knowledges.FindAsync(knowledgeId);
            if (knowledge == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge with id {knowledgeId}"));

            knowledge.NumberOfVotes = knowledge.NumberOfVotes.GetValueOrDefault(0) - 1;
            _context.Knowledges.Update(knowledge);

            _context.Votes.Remove(vote);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok();
            
            return BadRequest(new ApiBadRequestResponse($"Delete vote failed"));
        }

        #endregion
    }
}