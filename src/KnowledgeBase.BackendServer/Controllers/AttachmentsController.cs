using System.Threading.Tasks;
using KnowledgeBase.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using KnowledgeBase.ViewModels.Contents.Attachment;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Controllers
{
    public partial class KnowledgesController
    {
        #region Attachments

        [HttpGet("{knowledgeId}/attachments")]
        public async Task<IActionResult> GetAttachment(int knowledgeId)
        {
            var query = await _context.Attachments
                .Where(x => x.KnowledgeId == knowledgeId)
                .Select(c => new AttachmentVm()
                {
                    Id = c.Id,
                    LastModifiedDate = c.LastModifiedDate,
                    CreateDate = c.CreateDate,
                    FileName = c.FileName,
                    FilePath = c.FilePath,
                    FileSize = c.FileSize,
                    FileType = c.FileType,
                    KnowledgeId = c.KnowledgeId
                }).ToListAsync();

            return Ok(query);
        }

        [HttpDelete("{knowledgeId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found attachment with id {attachmentId}"));

            _context.Attachments.Remove(attachment);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok();
            
            return BadRequest(new ApiBadRequestResponse($"Delete attachment failed"));
        }

        #endregion Attachments
    }
}