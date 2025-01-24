using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealState.Data;
using RealState.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealState.Controllers
{
    [Authorize]
    public class BidsController : Controller
    {
        private readonly RealStateContext _context;

        public BidsController(RealStateContext context)
        {
            _context = context;
        }

        // Place bid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int propertyId, string bidderName, decimal amount)
        {
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
            if (property == null)
            {
                return NotFound();
            }

            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is not open for this property.";
                return RedirectToAction("Details", "Properties", new { id = propertyId });
            }

            var bid = new Bid
            {
                PropertyId = propertyId,
                BidderName = bidderName,
                Amount = amount,
                TimePlaced = DateTime.Now
            };

            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Properties", new { id = propertyId });
        }

        // Update bid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBid(int bidId, decimal newAmount)
        {
            var bid = await _context.Bids
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == bidId);

            if (bid == null || !bid.IsActive)
            {
                return NotFound();
            }

            var property = bid.Property;

            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is closed for this property.";
                return RedirectToAction("Details", "Properties", new { id = property.Id });
            }

            bid.Amount = newAmount;
            bid.TimePlaced = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Properties", new { id = property.Id });
        }

        // Cancel bid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBid(int bidId)
        {
            var bid = await _context.Bids
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == bidId);

            if (bid == null || !bid.IsActive)
            {
                return NotFound();
            }

            var property = bid.Property;

            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is closed for this property.";
                return RedirectToAction("Details", "Properties", new { id = property.Id });
            }

            bid.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Properties", new { id = property.Id });
        }
    }
}
