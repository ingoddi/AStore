using AStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AStore.Services.TablesServices
{

    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        public string CategoryName { get; set; } 
    }

    public class ProductService
    {
        private DatabaseManager _dbManager;

        public ProductService(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public List<ProductDTO> GetAllProducts()
        {
            var query = from product in _dbManager.Products
                        join category in _dbManager.Categories
                        on product.CategoryId equals category.CategoryId
                        select new ProductDTO
                        {
                            ProductId = product.ProductId,
                            Name = product.Name,
                            Price = product.Price,
                            Description = product.Description,
                            ImagePath = product.ImagePath,
                            CategoryName = category.Name 
                        };

            return query.ToList();
        }

        public Product AddProduct(Product product)
        {
            _dbManager.Products.Add(product);
            _dbManager.SaveChanges();
            return product;
        }

        public bool DeleteProduct(int productId)
        {
            var product = _dbManager.Products.Find(productId);
            if (product != null)
            {
                _dbManager.Products.Remove(product);
                _dbManager.SaveChanges();
                return true;
            }

            return false;
        }

        public Product UpdateProduct(Product updatedProduct)
        {
            var existingProduct = _dbManager.Products.Find(updatedProduct.ProductId);
            if (existingProduct != null)
            {
                existingProduct.Name = updatedProduct.Name;
                existingProduct.Description = updatedProduct.Description;
                existingProduct.Price = updatedProduct.Price;
                existingProduct.ImagePath = updatedProduct.ImagePath;
                existingProduct.CategoryId = updatedProduct.CategoryId;

                _dbManager.SaveChanges();

                return existingProduct;
            }

            return null;
        }

        public Product GetProductById(int productId)
        {
            return _dbManager.Products.FirstOrDefault(p => p.ProductId == productId);
        }
    }
}
