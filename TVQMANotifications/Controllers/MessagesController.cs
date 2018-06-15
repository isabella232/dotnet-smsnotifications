using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sinch.ServerSdk;
using TVQMANotifications.Data;
using TVQMANotifications.Models;

namespace TVQMANotifications.Controllers {
    public class MessagesController : Controller {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context) {
            _context = context;
        }


        [Route("/dashboard")]
        [HttpGet]
        public IActionResult Dashboard()
        {
            var model = new DashboardModel();
            model.SubscriberCount = _context.Subscribers.Count();
            model.LastMessages = _context.Messages.OrderByDescending(m => m.DateSent).Take(10).ToList();
            model.Message = "";
            return View(model);
        }

        [Route("/dashboard")]
        [HttpPost]
        public async Task<IActionResult> Dashboard(DashboardModel model) {

            var smsApi = SinchFactory.CreateApiFactory("86e26d90-f372-457b-b3ae-16044eb50e3f", "3i6YiAiFi0KC7fw4DAhJSA==").CreateSmsApi();
            if (ModelState.IsValid)
            {
                await CreateAndSendMessage(new Message
                {
                    MessageContent =  model.Message,
                    DateSent =  DateTime.UtcNow
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

            var message = await _context.Messages.Include(m=> m.Logs).Include("Logs.Subscriber")
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
        public async Task<IActionResult> Create([Bind("MessageId,MessageContent,DateSent")] Message message)
        {

            if (ModelState.IsValid)
            {
                await CreateAndSendMessage(message);
            }
            return View(message);
        }

        private async Task CreateAndSendMessage(Message message)
        {
            var smsApi = SinchFactory.CreateApiFactory("86e26d90-f372-457b-b3ae-16044eb50e3f", "3i6YiAiFi0KC7fw4DAhJSA==")
                .CreateSmsApi();
            message.DateSent = DateTime.UtcNow;
           
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                var subscribers = await _context.Subscribers.ToListAsync();
                foreach (var s in subscribers)
                {
                    var messageid = await smsApi.Sms(s.Number,
                            message.MessageContent +
                            ".\n\nSms by Sinch")
                        .WithCli("+18442872483").Send();
                    _context.SendLogs.Add(new SendLog()
                    {
                        DateSent = DateTime.UtcNow,
                        MessageId = message.MessageId,
                        SubscriberId = s.SubscriberId,
                        SinchMessageId = messageid.MessageId.ToString()
                    });
                    await _context.SaveChangesAsync();
                }
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
        public async Task<IActionResult> Edit(int id, [Bind("MessageId,MessageContent,DateSent")] Message message) {
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
