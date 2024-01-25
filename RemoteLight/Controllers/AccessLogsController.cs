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
    public class AccessLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccessLogsController> _logger;

        public AccessLogsController(ILogger<AccessLogsController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: AccessLogs
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = _context.AccessLogs.Include(a => a.RFIDCard);
            return View(await _context.AccessLogs.ToListAsync());
        }

        // GET: AccessLogs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.AccessLogs == null)
            {
                return NotFound();
            }

            var accessLog = await _context.AccessLogs
                //.Include(a => a.RFIDCard)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accessLog == null)
            {
                return NotFound();
            }

            return View(accessLog);
        }

        [Authorize]
        // GET: AccessLogs/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.AccessLogs == null)
            {
                return NotFound();
            }


            var accessLog = await _context.AccessLogs
               // .Include(a => a.RFIDCard)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accessLog == null)
            {
                return NotFound();
            }

            return View(accessLog);
        }

        [Authorize]
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
