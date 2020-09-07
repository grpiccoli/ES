using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EpicSolutions.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Reflection;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;

namespace EpicSolutions.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailSender _emailSender;

        public HomeController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Partners()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Services()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Portafolio()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact([Bind("Name, Email, Subject, Message")] Contact contact)
        {
            if(contact != null)
            {
                if (ModelState.IsValid)
                {
                    await _emailSender.SendEmailAsync(contact.Email, contact.Subject, $"{contact.Name}^{contact.Message}")
                        .ConfigureAwait(false);
                }
                contact.IsValid = ModelState.IsValid;
            }
            return View(contact);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
