using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bangazon.Data;
using Bangazon.Models;
using Bangazon.ViewModels;
using Microsoft.AspNetCore.Routing;


namespace Bangazon.Controllers
{
    
    
    // Author: Jammy Laird 
 
    

// Defines the PaymentController controller class, which inherits from base class Controller
    public class PaymentController : Controller     
    {
        private BangazonContext  context;
        //Set a private property on OrderController that stores the current session with db
        public PaymentController(BangazonContext cxt)
        {
            context = cxt;
        }
         //Method: Purpose is to return a view that allows the customer to create a payment method
        public IActionResult Create()
        {
            PaymentTypeViewModel model = new PaymentTypeViewModel(context);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (PaymentType paymentType)
        {
            // The new payment type is linked to the active customer if the entered text satifies the model
        
            var customer = ActiveCustomer.instance.Customer;
            paymentType.CustomerId = customer.CustomerId;
            if (ModelState.IsValid)
            {
                 //Add the line item to the database
                context.Add(paymentType);
                await context.SaveChangesAsync();
                return RedirectToAction ("Cart", "Order");
            }
            // if the model is not satisfied return an empty view
            PaymentTypeViewModel model = new PaymentTypeViewModel(context);
            // model.NewPaymentType = paymentType.NewPaymentType;

            return View(model);
        }

    }
    
}