using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsController(ShopContext context)
        {
            _context = context;
            //Make sure were seeding db
            _context.Database.EnsureCreated();
        }
        [HttpGet]
        public async Task<ActionResult> GetAllProducts()
        {
            return Ok(await _context.Products.ToListAsync());
        }
        // alt. Route("/products/{id}") route url
        [HttpGet("{id}")] //route template
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
             _context.Products.Add(product); //Passing in the product, via model binding.
                                             //Dont need to create a new product and pass the properties of the arg to tne new prop
            await _context.SaveChangesAsync();
            //We want to return the newly created product id for the user. We call CreateAtAction helper method to pull the id.
            // since the id is generated in the db, we need to do a query.
            //CreatedAtAction is status code 201, which is the correct one for a post
            return CreatedAtAction(
                "GetProduct",
                new { id = product.Id },
                product);
        }
        [HttpPut("{id}")] //route template
        public async Task<ActionResult<Product>> PutProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();
            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) //Catches if product was updated async at same time by another method
            {
                // two scenarios, the product is deleted, or product was update
                if (!_context.Products.Any(p => p.Id == id))
                    return NotFound();
                else
                    throw; // general error.
            }
            return NoContent(); //204 - idempotent. Do not return nothing.
        }
    }
}
