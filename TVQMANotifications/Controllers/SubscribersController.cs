using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVQMANotifications.Data;
using TVQMANotifications.Models;

namespace TVQMANotifications.Controllers {
    [Authorize]
    public class SubscribersController : Controller{
        private readonly ApplicationDbContext _context;

        public SubscribersController(ApplicationDbContext context){
            _context = context;
        }

        // GET: Subscribers
        public async Task<IActionResult> Index(){
            return View(await _context.Subscribers.ToListAsync());
        }

        // GET: Subscribers/Details/5
        public async Task<IActionResult> Details(int? id){
            if (id == null){
                return NotFound();
            }

            var subscriber = await _context.Subscribers
                .SingleOrDefaultAsync(m => m.SubscriberId == id);
            if (subscriber == null){
                return NotFound();
            }

            return View(subscriber);
        }

        // GET: Subscribers/Create
        public IActionResult Create(){
            return View();
        }

        // POST: Subscribers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubscriberId,Number")] Subscriber subscriber){
            if (ModelState.IsValid){
                _context.Add(subscriber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(subscriber);
        }

        // GET: Subscribers/Edit/5
        public async Task<IActionResult> Edit(int? id){
            if (id == null){
                return NotFound();
            }

            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(m => m.SubscriberId == id);
            if (subscriber == null){
                return NotFound();
            }

            return View(subscriber);
        }

        // POST: Subscribers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubscriberId,Number")] Subscriber subscriber){
            if (id != subscriber.SubscriberId){
                return NotFound();
            }

            if (ModelState.IsValid){
                try{
                    _context.Update(subscriber);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException){
                    if (!SubscriberExists(subscriber.SubscriberId)){
                        return NotFound();
                    }
                    else{
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(subscriber);
        }

        // GET: Subscribers/Delete/5
        public async Task<IActionResult> Delete(int? id){
            if (id == null){
                return NotFound();
            }

            var subscriber = await _context.Subscribers
                .SingleOrDefaultAsync(m => m.SubscriberId == id);
            if (subscriber == null){
                return NotFound();
            }

            return View(subscriber);
        }

        // POST: Subscribers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id){
            var subscriber = await _context.Subscribers.SingleOrDefaultAsync(m => m.SubscriberId == id);
            _context.Subscribers.Remove(subscriber);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubscriberExists(int id){
            return _context.Subscribers.Any(e => e.SubscriberId == id);
        }
    }
}