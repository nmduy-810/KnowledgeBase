using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.Utilities.Commons;
using KnowledgeBase.Utilities.Helpers;
using KnowledgeBase.ViewModels.Contents.Comment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Controllers
{
    public partial class KnowledgesController
    {
        [HttpGet("{knowledgeId}/comments/filter")]
        public async Task<IActionResult> GetCommentsPaging(int? knowledgeId, string filter, int pageIndex, int pageSize)
        {
            var query = from c in _context.Comments
                        join u in _context.Users on c.OwnerUserId equals u.Id
                        select new { c, u };
            
            if (knowledgeId.HasValue)
                query = query.Where(x => x.c.KnowledgeId == knowledgeId.Value);

            if (!string.IsNullOrEmpty(filter))
                query = query.Where(x => x.c.Content.Contains(filter));

            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).Select(c => new CommentVm()
                {
                    Id = c.c.Id,
                    Content = c.c.Content,
                    CreateDate = c.c.CreateDate,
                    KnowledgeId = c.c.KnowledgeId,
                    LastModifiedDate = c.c.LastModifiedDate,
                    OwnerUserId = c.c.OwnerUserId,
                    OwnerName = c.u.FirstName + " " + c.u.LastName
                }).ToListAsync();

            var pagination = new Pagination<CommentVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            
            return Ok(pagination);
        }

        [HttpGet("{knowledgeId}/comments/{commentId}")]
        public async Task<IActionResult> GetCommentDetail(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found comment with id: {commentId}"));
            
            var user = await _context.Users.FindAsync(comment.OwnerUserId);
            var commentVm = new CommentVm()
            {
                Id = comment.Id,
                Content = comment.Content,
                CreateDate = comment.CreateDate,
                KnowledgeId = comment.KnowledgeId,
                LastModifiedDate = comment.LastModifiedDate,
                OwnerUserId = comment.OwnerUserId,
                OwnerName = user.FirstName + " " + user.LastName
            };

            return Ok(commentVm);
        }

        [HttpPost("{knowledgeId}/comments")]
        public async Task<IActionResult> PostComment(int knowledgeId, [FromBody] CommentCreateRequest request)
        {
            var comment = new Comment()
            {
                Content = request.Content,
                KnowledgeId = request.KnowledgeId,
                // OwnerUserId = User.GetUserId()
                OwnerUserId = ""
            };
            _context.Comments.Add(comment);

            var knowledge = await _context.Knowledges.FindAsync(knowledgeId);
            if (knowledge == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge with id: {knowledgeId}"));

            knowledge.NumberOfComments = knowledge.NumberOfVotes.GetValueOrDefault(0) + 1;
            _context.Knowledges.Update(knowledge);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
                return CreatedAtAction(nameof(GetCommentDetail), new { id = knowledgeId, commentId = comment.Id }, request);
            
            return BadRequest(new ApiBadRequestResponse("Create comment failed"));
        }

        [HttpPut("{knowledgeBaseId}/comments/{commentId}")]
        public async Task<IActionResult> PutComment(int commentId, [FromBody]CommentCreateRequest request)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found comment with id: {commentId}"));
            
            /*GET USER FROM CLAIM*/
            // if (comment.OwnerUserId != User.GetUserId())
            //     return Forbid();
            
            comment.Content = request.Content;
            _context.Comments.Update(comment);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return NoContent();
            
            return BadRequest(new ApiBadRequestResponse($"Update comment failed"));
        }

        [HttpDelete("{knowledgeId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int knowledgeId, int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found the comment with id: {commentId}"));

            _context.Comments.Remove(comment);

            var knowledge = await _context.Knowledges.FindAsync(knowledgeId);
            if (knowledge == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id: {knowledgeId}"));

            knowledge.NumberOfComments = knowledge.NumberOfVotes.GetValueOrDefault(0) - 1;
            _context.Knowledges.Update(knowledge);

            var result = await _context.SaveChangesAsync();
            if (result <= 0) 
                return BadRequest(new ApiBadRequestResponse($"Delete comment failed"));
            
            var commentVm = new CommentVm()
            {
                Id = comment.Id,
                Content = comment.Content,
                CreateDate = comment.CreateDate,
                KnowledgeId = comment.KnowledgeId,
                LastModifiedDate = comment.LastModifiedDate,
                OwnerUserId = comment.OwnerUserId
            };
            return Ok(commentVm);
        }
    }
}