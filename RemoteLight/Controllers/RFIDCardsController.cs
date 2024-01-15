using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RemoteLight.Data;
using RemoteLight.Models;

namespace RemoteLight.Controllers
{
    public class RFIDCardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RFIDCardsController> _logger;

        public RFIDCardsController(ILogger<RFIDCardsController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: RFIDCards
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RFIDCards.Include(r => r.CardOwner);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: RFIDCards/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.RFIDCards == null)
            {
                return NotFound();
            }

            var rFIDCard = await _context.RFIDCards
                .Include(r => r.CardOwner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rFIDCard == null)
            {
                return NotFound();
            }

            return View(rFIDCard);
        }

        // GET: RFIDCards/Create
        public IActionResult Create()
        {
            ViewData["CardOwnerId"] = new SelectList(_context.CardOwners, "Id", "Id");
            return View();
        }

        // POST: RFIDCards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CardOwnerId")] RFIDCard rFIDCard)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rFIDCard);
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
            ViewData["CardOwnerId"] = new SelectList(_context.CardOwners, "Id", "Id", rFIDCard.CardOwnerId);
            return View(rFIDCard);
        }

        // GET: RFIDCards/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.RFIDCards == null)
            {
                return NotFound();
            }

            var rFIDCard = await _context.RFIDCards.FindAsync(id);
            if (rFIDCard == null)
            {
                return NotFound();
            }
            ViewData["CardOwnerId"] = new SelectList(_context.CardOwners, "Id", "Id", rFIDCard.CardOwnerId);
            return View(rFIDCard);
        }

        // POST: RFIDCards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,CardOwnerId")] RFIDCard rFIDCard)
        {
            if (id != rFIDCard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rFIDCard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RFIDCardExists(rFIDCard.Id))
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
            ViewData["CardOwnerId"] = new SelectList(_context.CardOwners, "Id", "Id", rFIDCard.CardOwnerId);
            return View(rFIDCard);
        }

        // GET: RFIDCards/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.RFIDCards == null)
            {
                return NotFound();
            }

            var rFIDCard = await _context.RFIDCards
                .Include(r => r.CardOwner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rFIDCard == null)
            {
                return NotFound();
            }

            return View(rFIDCard);
        }

        // POST: RFIDCards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.RFIDCards == null)
            {
                return Problem("Entity set 'ApplicationDbContext.RFIDCards'  is null.");
            }
            var rFIDCard = await _context.RFIDCards.FindAsync(id);
            if (rFIDCard != null)
            {
                _context.RFIDCards.Remove(rFIDCard);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RFIDCardExists(string id)
        {
          return (_context.RFIDCards?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
