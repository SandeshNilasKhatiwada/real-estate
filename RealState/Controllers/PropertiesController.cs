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
    public class PropertiesController : Controller
    {
        private readonly RealStateContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PropertiesController(RealStateContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List properties based on roles
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Seller"))
            {
                // Show only properties created by the current seller
                var sellerProperties = await _context.Properties
                    .Where(p => p.CreatedBy == user.Id)
                    .ToListAsync();
                return View(sellerProperties);
            }
            else if (User.IsInRole("Buyer"))
            {
                // Show all properties for buyers
                var allProperties = await _context.Properties
                    .Include(p => p.Bids)
                    .ToListAsync();
                return View(allProperties);
            }

            TempData["Error"] = "You do not have the required role to access this page.";
            return RedirectToAction("AccessDenied", "Account");
        }

        // Show property details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Bids) // Include related bids
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // Create property (Only sellers)
        [Authorize(Roles = "Seller")]
        public IActionResult Create()
        {
            return View(new Property());
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            var user = await _userManager.GetUserAsync(User);
            property.CreatedBy = user.Id;

            ModelState.Remove(nameof(Property.CreatedBy));

            if (ModelState.IsValid)
            {
                _context.Properties.Add(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }

        // Edit property (Only sellers)
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

            if (property == null)
            {
                TempData["Error"] = "You can only edit properties you own.";
                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property)
        {
            var user = await _userManager.GetUserAsync(User);

            if (id != property.Id)
            {
                return NotFound();
            }

            var existingProperty = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

            if (existingProperty == null)
            {
                TempData["Error"] = "You can only edit properties you own.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove(nameof(Property.CreatedBy));

            if (ModelState.IsValid)
            {
                existingProperty.Name = property.Name;
                existingProperty.Address = property.Address;
                existingProperty.Price = property.Price;
                existingProperty.VideoUrl = property.VideoUrl;
                existingProperty.BiddingStartTime = property.BiddingStartTime;
                existingProperty.BiddingEndTime = property.BiddingEndTime;

                _context.Update(existingProperty);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }

        // Delete property (Only sellers)
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

            if (property == null)
            {
                TempData["Error"] = "You can only delete properties you own.";
                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

            if (property == null)
            {
                TempData["Error"] = "You can only delete properties you own.";
                return RedirectToAction(nameof(Index));
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // View bids for a specific property (Only sellers)
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> ViewBids(int? propertyId)
        {
            if (propertyId == null || propertyId <= 0)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            // Ensure the property belongs to the current seller
            var property = await _context.Properties
                .Include(p => p.Bids) // Include related bids
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.CreatedBy == user.Id);

            if (property == null)
            {
                TempData["Error"] = "You can only view bids for your own properties.";
                return RedirectToAction(nameof(Index));
            }

            return View(property); // Pass property and its bids to the view
        }
    }
}
