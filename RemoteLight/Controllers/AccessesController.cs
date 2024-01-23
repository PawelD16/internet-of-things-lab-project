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
    public class AccessesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccessesController> _logger;

        public AccessesController(ILogger<AccessesController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Accesses
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Accesses.Include(a => a.RFIDCard).Include(a => a.Room);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Accesses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accesses == null)
            {
                return NotFound();
            }

            var access = await _context.Accesses
                .Include(a => a.RFIDCard)
                .Include(a => a.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (access == null)
            {
                return NotFound();
            }

            return View(access);
        }

        // GET: Accesses/Create
        public IActionResult Create()
        {
            var chuj = new SelectList(_context.RFIDCards, "Id", "Id");
            ViewData["RFIDId"] = chuj;
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id");
            return View();
        }

        // POST: Accesses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FkRoomId,FkRFIDId")] Access access)
        {
            if (ModelState.IsValid)
            {
                _context.Add(access);
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
            ViewData["RFIDId"] = new SelectList(_context.RFIDCards, "Id", "Id", access.FkRFIDCardId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", access.FkRoomId);
            return View(access);
        }

        // GET: Accesses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accesses == null)
            {
                return NotFound();
            }

            var access = await _context.Accesses.FindAsync(id);
            if (access == null)
            {
                return NotFound();
            }
            ViewData["RFIDId"] = new SelectList(_context.RFIDCards, "Id", "Id", access.FkRFIDCardId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", access.FkRoomId);
            return View(access);
        }

        // POST: Accesses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FkRoomId,FkRFIDId")] Access access)
        {
            if (id != access.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(access);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccessExists(access.Id))
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
            ViewData["RFIDId"] = new SelectList(_context.RFIDCards, "Id", "Id", access.FkRFIDCardId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", access.FkRoomId);
            return View(access);
        }

        // GET: Accesses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accesses == null)
            {
                return NotFound();
            }

            var access = await _context.Accesses
                .Include(a => a.RFIDCard)
                .Include(a => a.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (access == null)
            {
                return NotFound();
            }

            return View(access);
        }

        // POST: Accesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accesses == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Accesses'  is null.");
            }
            var access = await _context.Accesses.FindAsync(id);
            if (access != null)
            {
                _context.Accesses.Remove(access);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccessExists(int id)
        {
          return (_context.Accesses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
