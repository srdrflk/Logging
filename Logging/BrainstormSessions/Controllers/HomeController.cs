using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using BrainstormSessions.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BrainstormSessions.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IBrainstormSessionRepository sessionRepository, ILogger<HomeController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading the Index page...");
                var sessionList = await _sessionRepository.ListAsync();

                var model = sessionList.Select(session => new StormSessionViewModel()
                {
                    Id = session.Id,
                    DateCreated = session.DateCreated,
                    Name = session.Name,
                    IdeaCount = session.Ideas.Count
                }).ToList();

                _logger.LogInformation("Index page loaded successfully with {SessionCount} sessions.", model.Count);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the Index page.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        public class NewSessionModel
        {
            [Required]
            public string SessionName { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Index(NewSessionModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Index POST model state is invalid.");
                return BadRequest(ModelState);
            }

            try
            {
                var newSession = new BrainstormSession
                {
                    DateCreated = DateTimeOffset.Now,
                    Name = model.SessionName
                };

                await _sessionRepository.AddAsync(newSession);
                _logger.LogInformation("New session {SessionName} created successfully.", model.SessionName);

                return RedirectToAction(actionName: nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while trying to create a new session.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
