using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IoT_Project_Light_System.Data;
using IoT_Project_Light_System.Models;

namespace IoT_Project_Light_System.Controllers
{
    public class RFIDCardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RFIDCardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RFIDCards
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RFIDCards.Include(r => r.User);
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
                .Include(r => r.User)
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: RFIDCards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CreatedAt,Data,UserId")] RFIDCard rFIDCard)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rFIDCard);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", rFIDCard.UserId);
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", rFIDCard.UserId);
            return View(rFIDCard);
        }

        // POST: RFIDCards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,CreatedAt,Data,UserId")] RFIDCard rFIDCard)
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", rFIDCard.UserId);
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
                .Include(r => r.User)
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
