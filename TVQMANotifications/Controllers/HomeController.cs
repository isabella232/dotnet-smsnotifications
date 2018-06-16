using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVQMANotifications.Data;
using TVQMANotifications.Models;

namespace TVQMANotifications.Controllers {
    public class HomeController : Controller{
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        [Authorize]
        public IActionResult Index(){
            if (!_dbContext.Users.Any()){
                //First use, send to register
                RedirectToAction("TTRegister", "Account");
            }
            if (!_dbContext.SinchConfig.Any()){
                //set up sinch keys
                return RedirectToAction("Setup");
            }
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard", "Messages");
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Setup(){
            SinchConfig model = new SinchConfig();
             if (_dbContext.SinchConfig.Any()){
                model = _dbContext.SinchConfig.FirstOrDefault();
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Setup(SinchConfig model) {
            if (!_dbContext.SinchConfig.Any()){
                _dbContext.SinchConfig.Add(model);
            }
            else{
                _dbContext.SinchConfig.Update(model);
            }
            _dbContext.SaveChanges();
            return View(model);
        }

        public IActionResult Error(){
            return View(new ErrorViewModel{RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}