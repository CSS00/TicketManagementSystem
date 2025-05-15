using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly EventContext _eventContext;
        private readonly TicketContext _ticketContext;
        private readonly UserContext _userContext;
        private readonly ReservationContext _reservationContext;


        public AdminController(EventContext eventContext, TicketContext ticketContext, UserContext userContext, ReservationContext reservationContext)
        {
            _eventContext = eventContext;
            _ticketContext = ticketContext;
            _userContext = userContext;
            _reservationContext = reservationContext;
        }
        
        // PUT: api/Events/5 -- Update an event
        [HttpPut("events/{id}")]
        [Authorize]
        public async Task<IActionResult> PutEvent(Guid? id, Event @event)
        {
            bool hasPermission = await this.HasPermission();
            if (!hasPermission)
            {
                return Forbid();
            }

            if (id != @event.Id)
            {
                return BadRequest();
            }

            _eventContext.Entry(@event).State = EntityState.Modified;

            try
            {
                await _eventContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_eventContext.EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Events  -- Create an event
        [HttpPost("events")]
        [Authorize]
        public async Task<ActionResult<Event>> PostEvent([FromBody] Event @event)
        {
            bool hasPermission = await this.HasPermission();
            if (!hasPermission)
            {
                return Forbid();
            }

            _eventContext.Events.Add(@event);
            await _eventContext.SaveChangesAsync();

            return Ok(@event);
        }

        // DELETE: api/Events/5 -- Delete an event
        [HttpDelete("events/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(Guid? id)
        {
            bool hasPermission = await this.HasPermission();
            if (!hasPermission)
            {
                return Forbid();
            }

            var @event = await _eventContext.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            List<Reservation> resvs = new List<Reservation>(_reservationContext.Reservations.Where(r => r.EventId == @event.Id));
            foreach (Reservation resv in resvs)
            {
                // cannot delete the event if it is on an active/confirmed reservation
                if (resv.Status == ReservationStatus.Active && !resv.Expired())
                {
                    return BadRequest("Cannot delete the event. There are active reservations on this event.");
                }
                if (resv.Status == ReservationStatus.Confirmed)
                {
                    return BadRequest("Cannot delete the event(booked). There are confirmed reservations on this event.");
                }
            }

            _eventContext.Events.Remove(@event);
            await _eventContext.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Tickets/5 -- Update an ticket
        [HttpPut("tickets/{id}")]
        [Authorize]
        public async Task<IActionResult> PutTicket(Guid? id, Ticket ticket)
        {
            bool hasPermission = await this.HasPermission();
            if (!hasPermission)
            {
                return Forbid();
            }

            var originalTicket = await _ticketContext.Tickets.FindAsync(id);
            if (originalTicket == null)
            {
                return NotFound();
            }
            else if (originalTicket.EventId != ticket.EventId)
            {
                return BadRequest("Event id cannot be changed.");
            }

            if (id != ticket.Id)
            {
                return BadRequest();
            }

            _ticketContext.Entry(ticket).State = EntityState.Modified;

            try
            {
                await _ticketContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_ticketContext.TicketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tickets -- Create an ticket
        [HttpPost("tickets")]
        [Authorize]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            bool hasPermission = await this.HasPermission();
            if (!hasPermission)
            {
                return Forbid();
            }

            if (!_eventContext.EventExists(ticket.EventId))
            {
                return NotFound("Event not found!");
            }

            _ticketContext.Tickets.Add(ticket);
            await _ticketContext.SaveChangesAsync();

            return Ok(ticket);
        }

        // POST: api/Tickets -- Create an ticket
        [HttpPost("tickets/batch")]
        [Authorize]
        public async Task<ActionResult<Ticket>> PostTickets(TicketBatch ticketBatch)
        {
            bool hasPermission = await this.HasPermission();
            if (!hasPermission)
            {
                return Forbid();
            }

            if (!_eventContext.EventExists(ticketBatch.EventId))
            {
                return NotFound("Event not found!");
            }

            List<Ticket> newTickets = new List<Ticket>();
            foreach (var group in ticketBatch.TicketGroups)
            {
                if (!group.Rows.IsValid())
                {
                    return BadRequest("Invalid row range in ticket group.");
                }

                if (!group.SeatNumbers.IsValid())
                {
                    return BadRequest("Invalid seat number range in ticket group.");
                }

                for (char row = group.Rows.From[0]; row <= group.Rows.To[0]; row++)
                {
                    int seatFrom = int.Parse(group.SeatNumbers.From);
                    int seatTo = int.Parse(group.SeatNumbers.To);
                    for (int seat = seatFrom; seat <= seatTo; seat++)
                    {
                        var ticket = new Ticket(ticketBatch.EventId, group.Price, group.Section, row.ToString(), seat.ToString());
                        newTickets.Add(ticket);
                        
                    }
                }
            }

            _ticketContext.Tickets.AddRange(newTickets);
            await _ticketContext.SaveChangesAsync();
            return Ok(newTickets);
        }

        // DELETE: api/Tickets/5 -- Delete an ticket
        [HttpDelete("tickets/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTicket(Guid? id)
        {
            bool hasPermission = await this.HasPermission();
            if (!hasPermission)
            {
                return Forbid();
            }

            var ticket = await _ticketContext.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            if (ticket.ReservationId != null)
            {
                // cannot delete the ticket if it is on an active/confirmed reservation
                Reservation? resv = await _reservationContext.Reservations.FindAsync(ticket.ReservationId);
                if (resv != null)
                {
                    if (resv.Status == ReservationStatus.Active && !resv.Expired())
                    {
                        return BadRequest("Cannot delete the ticket(reserved).");
                    }
                    if (resv.Status == ReservationStatus.Confirmed)
                    {
                        return BadRequest("Cannot delete the ticket(booked).");
                    }
                }
            }

            _ticketContext.Tickets.Remove(ticket);
            await _ticketContext.SaveChangesAsync();

            return NoContent();
        }
   
        private async Task<bool> HasPermission()
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            Guid userGuid = Guid.Parse(userId);
            if (userGuid == Guid.Empty)
            {
                return false;
            }

            User? user = await _userContext.Users.FindAsync(userGuid);
            if (user == null)
            {
                return false;
            }

            return user.Role == Role.Admin;
        }
   }

    public class TicketBatch
    {
        public Guid EventId { get; set; }
        public List<TicketGroup> TicketGroups { get; set; }
        public TicketBatch(Guid eventId, List<TicketGroup> ticketGroups)
        {
            EventId = eventId;
            TicketGroups = ticketGroups;
        }
    }

    public class TicketGroup
    {
        public string Section { get; set; }
        public RowRange Rows { get; set; }
        public SeatNumberRange SeatNumbers { get; set; }
        public decimal Price { get; set; }
        public TicketGroup(string section, RowRange rows, SeatNumberRange seatNumbers, decimal price)
        {
            Section = section;
            Rows = rows;
            SeatNumbers = seatNumbers;
            Price = price;
        }
    }

    public abstract class Range
    {
        public string From { get; set; }
        public string To { get; set; }
        public Range (string from, string to)
        {
            From = from;
            To = to;
        }
        public abstract bool IsValid();
    }

    public class RowRange : Range
    {
        public RowRange(string from, string to):base(from.ToUpperInvariant(), to.ToUpperInvariant())
        {
        }

        public override bool IsValid() {
            bool invalid = string.IsNullOrEmpty(From) || string.IsNullOrEmpty(To) ||
                           From.Length != 1 || To.Length != 1 || From[0] > To[0];
            return !invalid;
        }
    }

    public class SeatNumberRange : Range
    {
        public SeatNumberRange(string from, string to):base(from, to)
        {
        }
        
        public override bool IsValid() {
            bool invalid = !int.TryParse(From, out int seatFrom) ||
                           !int.TryParse(To, out int seatTo) ||
                           seatFrom > seatTo;
            return !invalid;
        }
    }
}