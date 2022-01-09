
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaxiWebApplication.Models;
using TaxiWebApplication.ViewModels;

namespace TaxiWebApplication
{
    public class SelectDriverHub : Hub
    {
        private readonly IMemoryCache _cache;
        private readonly UserManager<IdentityUser> _userManager;

        public SelectDriverHub(IMemoryCache cache, UserManager<IdentityUser> userManager)
        {
            _cache = cache;
            _userManager = userManager;
        }

        public async Task SendToMap(string userName, string driverName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var userId = await _userManager.GetUserIdAsync(user);
            LatAndLogViewModelWithDriverStatus driverInfo;

            _cache.TryGetValue(driverName, out driverInfo);

            var variableForDriverInfo = driverInfo;
            
            
            await Clients.Client(userId).SendAsync($"{variableForDriverInfo.Lattitude} + {variableForDriverInfo.Longitude}");
        }
    }
}
