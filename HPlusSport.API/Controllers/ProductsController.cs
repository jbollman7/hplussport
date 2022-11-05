﻿using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products.ToList();
        }
    }
}
