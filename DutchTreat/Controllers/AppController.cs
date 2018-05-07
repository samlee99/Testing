using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DutchTreat.Data;
using DutchTreat.Services;
using DutchTreat.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DutchTreat.Controllers
{
    public class AppController : Controller
    {
        private IMailService _mailService;
        private readonly DutchContext context;

        public AppController(IMailService mailService, DutchContext context)
        {
            _mailService = mailService;
            this.context = context;
        }

        public IActionResult Index()
        {
            //throw new InvalidOperationException("Bad things");

            return View();
        }

        [HttpGet("contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost("contact")]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // send the email
                _mailService.SendMessage("sam@sam.com", model.Subject, $"From: {model.Name} - {model.Email}, Message: {model.Message}.");
                ViewBag.UserMessage = "Mail sent";
                ModelState.Clear();
            }
            return View();
        }

        public IActionResult About()
        {
            ViewBag.Title = "About Us";

            return View();
        }

        public IActionResult Shop()
        {
            var results = context.Products
                .OrderBy(p => p.Category)
                .ToList();
            return View(results);
        }
    }
}