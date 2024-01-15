﻿using Microsoft.AspNetCore.Mvc;
using RemoteLight.Models;
using System.Diagnostics;

// TODO: Autogenerated: We need to change it for our purposes
// TODO: Autogenerated: We need to change Views/Home/Index.cshtml for our purposes 
// TODO: Autogenerated: We need to change Views/Home/Privacy.cshtml for our purposes 
// TODO: Autogenerated: We need to change Views/Shared/_Layout.cshtml for our purposes
// TODO: Add Identity framework
// TODO: Add MQTT implementation

namespace RemoteLight.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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