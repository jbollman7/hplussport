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
        [HttpGet("/seconds")] //hostname:port/seconds
        public int Get()
        {
            return DateTime.Now.Second;
        }
        
        [HttpGet]
        public async Task<ActionResult> GetAllProducts([FromQuery]ProductQueryParameters queryParameters)
        {
            // We want to return parts of the product
            IQueryable<Product> products = _context.Products;
            if (queryParameters.MinPrice is not null)
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice.Value);  
            }
            if (queryParameters.MaxPrice is not null)
            {
                products = products.Where(
                    p => p.Price <= queryParameters.MaxPrice.Value);
            }
            //search
            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }
            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }

            //pagination
            products = products.Skip(queryParameters.Size * (queryParameters.Page - 1))
                                .Take(queryParameters.Size);
            return Ok(await products.ToListAsync());
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
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return product; //return deleted product
        }
        [HttpPost]
        [Route("Delete")]
        public async Task<ActionResult> DeleteMultiple([FromQuery] int[] ids)
        {
            var products = new List<Product>();
            foreach (var id in ids)
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                products.Add(product);
            }

            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();

            return Ok(products);
        }
    }
}
