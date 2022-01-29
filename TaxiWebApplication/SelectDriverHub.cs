
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

        public SelectDriverHub(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task SendToMap(string driverName)
        {
            LatAndLogViewModelWithDriverStatus driverInfo;
            _cache.TryGetValue(driverName, out driverInfo);
            var variableForDriverInfo = driverInfo;            
            await Clients.User(Context.UserIdentifier)
                    .SendAsync("Getting", $"{variableForDriverInfo.Lattitude} + ' ' + {variableForDriverInfo.Longitude}");
        }
    }
}
