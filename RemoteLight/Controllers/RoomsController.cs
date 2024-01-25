using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemoteLight.Data;
using RemoteLight.Models;

namespace RemoteLight.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(ILogger<RoomsController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Rooms.Include(r => r.Broker);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Broker)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            ViewData["IdBroker"] = new SelectList(_context.Brokers, "BrokerId", "IPAddress");
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AdditionalInformation,FkBrokerId")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Log the error with details about the invalid model state
                _logger.LogError("Failed to create Access record. Invalid model state.");
                foreach (var keyValuePair in ModelState)
                {
                    string key = keyValuePair.Key;
                    ModelStateEntry entry = keyValuePair.Value;

                    foreach (var error in entry.Errors)
                    {
                        string errorMessage = error.ErrorMessage;
						// Log the key and error message
						_logger.LogError($"Error in '{key}': {errorMessage}");
                    }
                }
            }
            ViewData["IdBroker"] = new SelectList(_context.Brokers, "BrokerId", "IPAddress", room.FkBrokerId);
            return View(room);
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["IdBroker"] = new SelectList(_context.Brokers, "BrokerId", "IPAddress", room.FkBrokerId);
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,AdditionalInformation,FkBrokerId")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdBroker"] = new SelectList(_context.Brokers, "BrokerId", "IPAddress", room.FkBrokerId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Rooms == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Broker)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Rooms == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Rooms'  is null.");
            }
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
          return (_context.Rooms?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
