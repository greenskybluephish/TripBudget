﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BackpackingBudget.Data;
using BackpackingBudget.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BackpackingBudget.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;
        public BudgetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: Budgets
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var currentUser = await GetCurrentUserAsync();
            var budgets = await _context.Budget.Include(b => b.User).Where(b => b.User == currentUser).ToListAsync();
            return View(budgets);
        }

        // GET: Budgets/Details/5
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await GetCurrentUserAsync();
            var budget = await _context.Budget.Include(b => b.User).Where(b => b.User == currentUser && b.IsActive).FirstOrDefaultAsync();

            if (budget == null)
            {
                return RedirectToAction("Index");
            }

            return View(budget);
        }

        // GET: Budgets/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Budgets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Name,UserId,StartDate,EndDate,BudgetAmount,IsActive")] Budget budget)
        {
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUserAsync();
                budget.UserId = currentUser.Id;
                _context.Add(budget);
                await _context.SaveChangesAsync();

                if (budget.IsActive)
                {
                    var makeInactive = await _context.Budget.Where(b => b.UserId == budget.UserId && budget.IsActive).FirstOrDefaultAsync();
                    if (makeInactive != null)
                    {
                        makeInactive.IsActive = false;
                        _context.Update(makeInactive);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Dashboard));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
                
            }

            return View(budget);
        }

        // GET: Budgets/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budget.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }
            return View(budget);
        }

        // POST: Budgets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BudgetId,Name,UserId,StartDate,EndDate,BudgetAmount,IsActive")] Budget budget)
        {
            if (id != budget.BudgetId)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await GetCurrentUserAsync();
                    budget.UserId = currentUser.Id;
                    if (budget.IsActive)
                    {
                        var makeInactive = await _context.Budget.Where(b => b.UserId == budget.UserId && b.IsActive).FirstOrDefaultAsync();
                        if (makeInactive != null)
                        {
                            makeInactive.IsActive = false;
                            _context.Update(makeInactive);   
                        }
                        _context.Update(budget);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Dashboard));
                    }

                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BudgetExists(budget.BudgetId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(budget);
        }

        // GET: Budgets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var budget = await _context.Budget
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BudgetId == id);
            if (budget == null)
            {
                return NotFound();
            }

            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budget.FindAsync(id);
            _context.Budget.Remove(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BudgetExists(int id)
        {
            return _context.Budget.Any(e => e.BudgetId == id);
        }
    }
}
