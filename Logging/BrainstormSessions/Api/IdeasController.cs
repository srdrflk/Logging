using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrainstormSessions.ClientModels;
using BrainstormSessions.Controllers;
using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BrainstormSessions.Api
{
    public class IdeasController : ControllerBase
    {
        private readonly IBrainstormSessionRepository _sessionRepository;
        private readonly ILogger<IdeasController> _logger;

        public IdeasController(IBrainstormSessionRepository sessionRepository, ILogger<IdeasController> logger)
        {
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        #region snippet_ForSessionAndCreate
        [HttpGet("forsession/{sessionId}")]
        public async Task<IActionResult> ForSession(int sessionId)
        {
            _logger.LogInformation("Attempting to retrieve session with ID: {SessionId}", sessionId);

            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Session with ID: {SessionId} not found.", sessionId);
                return NotFound(sessionId);
            }

            var result = session.Ideas.Select(idea => new IdeaDTO()
            {
                Id = idea.Id,
                Name = idea.Name,
                Description = idea.Description,
                DateCreated = idea.DateCreated
            }).ToList();

            _logger.LogInformation("Retrieved {Count} ideas for session with ID: {SessionId}", result.Count, sessionId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody]NewIdeaModel model)
        {
            _logger.LogDebug("Received a request to create a new idea.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create idea request model state is invalid.");
                return BadRequest(ModelState);
            }

            var session = await _sessionRepository.GetByIdAsync(model.SessionId);
            if (session == null)
            {
                _logger.LogWarning("Session with ID: {SessionId} not found for creating idea.", model.SessionId);
                return NotFound(model.SessionId);
            }

            var idea = new Idea()
            {
                DateCreated = DateTimeOffset.Now,
                Description = model.Description,
                Name = model.Name
            };
            session.AddIdea(idea);

            try
            {
                await _sessionRepository.UpdateAsync(session);
                _logger.LogInformation("New idea '{IdeaName}' added to session with ID: {SessionId}", model.Name, model.SessionId);
                return Ok(session);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add idea to session with ID: {SessionId}", model.SessionId);
                return StatusCode(500, "An error occurred while creating the idea.");
            }
        }
        #endregion

        #region snippet_ForSessionActionResult
        [HttpGet("forsessionactionresult/{sessionId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<IdeaDTO>>> ForSessionActionResult(int sessionId)
        {
            _logger.LogDebug("Request to retrieve ideas for session with ID: {SessionId}", sessionId);

            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Session with ID: {SessionId} not found.", sessionId);
                return NotFound(sessionId);
            }

            var result = session.Ideas.Select(idea => new IdeaDTO()
            {
                Id = idea.Id,
                Name = idea.Name,
                Description = idea.Description,
                DateCreated = idea.DateCreated
            }).ToList();

            _logger.LogInformation("Fetched {Count} ideas for session with ID: {SessionId}.", result.Count, sessionId);
            return result;
        }
        #endregion

        #region snippet_CreateActionResult
        [HttpPost("createactionresult")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BrainstormSession>> CreateActionResult([FromBody]NewIdeaModel model)
        {
            _logger.LogDebug("Received request to create a new idea for session ID: {SessionId}", model.SessionId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create idea request model state is invalid for session ID: {SessionId}", model.SessionId);
                return BadRequest(ModelState);
            }

            var session = await _sessionRepository.GetByIdAsync(model.SessionId);
            if (session == null)
            {
                _logger.LogWarning("Session with ID: {SessionId} not found during create idea attempt.", model.SessionId);
                return NotFound(model.SessionId);
            }

            var idea = new Idea()
            {
                DateCreated = DateTimeOffset.Now,
                Description = model.Description,
                Name = model.Name
            };
            session.AddIdea(idea);

            try
            {
                await _sessionRepository.UpdateAsync(session);
                _logger.LogInformation("New idea '{IdeaName}' successfully added to session with ID: {SessionId}", idea.Name, model.SessionId);
                return CreatedAtAction(nameof(CreateActionResult), new { id = session.Id }, session);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update session with new idea; Session ID: {SessionId}", model.SessionId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        #endregion
    }
}
