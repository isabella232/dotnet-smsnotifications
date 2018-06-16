using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Sinch.ServerSdk;
using Sinch.ServerSdk.Messaging;
using TVQMANotifications.Data;
using TVQMANotifications.Models;

namespace TVQMANotifications.Services {
    public class SMSSender {
        private ApplicationDbContext _context;
        
        private readonly IConfiguration _configuration;
        private SinchConfig _sinchConfig;

        public SMSSender(ApplicationDbContext context, IConfiguration configuration) {
            _context = context;
            
            _configuration = configuration;
            _sinchConfig = context.SinchConfig.FirstOrDefault();
        }
        public async Task SendSMS(Message message, Subscriber to) {
            var smsApi = SinchFactory.CreateApiFactory(_sinchConfig.Key, _sinchConfig.Secret).CreateSmsApi();
            var messageid = await smsApi.Sms(to.Number,
                    message.MessageContent + "\n\n" +
                    _sinchConfig.MarketingFooter)
                .WithCli(_sinchConfig.SinchNumber).Send();
            _context.SendLogs.Add(new SendLog() {
                DateSent = DateTime.UtcNow,
                MessageId = message.MessageId,
                SubscriberId = to.SubscriberId,
                SinchMessageId = messageid.MessageId.ToString()
            });
            await _context.SaveChangesAsync();
            
        }
    }
}
