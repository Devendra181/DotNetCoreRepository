using Microsoft.AspNetCore.Mvc;
using ProductManagementAPI.Data;
using ProductManagementAPI.Models;

namespace ProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------- 1. ADDED STATE -------------------
        [HttpPost("AddProduct")]
        public IActionResult AddProduct([FromBody] Product product)
        {
            /*
             * Step 1: When a new entity (product) is created and added using .Add(),
             *         EF Core marks it as 'Added' in the ChangeTracker.
             * Step 2: When SaveChanges() is called, EF Core executes an INSERT statement.
             * Step 3: After successful save, EF Core automatically changes its state to 'Unchanged'
             *         because now it matches the database record.
             */

            _context.Products.Add(product); // State => Added
            var beforeSave = _context.Entry(product).State.ToString(); // "Added"

            _context.SaveChanges(); // Executes INSERT

            var afterSave = _context.Entry(product).State.ToString(); // "Unchanged"

            return Ok(new
            {
                Message = "New Product Added Successfully",
                StateBeforeSave = beforeSave,
                StateAfterSave = afterSave
            });
        }

        // ------------------- 2. UNCHANGED STATE -------------------
        [HttpGet("GetProduct/{id}")]
        public IActionResult GetProduct(int id)
        {
            /*
             * Step 1: When you fetch an entity using Find() or FirstOrDefault(),
             *         EF Core starts tracking it immediately.
             * Step 2: Since the entity is freshly loaded and no property has changed yet,
             *         its state is 'Unchanged'.
             * Step 3: If you later modify it, the state will automatically become 'Modified'.
             */

            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound("Product not found.");

            var stateAfterFetch = _context.Entry(product).State.ToString(); // "Unchanged"

            return Ok(new
            {
                product,
                StateAfterFetching = stateAfterFetch
            });
        }

        // ------------------- 3. MODIFIED STATE -------------------
        [HttpPut("UpdateProduct/{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            /*
             * Step 1: Retrieve an existing product → EF Core sets its state as 'Unchanged'.
             * Step 2: When you modify one or more properties (like Price or Stock),
             *         EF Core automatically changes the entity's state to 'Modified'.
             * Step 3: When SaveChanges() is called, EF Core executes an UPDATE statement.
             * Step 4: After saving successfully, EF Core resets its state back to 'Unchanged'.
             */

            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound("Product not found.");

            var stateAfterFetch = _context.Entry(product).State.ToString(); // "Unchanged"

            // Modify one or more properties
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;

            var stateAfterModify = _context.Entry(product).State.ToString(); // "Modified"

            _context.SaveChanges(); // Executes UPDATE command

            var stateAfterSave = _context.Entry(product).State.ToString(); // "Unchanged" again

            return Ok(new
            {
                Message = "Product Updated Successfully",
                StateAfterFetching = stateAfterFetch,
                StateAfterModification = stateAfterModify,
                StateAfterSaveChanges = stateAfterSave
            });
        }

        // ------------------- 4. DELETED STATE -------------------
        [HttpDelete("DeleteProduct/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            /*
             * Step 1: When you fetch an entity, EF Core marks it as 'Unchanged'.
             * Step 2: When you call .Remove(), EF Core marks it as 'Deleted'.
             * Step 3: When SaveChanges() is called, EF Core executes a DELETE SQL command.
             * Step 4: After deletion, EF Core changes its state to 'Detached'
             *         because it no longer exists in the database.
             */

            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound("Product not found.");

            var stateAfterFetch = _context.Entry(product).State.ToString(); // "Unchanged"

            _context.Products.Remove(product); // Marks entity as Deleted
            var stateAfterRemove = _context.Entry(product).State.ToString(); // "Deleted"

            _context.SaveChanges(); // Executes DELETE command
            var stateAfterSave = _context.Entry(product).State.ToString(); // "Detached" (no longer tracked)

            return Ok(new
            {
                Message = "Product Deleted Successfully",
                StateAfterFetching = stateAfterFetch,
                StateAfterRemove = stateAfterRemove,
                StateAfterSaveChanges = stateAfterSave
            });
        }

        // ------------------- 5. DETACHED STATE -------------------
        [HttpGet("DetachedExample")]
        public IActionResult DetachedExample()
        {
            /*
             * Step 1: When you create a new entity instance manually and do not attach or add it to the DbContext,
             *         EF Core has no tracking information for it → its state is 'Detached'.
             * Step 2: Detached means EF Core won’t insert, update, or delete it unless you explicitly attach it.
             */

            var product = new Product
            {
                Id = 100,
                Name = "Portable Charger",
                Price = 2500,
                Stock = 15
            };

            var initialState = _context.Entry(product).State.ToString(); // "Detached"

            // Now attach it manually (simulate connecting it to context)
            _context.Attach(product);
            var stateAfterAttach = _context.Entry(product).State.ToString(); // "Unchanged"

            // Modify property to see state transition
            product.Price = 3000;
            var stateAfterModification = _context.Entry(product).State.ToString(); // "Modified"

            _context.SaveChanges(); // Executes UPDATE command if entity exists
            var stateAfterSave = _context.Entry(product).State.ToString(); // "Unchanged" again

            return Ok(new
            {
                Message = "Detached Entity Demonstration Complete",
                InitialState = initialState,
                AfterAttach = stateAfterAttach,
                AfterModification = stateAfterModification,
                AfterSaveChanges = stateAfterSave
            });
        }

        // ------------------- 6. SHOW ALL TRACKED ENTITIES -------------------
        [HttpGet("ShowTrackedEntities")]
        public IActionResult ShowTrackedEntities()
        {
            /*
             * Displays all entities currently tracked by the DbContext's ChangeTracker,
             * along with their entity type and current state.
             * This helps visualize which entities EF Core is keeping in memory and how it perceives them.
             */

            var trackedEntities = _context.ChangeTracker.Entries()
                .Select(e => new
                {
                    EntityName = e.Entity.GetType().Name,
                    CurrentState = e.State.ToString()
                }).ToList();

            return Ok(trackedEntities);
        }
    }
}

