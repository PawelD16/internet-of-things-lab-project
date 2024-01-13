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
    public class CardOwnersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CardOwnersController> _logger;

        public CardOwnersController(ILogger<CardOwnersController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CardOwners
        public async Task<IActionResult> Index()
        {
              return _context.CardOwners != null ? 
                          View(await _context.CardOwners.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.CardOwners'  is null.");
        }

        // GET: CardOwners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CardOwners == null)
            {
                return NotFound();
            }

            var cardOwner = await _context.CardOwners
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cardOwner == null)
            {
                return NotFound();
            }

            return View(cardOwner);
        }

        // GET: CardOwners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CardOwners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CreatedAt,Name")] CardOwner cardOwner)
        {
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                _context.Add(cardOwner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Log the error with details about the invalid model state
                _logger.LogError("Failed to create CardOwner record. Invalid model state.");
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
            return View(cardOwner);
        }

        // GET: CardOwners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CardOwners == null)
            {
                return NotFound();
            }

            var cardOwner = await _context.CardOwners.FindAsync(id);
            if (cardOwner == null)
            {
                return NotFound();
            }
            return View(cardOwner);
        }

        // POST: CardOwners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CreatedAt,Name")] CardOwner cardOwner)
        {
            ModelState.Clear();
            if (id != cardOwner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cardOwner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CardOwnerExists(cardOwner.Id))
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
            return View(cardOwner);
        }

        // GET: CardOwners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CardOwners == null)
            {
                return NotFound();
            }

            var cardOwner = await _context.CardOwners
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cardOwner == null)
            {
                return NotFound();
            }

            return View(cardOwner);
        }

        // POST: CardOwners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CardOwners == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CardOwners'  is null.");
            }
            var cardOwner = await _context.CardOwners.FindAsync(id);
            if (cardOwner != null)
            {
                _context.CardOwners.Remove(cardOwner);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CardOwnerExists(int id)
        {
          return (_context.CardOwners?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
