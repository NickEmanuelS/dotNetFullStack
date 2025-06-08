using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requer autenticação para todos os endpoints
    public class ProductsController : ControllerBase
    {
        // Lista de produtos em memória (em produção, usar banco de dados)
        private static List<Product> products = new();
        private static int nextProductId = 1;

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            var userId = GetCurrentUserId();
            var userProducts = products.Where(p => p.UserId == userId).ToList();
            return Ok(userProducts);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            var userId = GetCurrentUserId();
            var product = products.FirstOrDefault(p => p.Id == id && p.UserId == userId);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public ActionResult<Product> CreateProduct(Product product)
        {
            var userId = GetCurrentUserId();

            product.Id = nextProductId++;
            product.UserId = userId;
            product.CreatedAt = DateTime.Now;

            products.Add(product);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateProduct(int id, Product product)
        {
            var userId = GetCurrentUserId();
            var existingProduct = products.FirstOrDefault(p => p.Id == id && p.UserId == userId);

            if (existingProduct == null)
                return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;

            return Ok(existingProduct);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(int id)
        {
            var userId = GetCurrentUserId();
            var product = products.FirstOrDefault(p => p.Id == id && p.UserId == userId);

            if (product == null)
                return NotFound();

            products.Remove(product);
            return NoContent();
        }
    }
}