using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;

        public BidsController(RealStateContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Place bid
        [HttpPost]
        [Authorize(Roles = "Buyer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int propertyId, decimal amount)
        {
            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
            {
                return NotFound();
            }

            // Check if bidding is open
            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is not open for this property.";
                return RedirectToAction("Details", "Properties", new { id = propertyId });
            }

            // Check if buyer already has an active bid
            var existingBid = await _context.Bids
                .FirstOrDefaultAsync(b => b.PropertyId == propertyId && b.BidderId == user.Id && b.IsActive);

            if (existingBid != null)
            {
                TempData["Error"] = "You already have an active bid for this property. Update your bid instead.";
                return RedirectToAction("Details", "Properties", new { id = propertyId });
            }

            // Place a new bid
            var bid = new Bid
            {
                PropertyId = propertyId,
                BidderId = user.Id,
                BidderName = user.UserName,
                Amount = amount,
                TimePlaced = DateTime.Now,
                IsActive = true
            };

            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Properties", new { id = propertyId });
        }

        // Update bid
        [HttpPost]
        [Authorize(Roles = "Buyer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBid(int bidId, decimal newAmount)
        {
            var user = await _userManager.GetUserAsync(User);
            var bid = await _context.Bids
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == bidId && b.BidderId == user.Id && b.IsActive);

            if (bid == null)
            {
                TempData["Error"] = "You can only update your active bids.";
                return RedirectToAction("Details", "Properties", new { id = bid?.PropertyId });
            }

            var property = bid.Property;

            // Check if bidding is still open
            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is closed for this property.";
                return RedirectToAction("Details", "Properties", new { id = property.Id });
            }

            // Update bid details
            bid.Amount = newAmount;
            bid.TimePlaced = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Properties", new { id = property.Id });
        }

        // Cancel bid
        [HttpPost]
        [Authorize(Roles = "Buyer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBid(int bidId)
        {
            var user = await _userManager.GetUserAsync(User);
            var bid = await _context.Bids
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == bidId && b.BidderId == user.Id && b.IsActive);

            if (bid == null)
            {
                TempData["Error"] = "You can only cancel your active bids.";
                return RedirectToAction("Details", "Properties", new { id = bid?.PropertyId });
            }

            var property = bid.Property;

            // Check if bidding is still open
            if (property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is closed for this property.";
                return RedirectToAction("Details", "Properties", new { id = property.Id });
            }

            // Cancel the bid
            bid.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Properties", new { id = property.Id });
        }

        // Determine the highest bid (Winner)
        public async Task DetermineWinningBid(int propertyId)
        {
            var property = await _context.Properties.Include(p => p.Bids).FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null || property.BiddingEndTime > DateTime.Now)
            {
                return; // Either the property doesn't exist or the bidding period isn't over
            }

            var highestBid = property.Bids
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefault();

            if (highestBid != null)
            {
                highestBid.IsWinningBid = true;
            }

            await _context.SaveChangesAsync();
        }
        [HttpGet]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> ViewBids(int propertyId)
        {
            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties
                .Include(p => p.Bids)
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.CreatedBy == user.Id);

            if (property == null)
            {
                TempData["Error"] = "You can only view bids for your own properties.";
                return RedirectToAction("Index", "Properties");
            }

            return View(property); // Pass the property along with its bids to the view
        }

    }
}
