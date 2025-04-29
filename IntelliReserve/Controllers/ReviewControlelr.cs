using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;

namespace IntelliReserve.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = _context.Reviews.Include(r => r.User).Include(r => r.Business);
            return View(await reviews.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews.Include(r => r.User).Include(r => r.Business).FirstOrDefaultAsync(r => r.Id == id);
            if (review == null) return NotFound();
            return View(review);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,BusinessId,Rating,Comment,CreatedAt")] Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,BusinessId,Rating,Comment,CreatedAt")] Review review)
        {
            if (id != review.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews.Include(r => r.User).Include(r => r.Business).FirstOrDefaultAsync(r => r.Id == id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
