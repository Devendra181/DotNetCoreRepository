using Catalog.Core.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Catalog.Infrastructure.Data
{
    public static class TypeContextSeed
    {
        public static void SeedData(IMongoCollection<ProductType> typesCollection)
        {
            bool checkTypes = typesCollection.Find(b => true).Any();
            string path = Path.Combine("Data", "SeedData", "types.json");
            if (!checkTypes)
            {
                var typeData = File.ReadAllText(path);
                var types = JsonSerializer.Deserialize<List<ProductType>>(typeData);
                if (types != null && types.Count > 0)
                {
                    foreach (var item in types)
                    {
                        typesCollection.InsertOneAsync(item);
                    }
                }
            }
        }
    }
}
