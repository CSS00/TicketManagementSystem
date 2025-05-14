using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TicketManagementSystem.Models;
using TicketManagementSystem.Services;
using TicketManagementSystem.Utils;

namespace TicketManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly EventContext _eventContext;
        private readonly TicketContext _ticketContext;
        private readonly ReservationContext _reservationContext;
        private readonly PaymentService paymentService = new PaymentService();
        public ReservationsController(
            EventContext eventContext,
            TicketContext ticketContext,
            ReservationContext reservationContext)
        {
            _eventContext = eventContext;
            _ticketContext = ticketContext;
            _reservationContext = reservationContext;
        }

        // Get reservation details by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDetails>> GetReservationAsync([FromRoute] Guid id)
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }

            Guid userGuid = Guid.Parse(userId);
            if (userGuid == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            if (id == Guid.Empty)
            {
                return BadRequest("Invalid reservation ID.");
            }

            Reservation? reservation = await _reservationContext.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation {id} is not found.");
            }
            if (reservation.UserId != userGuid)
            {
                return Unauthorized($"User does not have access to the reservation.");
            }

            List<Ticket> tickets = _ticketContext.Tickets.Where(t => reservation.TicketIds.Contains((Guid)t.Id)).ToList();
            if (tickets.Count != reservation.TicketIds.Count)
            {
                return Problem("Some tickets in the reservation are not found.");
            }

            ReservationDetails details = new ReservationDetails(reservation, tickets);

            return Ok(details);
        }

        // Get all reservations of a user
        [HttpGet()]
        [Authorize]
        public async Task<ActionResult<List<ReservationDetails>>> GetUserReservationsAsync()
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }
            Guid userGuid = Guid.Parse(userId);

            if (userGuid == Guid.Empty)
            {
                return BadRequest("Invalid user ID.");
            }

            var reservations = new List<Reservation>();
            reservations.AddRange(
                _reservationContext.Reservations
                .Where(r => r.UserId == userGuid)
                .ToList<Reservation>()
            );

            var reservationDetailsList = new List<ReservationDetails>();
            foreach (var reservation in reservations)
            {
                var tickets = _ticketContext.Tickets
                    .Where(t => reservation.TicketIds.Contains((Guid)t.Id))
                    .ToList();

                if (tickets.Count != reservation.TicketIds.Count)
                {
                    return Problem("Some tickets in the reservation are not found.");
                }

                reservationDetailsList.Add(new ReservationDetails(reservation, tickets));
            }

            return Ok(reservationDetailsList);
        }

        // Reserve a list of tickets and return a reservation object
        [HttpPost()]
        [Authorize]
        public async Task<ActionResult<Reservation>> ReserveTicketsAsync([FromBody] CreateReservationRequest request)
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }
            Guid userGuid = Guid.Parse(userId);

            if (request.TicketIds == null || request.TicketIds.Count == 0)
            {
                return BadRequest("Ticket IDs cannot be null or empty.");
            }

            // check if tickets are available to reserve
            // if any of them are not available return 400
            List<Ticket> tickets = [];
            foreach (Guid id in request.TicketIds)
            {
                Ticket? ticket = await _ticketContext.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return BadRequest($"Ticket {id} is not found.");
                }
                if (ticket.EventId != request.EventId)
                {
                    return BadRequest($"Bad input! Tickets and event have to match.");
                }
                bool isAvailable = await IsTicketAvailableForReserve(ticket, userGuid);
                if (!isAvailable)
                {
                    return BadRequest($"Ticket {id} is not available.");
                }
                else
                {
                    tickets.Add(ticket);
                }
            }

            // if they are available, update them with the new reservation id and create the reservation & save
            var reservation = new Reservation(request.EventId, userGuid, request.TicketIds);
            _reservationContext.Reservations.Add(reservation);
            tickets.ForEach(t => {t.ReservationId = reservation.Id;});
            await _ticketContext.SaveChangesAsync();
            await _reservationContext.SaveChangesAsync();
            return Ok(reservation);
        }

        // Confirm a reservation with reservation ID and payment information, and return a booking object
        [HttpPost("{id}/confirm")]
        [Authorize]
        public async Task<ActionResult<Reservation>> ConfirmReservationAsync([FromRoute] Guid id, [FromBody] ConfirmReservationRequest request)
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }
            Guid userGuid = Guid.Parse(userId);

            if (request == null || id == Guid.Empty)
            {
                return BadRequest("Invalid reservation confirmation request.");
            }
            
            Reservation? reservation = await _reservationContext.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation {id} is not found.");
            }

            if (reservation.UserId != userGuid)
            {
                return Unauthorized($"User does not have access to the reservation.");
            }

            if (reservation.Status == ReservationStatus.Confirmed)
            {
                return Ok(reservation);
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return BadRequest("The reservation was canceled.");
            }

            bool expired = reservation.Expired();
            if (reservation.Status == ReservationStatus.Expired || expired)
            {
                reservation.Status = ReservationStatus.Expired;
                await _reservationContext.SaveChangesAsync();
                return BadRequest("The reservation has expired.");
            }

            // Calculate total payment using ticket information from the reservation
            var tickets = _ticketContext.Tickets.Where(t => reservation.TicketIds.Contains((Guid)t.Id)).ToList();
            if (tickets.Count != reservation.TicketIds.Count)
            {
                return BadRequest("Some tickets in the reservation are not found.");
            }
            decimal totalPayment = tickets.Sum(t => t.Price);

            try {
                paymentService.ProcessPayment(totalPayment, request.PaymentInfo);
            } catch(Exception ex) {
                reservation.Status = ReservationStatus.Active;
                return Problem($"Failed to process payment {ex.Message}.");
            }

            reservation.ConfirmedAt = DateTime.UtcNow;
            reservation.Status = ReservationStatus.Confirmed;
            await _reservationContext.SaveChangesAsync();
            return Ok(reservation);
        }

        // Cancel a reservation with reservation ID
        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<ActionResult> CancelReservationAsync([FromRoute] Guid id)
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid();
            }
            Guid userGuid = Guid.Parse(userId);

            if (id == Guid.Empty)
            {
                return BadRequest("Invalid reservation ID.");
            }

            Reservation? reservation = await _reservationContext.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation {id} is not found.");
            }

            if (reservation.UserId != userGuid)
            {
                return Unauthorized($"User does not have access to the reservation.");
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return BadRequest("The reservation is already canceled.");
            }

            if (reservation.Status == ReservationStatus.Confirmed)
            {
                return BadRequest("Cannot cancel a confirmed reservation.");
            }

            // Reset reservation-related information in the tickets
            var tickets = _ticketContext.Tickets.Where(t => reservation.TicketIds.Contains((Guid)t.Id)).ToList();
            if (tickets.Count != reservation.TicketIds.Count)
            {
                return BadRequest("Some tickets in the reservation are not found.");
            }

            tickets.ForEach(t => t.ReservationId = null);

            reservation.Status = ReservationStatus.Cancelled;
            await _ticketContext.SaveChangesAsync();
            await _reservationContext.SaveChangesAsync();

            return Ok("Reservation canceled successfully.");
        }

        public async Task<bool> IsTicketAvailableForReserve(Ticket ticket, Guid userId)
        {
            // ticket is not found
            if (ticket == null)
            {
                return false;
            }
            
            // ticket is not reserved by anyone
            if (ticket.ReservationId == null)
            {
                return true;
            }

            Reservation? reservation = await _reservationContext.Reservations.FindAsync(ticket.ReservationId);
            if (reservation == null || reservation.Status == ReservationStatus.Expired || reservation.Status == ReservationStatus.Cancelled)
            {
                return true;
            }
            else if (reservation.Status == ReservationStatus.Confirmed)
            {
                return false;
            }
            else
            {
                // correlated with a acitve reservation
                if (reservation.UserId != userId)
                {
                    // from a different user
                    if (!reservation.Expired())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    // from the same user
                    if (!reservation.Expired())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
    }

    public class CreateReservationRequest
    {
        public Guid EventId { get; set; }
        public List<Guid> TicketIds { get; set; }
        public CreateReservationRequest(Guid eventId, List<Guid> ticketIds)
        {
            EventId = eventId;
            TicketIds = ticketIds;
        }
    }

    public class ConfirmReservationRequest
    {
        public Payment PaymentInfo { get; set; }
        public ConfirmReservationRequest(Guid userId, Payment paymentInfo)
        {
            PaymentInfo = paymentInfo;
        }
    }
}