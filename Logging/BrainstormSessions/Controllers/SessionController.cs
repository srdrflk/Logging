using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BrainstormSessions.Controllers
{
    public class SessionController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<SessionController> _logger;

        public SessionController(IBrainstormSessionRepository sessionRepository, ILogger<SessionController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (!id.HasValue)
            {
                _logger.LogWarning("Session ID was not provided, redirecting to Home Index.");
                return RedirectToAction(actionName: nameof(Index), controllerName: "Home");
            }

            _logger.LogDebug("Attempting to retrieve session with ID: {SessionId}", id.Value);

            var session = await _sessionRepository.GetByIdAsync(id.Value);
            if (session == null)
            {
                _logger.LogWarning("Session with ID: {SessionId} not found.", id.Value);
                return Content("Session not found.");
            }

            var viewModel = new StormSessionViewModel()
            {
                DateCreated = session.DateCreated,
                Name = session.Name,
                Id = session.Id
            };

            _logger.LogInformation("Successfully retrieved session with ID: {SessionId} and is showing details.", id.Value);
            return View(viewModel);
        }
    }
}
