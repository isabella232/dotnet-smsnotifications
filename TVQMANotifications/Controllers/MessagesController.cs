using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TVQMANotifications.Data;
using TVQMANotifications.Models;
using TVQMANotifications.Services;


namespace TVQMANotifications.Controllers {
    [Authorize]
    public class MessagesController : Controller {
        private readonly ApplicationDbContext _context;
        private readonly SMSSender _smsSender;
        private readonly IConfiguration _configuration;

        public MessagesController(ApplicationDbContext context, SMSSender smsSender, IConfiguration configuration) {
            _context = context;
            _smsSender = smsSender;
            _configuration = configuration;
        }


        [Route("/dashboard")]
        [HttpGet]
        public IActionResult Dashboard() {
            var model = new DashboardModel();
            model.SubscriberCount = _context.Subscribers.Count();
            model.LastMessages = _context.Messages.OrderByDescending(m => m.DateSent).Take(10).ToList();
            model.Message = "";
            return View(model);
        }

        [Route("/dashboard")]
        [HttpPost]
        public async Task<IActionResult> Dashboard(DashboardModel model) {
            if (ModelState.IsValid) {
                await CreateAndSendMessage(new Message {
                    MessageContent = model.Message,
                    DateSent = DateTime.UtcNow
                });
            }

            model.SubscriberCount = _context.Subscribers.Count();
            model.LastMessages = _context.Messages.OrderByDescending(m => m.DateSent).Take(10).ToList();
            model.Message = "";
            return View(model);
        }

        // GET: Messages
        public async Task<IActionResult> Index() {
            return View(await _context.Messages.OrderByDescending(m => m.DateSent).ToListAsync());
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var message = await _context.Messages.Include(m => m.Logs).Include("Logs.Subscriber")
                .SingleOrDefaultAsync(m => m.MessageId == id);
            if (message == null) {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create() {
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MessageId,MessageContent,DateSent")]
            Message message) {
            if (ModelState.IsValid) {
                await CreateAndSendMessage(message);
            }

            return View(message);
        }

        private async Task CreateAndSendMessage(Message message) {
            message.DateSent = DateTime.UtcNow;
            _context.Add(message);
            await _context.SaveChangesAsync();
            var subscribers = await _context.Subscribers.ToListAsync();
            foreach (var s in subscribers) {
                await _smsSender.SendSMS(message, s);
            }
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var message = await _context.Messages.SingleOrDefaultAsync(m => m.MessageId == id);
            if (message == null) {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MessageId,MessageContent,DateSent")]
            Message message) {
            if (id != message.MessageId) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!MessageExists(message.MessageId)) {
                        return NotFound();
                    } else {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(message);
        }

        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var message = await _context.Messages
                .SingleOrDefaultAsync(m => m.MessageId == id);
            if (message == null) {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var message = await _context.Messages.SingleOrDefaultAsync(m => m.MessageId == id);
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id) {
            return _context.Messages.Any(e => e.MessageId == id);
        }
    }
}