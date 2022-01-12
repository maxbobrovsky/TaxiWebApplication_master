using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TaxiWebApplication.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public User Passenger { get; set; }

        public User Driver { get; set; }

        public DateTime OrderCreated { get; set; } = DateTime.Now;
    }


}
