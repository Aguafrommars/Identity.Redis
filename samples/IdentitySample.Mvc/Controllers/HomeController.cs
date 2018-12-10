// Project: aguacongas/Identity.Redis
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}