using MTKPM_FE.Helpers;
using MTKPM_FE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace MTKPM_FE.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItemViewModel>>(CartController.CARTKEY) ?? new List<CartItemViewModel>();

            var numberOfItems = cart.Sum(p => p.Quantity);

            return View(numberOfItems);
        }
    }

    public class CartSummaryViewModel
    {
        public int NumberOfItems { get; set; }
    }
}