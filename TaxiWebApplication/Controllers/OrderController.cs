using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using TaxiWebApplication.Models;
using TaxiWebApplication.Services;
using TaxiWebApplication.ViewModels;

namespace TaxiWebApplication.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationContext _adb;
        private IUserManagerService _userService;
        private readonly KnnService _knn;
        private readonly GraphRoadService _grs;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;
        //  private readonly UserManagerService _userService;

        public OrderController(ApplicationContext adb, IUserManagerService userService, UserManager<User> userManager,
            KnnService knn, GraphRoadService grs, IMemoryCache cache)
        {
            _adb = adb;
            _userManager = userManager;
            _userService = userService;
            _knn = knn;
            _grs = grs;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new OrderViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(OrderViewModel orderViewModel)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            int t = _knn.Parse(orderViewModel.Address);
            string[] mas = orderViewModel.Address.Split(' ');
            
            double[] unknown = new double[2];
            for (int i = 0; i < mas.Length; i++)
            {
                unknown[i] = double.Parse(mas[i]);
            }

            ViewData["Distance"] = unknown[0];
            ViewData["Price"] = unknown[1];

            return RedirectToAction("DistanceAndPrice", "Order", new {distance = unknown[0], price = unknown[1]});


        }

        public IActionResult DistanceAndPrice(decimal distance, decimal price)
        {
            ViewData["Distance"] = distance;
            ViewData["Price"] = price;
            return View();
        }

        [HttpPost]
       
        public async Task<JsonResult> VVDistance([FromBody]TwoMarkersViewModel viewModel)
        {
            TwoMarkersViewModel coords = new TwoMarkersViewModel
            {
                FirstLat = viewModel.FirstLat,
                FirstLong = viewModel.FirstLong,
                SecondLat = viewModel.SecondLat,
                SecondLong = viewModel.SecondLong
            };

            double dist = await _grs.GraphDistance(coords.FirstLat, coords.FirstLong, coords.SecondLat, coords.SecondLong);
            DistPriceViewModel mod = new DistPriceViewModel { Distance = dist, Price = dist * 6 };
            

            return new JsonResult(mod);
        }

        [Authorize(Roles = "user,admin")]
        [HttpPost]
        public IActionResult FindDriver([FromForm] string model)
        {
            var userCoords = model.Split(" ");
            
            ViewData["Lattitude"] = Double.Parse(userCoords[0]);
            ViewData["Longitude"] = Double.Parse(userCoords[1]);

            return View(ViewData);
        }

        [Authorize(Roles = "driver")]
        [HttpGet]

        public IActionResult GettingOnTheLine()
        {
           // _cache.Set(User.Identity.Name, "hi");

            return View();
        }


        [Authorize(Roles = "driver")]
        [HttpPost]

        public IActionResult GettingOnTheLine([FromBody] LatAndLogViewModelWithDriverStatus model)
        {
            LatAndLogViewModelWithDriverStatus helperModel;
            if ((_cache.TryGetValue(User.Identity.Name, out helperModel)))
            {
                if ((model.Lattitude == helperModel.Lattitude) && (model.Longitude == helperModel.Longitude) && (model.Status == "Free"))
                {
                    _cache.Remove(User.Identity.Name);
                    //RedirectToAction("DriverIndex", "Account");

                    return new JsonResult("afk");
                } else
                {
                    _cache.Remove(User.Identity.Name);
                    _cache.Set(User.Identity.Name, model);
                }
               
            }
            else
            {
                _cache.Set(User.Identity.Name, model);
            }

            return new JsonResult(model);
        }

        [Authorize(Roles = "user, admin")]
        [HttpPost]

        public IActionResult OrderWindowPage([FromBody] LatAndLogViewModel userCoords)
        {
            //ViewData["Lattitude"] = userCoords.Lattitude;
            //ViewData["Longitude"] = userCoords.Longitude;

            double lattitude = userCoords.Lattitude;
            double longitude = userCoords.Longitude;
            return RedirectToAction("OrderWindowPageOpen", "Order", new { 
                HttpContext.User.Identity.Name, lattitude, longitude
            });
        }

        public IActionResult OrderWindowPageOpen(string name, double lattitude, double longitude)
        {
            ViewData["Lattitude"] = lattitude;
            ViewData["Longitude"] = longitude;
            ViewData["Name"] = name;

            return View();
        }
    }
}
