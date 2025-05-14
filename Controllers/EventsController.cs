using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly EventContext _eventContext;
        private readonly TicketContext _ticketContext;

        public EventsController(EventContext eventContext, TicketContext ticketContext)
        {
            _eventContext = eventContext;
            _ticketContext = ticketContext;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _eventContext.Events.ToListAsync();
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(Guid? id)
        {
            var @event = await _eventContext.Events.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }


        // GET: api/Events/5/tickets
        [HttpGet("{id}/tickets")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets(Guid id)
        {
            return await _ticketContext.Tickets.Where(t => t.EventId == id).ToListAsync();
        }
    }
}
