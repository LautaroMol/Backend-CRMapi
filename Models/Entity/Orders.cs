﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace CRMapi.Models.Entity
{
    [Index(nameof(OrderNumber), Name = "OrderNumber_UQ", IsUnique = true)]
    public class Orders : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderNumber { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public string CustomerDni { get; set; }
        public Clients Client { get; set; }
        public List <OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();

    }
}
