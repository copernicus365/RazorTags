using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorTags.WebDemo.Models;

namespace RazorTags.WebDemo.Controllers
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

		public IActionResult Playground(int page = 0)
		{
			ViewData["Message"] = "Howdy.";

			var vm = new PlaygroundVM() {
				Page = page,
				Name = "John",
				Age = 30,
				Email = "john@test.com",
				Phone = "",
				PublishedTime = DateTime.Now,
				Title = "Lorem ipsum κάλος שָׁלוֹם",
				Color = ColorType.Black
			};

			return View(vm);
		}

		[HttpPost]
		public IActionResult Playground(PlaygroundVM vm)
		{
			if(!ModelState.IsValid || vm == null)
				return View(vm);

			ViewData["Message"] = $"IsCool!: {vm.IsCool}, IsSanctus: {vm.IsSanctified}, Run: {vm.RunToStore}, Color: {(vm.Color?.ToString()?.ToUpper() ?? "--")}";
			return View(vm);
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
