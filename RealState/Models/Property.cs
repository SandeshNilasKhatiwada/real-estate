using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RealState.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Price { get; set; }
        public string VideoUrl { get; set; }
        public DateTime? BiddingStartTime { get; set; }
        public DateTime? BiddingEndTime { get; set; }
        public List<Bid> Bids { get; set; } = new List<Bid>();

        // Track ownership
        [BindNever] // Prevent validation for this field
        public string CreatedBy { get; set; }
    }
}
