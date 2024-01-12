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
    public class AccessLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccessLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AccessLogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AccessLogs.Include(a => a.RFIDCard);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AccessLogs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.AccessLogs == null)
            {
                return NotFound();
            }

            var accessLog = await _context.AccessLogs
                .Include(a => a.RFIDCard)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accessLog == null)
            {
                return NotFound();
            }

            return View(accessLog);
        }

        // GET: AccessLogs/Create
        public IActionResult Create()
        {
            ViewData["RFIDCardId"] = new SelectList(_context.RFIDCards, "Id", "Id");
            return View();
        }

        // POST: AccessLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CreatedAt,Data,RFIDCardId")] AccessLog accessLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(accessLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RFIDCardId"] = new SelectList(_context.RFIDCards, "Id", "Id", accessLog.RFIDCardId);
            return View(accessLog);
        }

        // GET: AccessLogs/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.AccessLogs == null)
            {
                return NotFound();
            }

            var accessLog = await _context.AccessLogs.FindAsync(id);
            if (accessLog == null)
            {
                return NotFound();
            }
            ViewData["RFIDCardId"] = new SelectList(_context.RFIDCards, "Id", "Id", accessLog.RFIDCardId);
            return View(accessLog);
        }

        // POST: AccessLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,CreatedAt,Data,RFIDCardId")] AccessLog accessLog)
        {
            if (id != accessLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accessLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccessLogExists(accessLog.Id))
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
            ViewData["RFIDCardId"] = new SelectList(_context.RFIDCards, "Id", "Id", accessLog.RFIDCardId);
            return View(accessLog);
        }

        // GET: AccessLogs/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.AccessLogs == null)
            {
                return NotFound();
            }

            var accessLog = await _context.AccessLogs
                .Include(a => a.RFIDCard)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accessLog == null)
            {
                return NotFound();
            }

            return View(accessLog);
        }

        // POST: AccessLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.AccessLogs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AccessLogs'  is null.");
            }
            var accessLog = await _context.AccessLogs.FindAsync(id);
            if (accessLog != null)
            {
                _context.AccessLogs.Remove(accessLog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccessLogExists(long id)
        {
          return (_context.AccessLogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
