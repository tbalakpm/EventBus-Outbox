using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UserService.Api.Context;
using UserService.Api.Entities;
using UserService.Api.Services;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserServiceContext _context;
        private readonly IntegrationEventSenderService _integrationEventSenderService;

        private readonly ILogger<UsersController> _logger;

        public UsersController(UserServiceContext context,
            IntegrationEventSenderService integrationEventSenderService,
            ILogger<UsersController> logger)
        {
            _context = context;
            _integrationEventSenderService = integrationEventSenderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            using var transaction = _context.Database.BeginTransaction();
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            var integrationEventData = JsonSerializer.Serialize(user);
            ///new
            ///{
            ///    id = user.Id,
            ///    newname = user.Name,
            ///}
            _context.IntegrationEvents.Add(
                new IntegrationEvent()
                {
                    Event = "user.update",
                    Data = integrationEventData
                });
            await _context.SaveChangesAsync();
            transaction.Commit();
            _integrationEventSenderService.StartPublishingOutstandingIntegrationEvents();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            using var transaction = _context.Database.BeginTransaction();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var integrationEventData = JsonSerializer.Serialize(user);
            ///new
            ///{
            ///    id = user.Id,
            ///    name = user.Name
            ///}
            _context.IntegrationEvents.Add(
                new IntegrationEvent()
                {
                    Event = "user.add",
                    Data = integrationEventData
                });
            await _context.SaveChangesAsync();
            transaction.Commit();
            _integrationEventSenderService.StartPublishingOutstandingIntegrationEvents();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
    }
}