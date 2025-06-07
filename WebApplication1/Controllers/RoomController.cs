using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Model;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return Ok(new
            {
                Message = "Get all rooms successful",
                Count = rooms.Count,
                Data = rooms
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound(new { Message = $"Room with ID {id} not found" });
            return Ok(new
            {
                Message = "Room found",
                Data = room
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid data", Errors = ModelState });
            }
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, new
            {
                Message = "Room created successfully",
                data = room
            });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] JsonElement updateData)
        {
            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null)
            {
                return NotFound(new { Message = $"Room with ID {id} not found" });
            }

            if (updateData.TryGetProperty("roomNumber", out var roomNumber))
            {
                existingRoom.RoomNumber = roomNumber.GetString() ?? existingRoom.RoomNumber;
            }

            if (updateData.TryGetProperty("roomType", out var roomType))
            {
                existingRoom.RoomType = roomType.GetString() ?? existingRoom.RoomType;
            }

            if (updateData.TryGetProperty("capacity", out var capacity))
            {
                existingRoom.Capacity = capacity.GetInt32();
            }

            if (updateData.TryGetProperty("price", out var price))
            {
                existingRoom.Price = price.GetDecimal();
            }

            if (updateData.TryGetProperty("description", out var description))
            {
                existingRoom.Description = description.GetString() ?? existingRoom.Description;
            }

            if (updateData.TryGetProperty("isAvailable", out var isAvailable))
            {
                existingRoom.IsAvailable = isAvailable.GetBoolean();
            }

            existingRoom.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Message = "Room updated successfully",
                    //Data = existingRoom
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Update failed", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoomComplete(int id, [FromBody] Room room)
        {
            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null)
            {
                return NotFound(new { Message = $"Room with ID {id} not found" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid data", Errors = ModelState });
            }

            existingRoom.RoomNumber = room.RoomNumber;
            existingRoom.RoomType = room.RoomType;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Price = room.Price;
            existingRoom.Description = room.Description;
            existingRoom.IsAvailable = room.IsAvailable;
            existingRoom.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "Room updated successfully",
                //Data = existingRoom
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound(new { Message = $"Room with ID {id} not found" });
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "Room deleted successfully",
                Data = room
            });
        }
    }
}