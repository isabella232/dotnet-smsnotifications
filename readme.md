## Building a simple SMS notification system with Sinch

A few months back my son and I started to race [quarter midgets](https://en.wikipedia.org/wiki/Quarter_Midget_racing). For myself, it has been loads of fun and a steep learning curve. Believe me, getting perfectly aligned cells without using tables in HTML is pretty easy compared to tuning a racing chassis! It's also amazing to meet the people that put in the hard work, and their heart to make racing (somewhat) affordable for kids. Anyway, everyone is encouraged to contribute to the club with their skills - in my case, coding is a skill I am at least somewhat competent in. In this article I’ll show you a simple notification system I built yesterday for the Western Grands. Every year there are three main events in Quarter Midget racing, one for each region and one dirt event. This year our club [Tri Valley Quarter Midget Association](https://www.tvqma.org/) has the honor of hosting the events.

![](images/qmcar.jpg)

This year, 250 cars and about 125 drivers will come from all over the Western States to race at our track in 17 different classes, for three intensive days. To manage all the races and drivers and make sure everyone is in the right place at the right time, there are a lot of logistics to take care of. To give you an idea of the schedule, 2 days alone are taken up with parking trailers, then every car needs to be checked that they have the correct fuel, inspected for safety, be weighed, have lap times measured and last but not least, get the cars and kids out on the track. Each kid has practice laps, qualifying laps, heats, Lower Mains and the A mains. In a club race; this is usually done by a single announcer who will announce who needs to be where. This year the event is just going to be too big. If you're in your trailer there's no way to hear the announcer during the Grands. To solve this problem, a simple SMS system for the Tower and Pit stewards to send SMS to everyone to communicate announcements seemed like a good idea. We believe this system has the potential to help people be on time and also give them the confidence to relax and have fun! After all that’s what it's all about!

## Prerequistes
This article assumes that you are familiar with .net core and ASP.net MVC patterns, you will also need an account and a phone number with Sinch. 

The repo is located at github [here](http://github.com/sinch/dotnet-smsnotificaions), the solution is kind of ready to run, you just need to add a valid connection string. There is more code in the repo in this article, but I will talk about the Sinch specific parts


## Time to build 
[screen shot]
So the basic idea is that the track official who would usually announce something over a PA system, also has access to a tool to send a quick SMS out to everyone containing the same message. Due to regulations in the US, we are going to use Toll-free numbers to send SMS to ensure high throughput and no spam filter. If you live in Europe, you can pick any number you like in most countries. 

First we needed to collect phone numbers from racers, we managed this by advertising on social media and having signs around the track asking people to send an SMS containing 'START' to +1 888-851-0949. Once a text was sent, the driver / racer was added to the database. 

The next thing we needed was a way for track officials to send out messages. In this case, we had two possibilities: 
1. Via a website 
2. Via a whitelist of numbers with the ability to send SMS to the number above number, with anything they send also being sent to everyone in the list. 


![](images/flowchart.png)

### Managing signups via SMS
I bought a number in the [portal](https://portal.sinch.com/#/numbers) (Yeah, I know we should have way more countries in stock, its coming. For now mail me if you need a particular country.) Created an app and assigned the number a webhook url to receive SMS. 

![](images/dashboardcallback.png)

I used the awesome tool [ngrok](https://www.sinch.com/tutorials/getting-second-number-testing-sinch-callbackswebhooks-ngrok/) during development. 

Next I needed to add a WebApi controller to handle all incoming SMS. 

*SMSController.cs*
```csharp
[Produces("application/json")]
public class SMSController : Controller {
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
```

There are a few things to mention here. Firstly I wanted to reach the "Start" and "Stop" keyword, the unstop command kicks in if you start, then send a stop message. If this happens you'll need to send an unstop message to re-enble sms from that number. 
In the start command I checked to make sure that the subscriber didn't already exist, if they are not already in the database I added them and then finally sent out the welcome message. I opted for sending the message even if the subscriber exists, since its a command the program still understands. **One Gotcha here, we send you the number with out + in e 164 format, but we require you to send it with a + to make sure its a country code hence the var fromNumber = "+" + model.From.Endpoint;**

I also wanted to support stop so that people could remove themselves and also provide somewhat meaningful feedback if something was sent in that we didn't understand.



### Subscriber data class 
The subscriber data class keeps track of the people that send in a start SMS, it's using Entity framework. In the github repo you will also see that I scafolded the whole class to provide the club with a crude admin of subscribers. 

*Susbscriber.cs*
```csharp
public class Subscriber {
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int SubscriberId { get; set; }
	public string Number { get; set; }
}
```

And then I added it as a db set to the ApplicationDbContext as a DBset<Subscriber>  

*ApplicationDbContext.cs*
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
    public DbSet<Message> Messages { get; set; }
    public DbSet<Subscriber> Subscribers{ get; set; }
    public DbSet<SendLog> SendLogs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
    }
```

Then the SMS is sent through an SMSsender service that is added in Startup.cs - I will cover that a bit later. 
As you can see,  I use the Sinch Nuget package to help me out https://www.nuget.org/packages/Sinch.ServerSdk/ while not necessary, it sure makes it easier when it comes to signing requests. 

*SMSSender.cs* 
```
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
```

This service creates a SMS API, adds the footer marketing text and logs the SMS message to a send log so if we needed to implement status checks at a late stage we would be able to. 

In startup.cs in the ConfigureServices, I added a line so dependency injection could take of this whenever I needed to send an SMS message. 

```
services.AddTransient<SMSSender>();
```


## Sending SMS notifications. 
I wanted to store each message sent, and also keep a log of of who messages were snet to, so I added two more classes to support this. 

*Message.cs*
```csharp
public class Message {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MessageId { get; set; }
    public String MessageContent { get; set; }
    public DateTime DateSent { get; set; }
    public virtual IEnumerable<SendLog> Logs { get; set; }
}
```

and *SendLog.cs*
```csharp
public class SendLog {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SendLogId { get; set; }
    public int SubscriberId { get; set; }
    public int MessageId { get; set; }
    public string SinchMessageId { get; set; }
    public DateTime DateSent { get; set; }
    public DateTime DateDelivered { get; set; }
    public string Status { get; set; }
    public virtual Subscriber Subscriber { get; set; }
    }
```

I wanted to show this in a dashboard like page so a ViewModel is also needed

*DashboardModel.cs*
```
public class DashboardModel {
    public int SubscriberCount { get; set; }
    public string Message { get; set; }
    public List<Message> LastMessages { get; set; }
}
```

Finally a a Controller and a View 

*MessageContoller.cs*
```
public class MessagesController : Controller {
    private readonly ApplicationDbContext _context;
    public MessagesController(ApplicationDbContext context) {
        _context = context;
    }

    [Route("/dashboard")]
    [HttpGet]
    public IActionResult Dashboard()  {
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
        private async Task CreateAndSendMessage(Message message) {
            message.DateSent = DateTime.UtcNow;
            _context.Add(message);
            await _context.SaveChangesAsync();
            var subscribers = await _context.Subscribers.ToListAsync();
            foreach (var s in subscribers) {
                await _smsSender.SendSMS(message, s);
            }
        }
}

```

I wanted to make it super easy for officials to send a quick message. Getting drivers to staging is a critical step of the race, if you dont show up on time for your race you will miss it. 

*Dashboard.cshml*
```
@model TVQMANotifications.Models.DashboardModel
@{
    ViewBag.Title = "TVQMA Dashboard";
}

<H1>Send an SMS to 844-287-2483 with the the word START in it.</H1>
<div class="row">
    <div class="col-md-6">
        <h2>Send a new notification</h2>
        <form asp-action="Dashboard">
            @*<input type="hidden"  value="Jr Novice to Staging"/>*@
            <input type="submit" asp-for="Message" value="Jr Novice to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Sr Novice to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Jr Animal to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Sr Animal to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Hvy Animal to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Jr Honda to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Sr Honda to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Hvy Honda to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Jr Stock to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Mod  to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Lt 160 to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Hvy 160 to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="B to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="AA/Modified World to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Jr Half to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Lt World Formula to staging" class="btn btn-default" />
            <input type="submit" asp-for="Message" value="Hvy World Formula to staging" class="btn btn-default" />
        </form>        
        <form asp-action="Dashboard">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="Message" class="control-label"></label>
                <textarea asp-for="Message" class="form-control" ></textarea>
                <span asp-validation-for="Message" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Send" class="btn btn-default" />
            </div>
        </form>        
    </div>
    <div class="col-md-6">
        <h2>Number of Subscribers @Model.SubscriberCount</h2>
        <table class="table">
            <thead>
            <tr>
                <th colspan="2">
                    Latest messages
                </th>
                <th>
                </th>
                <th></th>
            </tr>
            </thead>
            <tbody>
            @{
                var i = 0;
            }
            @foreach (var item in Model.LastMessages)
            {
                i =++ i;
                <tr  class="@(i == 1 ? "sucess" : "")" style="background: @(i == 1 ? "#dff0d8" : "")">
                    <td >
                        @item.DateSent.AddHours(-7)
                    </td>
                    <td>
                        @Html.DisplayFor(modelItem => item.MessageContent)
                    </td>

                    <td>
                        <a asp-action="Details" asp-route-id="@item.MessageId">Details</a>
                    </td>
                </tr>
            }
            </tbody>
        </table>

    </div>
    <div class="row">
        <div class="col-md-6">
            To unsubscibe sent stop to same number
        </div>
    </div>
</div>
```

That's pretty much it when it comes to the SMS and Sinch functionality, in the solution you will see some user handling and a few pages to deal with messages that is standard aspnet stuff. Clone this and let me know if you have any questions. 




 
