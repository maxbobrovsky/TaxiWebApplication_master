using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaxiWebApplication.Models;
using TaxiWebApplication.Services;
using TaxiWebApplication.ViewModels;

namespace TaxiWebApplication
{
    
    public class OrderHub : Hub
    {
        public OrderHub(ApplicationContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        private ApplicationContext _context;
        private IMemoryCache _cache;

        [Authorize]
        public async Task Send(string user_lat, string user_lon)
        {
            var user = Context.User;
            var userName = Context.User.Identity.Name;

            var Helper_drivers = _context.Users.Join(_context.UserRoles,
                                    u => u.Id,
                                    r => r.UserId,
                                    (u, r) => new
                                    {
                                        UserId = r.UserId,
                                        NickName = u.UserName,
                                        RoleId = r.RoleId
                                    });

            var drivers = Helper_drivers.Join(_context.Roles,
                                              h => h.RoleId,
                                              ro => ro.Id,
                                              (h, ro) => new
                                              {
                                                  UserName = h.NickName,
                                                  Role = ro.Name
                                              }).Where(x => x.Role == "driver");


            List<string> names = new List<string>();

            LatAndLogViewModelWithDriverStatus coord;

            foreach(var u in drivers)
            {
                var usr = u.UserName;
                if (_cache.TryGetValue(usr, out coord))
                {
                    names.Add(u.UserName + " " + KnnService.DistanceToDest(new double[] { double.Parse(user_lat), double.Parse(user_lon) }, new double[] { coord.Lattitude, coord.Longitude}) + " km-s to you");
                }
                continue;
            }

            await Clients.User(Context.UserIdentifier).SendAsync("Receive", string.Join(" ", names));
        }
    }
}