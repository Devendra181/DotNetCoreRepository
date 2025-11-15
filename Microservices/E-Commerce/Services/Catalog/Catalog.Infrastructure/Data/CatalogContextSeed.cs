using Catalog.Core.Entities;
using MongoDB.Driver;
using System.Text.Json;

namespace Catalog.Infrastructure.Data
{
    public class CatalogContextSeed
    {
        public static void SeedData(IMongoCollection<Product> productCollection)
        {
            bool checkProducts = productCollection.Find(b => true).Any();
            string path = Path.Combine("Data", "SeedsData", "product.json");
            if (!checkProducts)
            {
                var productData = File.ReadAllText(path);
                var products = JsonSerializer.Deserialize<List<Product>>(productData);
                if (products != null && products.Count > 0)
                {
                    foreach (var item in products)
                    {
                        productCollection.InsertOneAsync(item);
                    }
                }
            }

        }
    }
}
