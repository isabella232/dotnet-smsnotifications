using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sinch.ServerSdk.Messaging.Models;
using TVQMANotifications.Data;
using TVQMANotifications.Models;
using TVQMANotifications.Services;

namespace TVQMANotifications.Controllers {
    [Produces("application/json")]
    public class SMSController : Controller{
        private readonly SMSSender _smsSender;
        private readonly ApplicationDbContext _dbContext;


        private readonly IConfiguration _configuration;

        public SMSController(ApplicationDbContext dbContext, SMSSender smsSender, IConfiguration configuration) {
            _smsSender = smsSender;
            _dbContext = dbContext;

            _configuration = configuration;
        }

        [Route("/SMS")]
        public async Task<OkResult> Post([FromBody] IncomingMessageEvent model) {
            var fromNumber = "+" + model.From.Endpoint;
            Subscriber subscriber = _dbContext.Subscribers.FirstOrDefault(m => m.Number == fromNumber);
            if (model.Message.Trim().ToLower() == "start" || model.Message.Trim().ToLower() == "unstop") {
                if (subscriber != null) {
                    subscriber = new Subscriber {
                        Number = fromNumber
                    };
                    _dbContext.Subscribers.Add(subscriber);
                    await _dbContext.SaveChangesAsync();
                }
                await _smsSender.SendSMS(new Message { MessageContent = _configuration["Sinch:WelcomeMessage"] } , subscriber);
                return Ok();
            }

            if (model.Message.Trim().ToLower() == "stop") {
                if (_dbContext.Subscribers.Any(m => m.Number == fromNumber)) {
                    _dbContext.Subscribers.Remove(_dbContext.Subscribers.First(m => m.Number == fromNumber));
                    await _dbContext.SaveChangesAsync();
                }
                //add subscribper

                return Ok();
            }
            await _smsSender.SendSMS(new Message { MessageContent = "Sorry, we only support Start and stopm if you have any questions please contact TVQMA" }, subscriber);
            return Ok();
        }
    }
}