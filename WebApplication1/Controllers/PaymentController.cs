using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Model;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayment()
        { 
            var payments = await _context.Payments.ToListAsync();
            if (payments == null || !payments.Any())
            {
                return NotFound("No payments found.");
            }

            return Ok(new
            {
                message = "Payments retrieved successfully.",
                data = payments
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound($"Payment with ID {id} not found.");
            }
            return Ok(new
            {
                message = "Payment retrieved successfully.",
                data = payment
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        { 
            if(ModelState.IsValid) return BadRequest(new { message = "Invalid payment data", errors = ModelState });
            _context.Payments.Add(payment);
            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.PaymentId }, new
                {
                    message = "Payment created successfully.",
                    data = payment
                });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Error creating payment.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return BadRequest("Payment ID mismatch.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid payment data", errors = ModelState });
            }
            _context.Entry(payment).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Payment updated successfully.",
                    data = payment
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound($"Payment with ID {id} not found.");
                throw;
            }
        }
    }
}
