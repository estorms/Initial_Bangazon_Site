using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bangazon.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BangazonWeb.Controllers
{
    public class CustomerController : Controller
    {
        private BangazonContext context;

        public CustomerController(BangazonContext ctx)
        {
            context = ctx;
        }

   
    //     public Create()
    //     {

    //     }

    //     public async Task<IActionResult> Detail([FromRoute]int? id)
    //     {
    //         // If no id was in the route, return 404
    //         if (id == null)
    //         {
    //             return NotFound();
    //         }

    //         var product = await context.Product
    //                 .Include(s => s.Customer)
    //                 .SingleOrDefaultAsync(m => m.ProductId == id);

    //         // If product not found, return 404
    //         if (product == null)
    //         {
    //             return NotFound();
    //         }

    //         return View(product);
    //     }

    //     public IActionResult Type([FromRoute]int id)
    //     {
    //         ViewData["Message"] = "Your contact page.";

    //         return View();
    //     }

    //     public IActionResult Error()
    //     {
    //         return View();
    //     }
    }
}