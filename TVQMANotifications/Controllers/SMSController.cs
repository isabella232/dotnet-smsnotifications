using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sinch.ServerSdk;
using Sinch.ServerSdk.Calling.Models;
using Sinch.ServerSdk.Messaging.Models;
using Sinch.ServerSdk.Models;
using TVQMANotifications.Data;
using TVQMANotifications.Models;
using TVQMANotifications.Services;

namespace TVQMANotifications.Controllers
{


   
    [Produces("application/json")]
    public class SMSController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public SMSController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("/SMS")]
        public async Task<OkResult> Post([FromBody] IncomingMessageEvent model) {

            var smsApi = SinchFactory.CreateApiFactory("86e26d90-f372-457b-b3ae-16044eb50e3f", "3i6YiAiFi0KC7fw4DAhJSA==").CreateSmsApi();
            var fromNumber = "+" + model.From.Endpoint;
            if (model.Message.Trim().ToLower() == "start" || model.Message.Trim().ToLower() == "unstop")
            {
                if (!_dbContext.Subscribers.Any(m=> m.Number == fromNumber))
                {
                    _dbContext.Subscribers.Add(new Subscriber
                    {
                        Number = fromNumber
                    });
                    await _dbContext.SaveChangesAsync();
                }
                //add subscribper
                await smsApi.Sms(fromNumber,
                        "Thank you! \nYou are now subscribed to Western Grands 2018 Notifications.\n\nSms by Sinch https://www.sinch.com/")
                    .WithCli("+18442872483").Send();
                return Ok();
            }
            if (model.Message.Trim().ToLower() == "stop") {
                if (_dbContext.Subscribers.Any(m => m.Number == fromNumber)) {
                    _dbContext.Subscribers.Remove(_dbContext.Subscribers.First(m=> m.Number == fromNumber));
                    await _dbContext.SaveChangesAsync();
                }
                //add subscribper
             
                return Ok();
            }
            await smsApi.Sms(fromNumber,
                    "Sorry, we only support Start and stopm if you have any questions please contact TVQMA")
                .WithCli("+18442872483").Send();
            return Ok();
        }
    }
}