using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bangazon.Data;
using Bangazon.Models;
using Bangazon.ViewModels;
using Microsoft.AspNetCore.Routing;


namespace Bangazon.Controllers
{
    
    
    // Authors: Jammy Laird & Liz Sanger
 
    

// Class: PaymentController controller, which inherits from base class Controller
    public class PaymentController : Controller     
    {
        //Set a private property on OrderController that stores the current session with db
        private BangazonContext  context;

        //Method: a custom constructor for the payment controller that passes in BangazonContext as an argument so that the controller has access to the database 
        public PaymentController(BangazonContext cxt)
        {
            context = cxt;
        }
         //Method: Purpose is to inject the PaymentTypeViewModel into the Payment/Create view so that the customer can create a payment method
        public IActionResult Create()
        {
            PaymentTypeViewModel model = new PaymentTypeViewModel(context);
            return View(model);
        }

        //Method: Purpose is to post a new payment type to the database and associate it with the active customer's id, accepts an argument of type Payment Type, which is composed of data injected in the payment/create form 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (PaymentType paymentType)
        {
            // Link the active customer's id to the newly created payment type
            var customer = ActiveCustomer.instance.Customer;
            paymentType.CustomerId = customer.CustomerId;
            
            //if the payment type received from the form is valid, post it to the database and redirect the customer to the Order/Cart view
            if (ModelState.IsValid)
            {
                 //Add the payment type to the database
                context.Add(paymentType);
                await context.SaveChangesAsync();
                return RedirectToAction ("Cart", "Order");
            }
            //otherwise, recreate the payment/create view, which will display an error message based on the invalidity in the model
            PaymentTypeViewModel model = new PaymentTypeViewModel(context);
            return View(model);
        }

    }
    
}