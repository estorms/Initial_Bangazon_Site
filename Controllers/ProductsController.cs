using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bangazon.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bangazon.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bangazon.ViewModels;

//Authors: Jammy Laird, Liz Sanger, Elliott Williams, David Yunker, Fletcher Watson

namespace Bangazon.Controllers
{
    //Creates ProductsController
    public class ProductsController : Controller
    {
        //Sets private property of BangazonContext;
        private BangazonContext context;

        //Method: creates custom constructor method with argument of context, therefore rendering context public 
        public ProductsController(BangazonContext ctx)
        {
            context = ctx;
        }

        //Method: creates async method for two purposes: extract the Customer table from current context for extraction into the dropdown menu and return the Index view of complete product list
        public async Task<IActionResult> Index()
        {
            // Create new instance of the view model
            ProductListViewModel model = new ProductListViewModel(context);

            // Set the properties of the view model
            model.Products = await context.Product.OrderBy(s => s.Title.ToUpper()).ToListAsync();

            return View(model);
        }

        //Method: purpose is to return the AllProductsView only show products in the selected filtered by subcategory. Accepts an argument of the selected subcategory's id
        public async Task<IActionResult> ProductsInSubCategory([FromRoute] int id)
        {
            ProductListViewModel model = new ProductListViewModel(context);
            
            model.Products = await context.Product.Where(p => p.ProductTypeSubCategoryId == id).OrderBy(s => s.Title.ToUpper()).ToListAsync();
            // codebase.Methods.Where(x => (x.Body.Scopes.Count > 5) && (x.Foo == "test"));
            return View("Index", model);

        }
        //Method: purpose is to create Products/Create view that delivers the form to create a new product, including the product type dropdown (will need adjustment when creating subcategories) and customer dropdown on navbar

        [HttpGet]
        public IActionResult Create()
        {
            CreateProductViewModel model = new CreateProductViewModel(context);
            return View(model);
        }

        //Method: Purpose is to send the customer's product to the database and then redirects the user to the homepage (AllProductsView)
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Product product)
        {
            //This creates a new variable to hold our current instance of the ActiveCustomer class and then sets the active customer's id to the CustomerId property on the product being created so that a valid model is sent to the database
            var customer = ActiveCustomer.instance.Customer;
            product.CustomerId = customer.CustomerId;

            //This creates a new instance of the CreateProductViewModel so that we can return the same view (i.e., the existing product info user has entered into the form) if the model state is invalid when user tries to create product
            CreateProductViewModel model = new CreateProductViewModel(context);

            if (ModelState.IsValid)
            {
                context.Add(product);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult GetSubCategories([FromRoute]int id)
        {
            //get sub categories with that product type on them
            var subTypes = context.ProductTypeSubCategory.OrderBy(s => s.Name.ToUpper()).Where(p => p.ProductTypeId == id).ToList();
            return Json(subTypes);
        }

        //Method: Purpose is to route user to the detail view on a selected product. Accepts an argument (passed in through the route) of the product's primary key (id)

        public async Task<IActionResult> Detail([FromRoute]int? id)
        {
            //throw a 404(NotFound) error if method is called w/o id in route
            if (id == null)
            {
                return NotFound();
            }

            // Create a new instance of the ProductDetailViewModel and pass it the existing BangazonContext (current db session) as an argument in order to extract the product whose id matches the argument passed in¸
            ProductDetailViewModel model = new ProductDetailViewModel(context);

            // Set the `Product` property of the view model and include the product's seller (i.e., its .Customer property, accessed via Include, which traverses Product table and selects the Customer FK)
            model.Product = await context.Product
                    .Include(prod => prod.Customer)
                    .SingleOrDefaultAsync(prod => prod.ProductId == id);

            // If no matching product found, return 404 error
            if (model.Product == null)
            {
                return NotFound();
            }

            //Otherwise, return the ProductDetailViewModel view with the ProductDetailViewModel passed in as argument for rendering that specific product on the page

            return View(model);
        }

        //Method: Purpose is to return a view that displays all the products of one category. Accepts one argument, passed in through route, of ProductTypeId.
        public async Task<IActionResult> Type([FromRoute]int id)
        {
            ProductListViewModel model = new ProductListViewModel(context);
            model.Products = await context.Product.OrderBy(s => s.Title.ToUpper()).Where(p => p.ProductTypeId == id).ToListAsync();
            return View(model);
        }

        //Method: Purpose is to render the ProductTypes view, which displays all product categories
        public async Task<IActionResult> Types()
        {
            //This creates a new instance of the ProductTypesViewModel and passes in the current session with the database (context) as an argument

            ProductTypesViewModel model = new ProductTypesViewModel(context);
            model.ProductTypes = await context.ProductType.OrderBy(s => s.Label).ToListAsync();
            model.ProductTypeSubCategories = await context.ProductTypeSubCategory.OrderBy(s => s.Name).ToListAsync();
            //list of subcategories
            var subCats = context.ProductTypeSubCategory.ToList();
            //cycle through each subcategory and define its Quantity as 
            subCats.ForEach(sc => sc.Quantity = context.Product.Count(p => p.ProductTypeSubCategoryId == sc.ProductTypeSubCategoryId));
            // model.ProductTypes = productTypes;
            return View(model);
        }

        //Method: Purpose is to return the Error view
        public IActionResult Error()
        {
            return View();
        }

        //Method: Purpose is to create a new line item in the database when a customer clicks the "Add to Cart" button on a product. Accepts an argument of the productId, which is passed in through the post request attached to event listener on "Add to Cart" button
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromRoute] int id)
        {
            //Define "customer" as the current active customer so that this customer's id can be used throughout the method
            Customer customer = ActiveCustomer.instance.Customer;
            //Define "openOrder" as an order in the database associated with the customer's id but not yet completed
            Order openOrder = await context.Order.Where(o => o.DateCompleted == null && o.CustomerId == customer.CustomerId).SingleOrDefaultAsync();
            //If there is no open order, create one and add the customer's id to the order 
            if (openOrder == null)
            {
                Order newOrder = new Order();
                newOrder.CustomerId = Convert.ToInt32(customer.CustomerId);
                //Add the new order to the database
                context.Add(newOrder);
                await context.SaveChangesAsync();
                // Then, create a new line item and add the open order's id and the product id to that line item

                LineItem lineItem = new LineItem();
                lineItem.OrderId = newOrder.OrderId;
                lineItem.ProductId = Convert.ToInt32(id);
                //Add the line item to the database
                context.Add(lineItem);
                await context.SaveChangesAsync();
                // return RedirectToAction("Index", "Products");
            }

            //If there is an open order, create a new line item and add it to that order
            else
            {
                LineItem lineItem = new LineItem();
                lineItem.OrderId = openOrder.OrderId;
                lineItem.ProductId = Convert.ToInt32(id);
                //Add the line item to the database
                context.Add(lineItem);
                await context.SaveChangesAsync();
                // return RedirectToAction("Index", "Products");

            }
            return RedirectToAction("Index", "Products");
        }
    }
}