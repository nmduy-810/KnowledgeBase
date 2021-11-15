using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data;
using KnowledgeBase.ViewModels.Systems.Command;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class CommandsController : BaseController
    {
        #region Property
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommandsController> _logger;
        #endregion Property

        #region Constructor
        public CommandsController(ApplicationDbContext context, ILogger<CommandsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        #endregion Constructor

        #region Method
        [HttpGet]
        public async Task<IActionResult> GetCommands()
        {
            var commands = _context.Commands;
            var commandVm = await commands.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
            return Ok(commandVm);
        }
        #endregion
    }
}