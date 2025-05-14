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
        public async Task<ActionResult<IEnumerable<EventDetails>>> GetEvents()
        {
            var events = await _eventContext.Events.ToListAsync();
            var eventDetails = new List<EventDetails>();
            foreach (var evt in events)
            {
                var tickets = await this.GetAllTicketsOfAnEvent((Guid)evt.Id!);
                eventDetails.Add(new EventDetails(evt, tickets.Count));
            }
            return Ok(eventDetails);
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDetails>> GetEvent(Guid? id)
        {
            var evt = await _eventContext.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound();
            }

            var tickets = await this.GetAllTicketsOfAnEvent((Guid)evt.Id!);
            return Ok(new EventDetails(evt, tickets.Count));
        }


        // GET: api/Events/5/tickets
        [HttpGet("{id}/tickets")]
        public async Task<ActionResult<IList<Ticket>>> GetTickets(Guid id)
        {
            var tickets = await this.GetAllTicketsOfAnEvent(id);
            return Ok(tickets);
        }

        private async Task<IList<Ticket>> GetAllTicketsOfAnEvent(Guid eventId)
        {
            return await _ticketContext.Tickets.Where(t => t.EventId == eventId).ToListAsync();
        }
    }
}
