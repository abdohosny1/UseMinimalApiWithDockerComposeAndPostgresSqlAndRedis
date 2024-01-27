using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UseDockerComposeWithPostgresSqlAndRedis.Contarcts;
using UseDockerComposeWithPostgresSqlAndRedis.Database;
using UseDockerComposeWithPostgresSqlAndRedis.Entities;

namespace UseDockerComposeWithPostgresSqlAndRedis.Endpoints
{
    public static class ProductEndpoint
    {
        public static void MapProductEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("products", async(
                CreateProductRequest request,
                ApplicationDbContext dbContext,
                CancellationToken cte
                ) =>{

                    var product = new Product
                    {
                        Name = request.Name,
                        Price = request.Price,
                    };

                    dbContext.Add(product);
                    dbContext.SaveChangesAsync(cte);
                    return Results.Ok(product);
            });

            app.MapGet("products", async (
               ApplicationDbContext dbContext,
               CancellationToken cte
               //int page,
               //int pageSize
               ) => {

                  // page = 1; pageSize = 10;
                   var products = await dbContext.Products
                       .AsNoTracking()
                       //.Skip((page - 1) * pageSize)
                       //.Take(pageSize)
                       .ToListAsync(cte);
                   return Results.Ok(products);
               });


            //app.MapGet("products/{id}", async (int id, ApplicationDbContext dbContext,
            //    IDistributedCache cache, CancellationToken ct) =>
            //{
            //    var product = await cache.GetAsync($"products-{id}",
            //        async token =>
            //        {
            //            var productFromDb = await dbContext.Products
            //                .AsNoTracking()
            //                .FirstOrDefaultAsync(p => p.Id == id, token);
            //            return productFromDb;
            //        },
            //        new DistributedCacheEntryOptions
            //        {
            //            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
            //        },
            //        ct);

            //    return product is null ? Results.NotFound() : Results.Ok(product);
            //});

            app.MapGet("products/{id}", async (int id, ApplicationDbContext dbContext,
                  IDistributedCache cache, CancellationToken cancellationToken) =>
            {
                var cacheKey = $"products-{id}";

                var productJson = await cache.GetStringAsync(cacheKey, cancellationToken);

                if (productJson is not null)
                {
                    var cachedProduct = JsonSerializer.Deserialize<Product>(productJson);

                    return Results.Ok(cachedProduct);
                }

                var productFromDb = await dbContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

                if (productFromDb is not null)
                {
                    await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(productFromDb), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
                    }, cancellationToken);

                    return Results.Ok(productFromDb);
                }

                return Results.NotFound();
            });

            app.MapPut("products/{id}", async (int id, UpdateProduct request, ApplicationDbContext context, IDistributedCache cache, CancellationToken ct) =>
            {
                var product = await context.Products.FindAsync(id, ct);

                if (product is null)
                {
                    return Results.NotFound();
                }

                product.Name = request.Name;
                product.Price = request.Price;

                await context.SaveChangesAsync(ct);

                // Optionally, you can invalidate the cache for the updated product
                var cacheKey = $"products-{id}";
                await cache.RemoveAsync(cacheKey, ct);

                return Results.NoContent();
            });

            app.MapDelete("products/{id}", async (int id, ApplicationDbContext context, IDistributedCache cache, CancellationToken ct) =>
            {
                var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

                if (product is null)
                {
                    return Results.NotFound();
                }

                context.Remove(product);
                await context.SaveChangesAsync(ct);
                await cache.RemoveAsync($"products-{id}", ct);

                return Results.NoContent();
            });






        }
    }
}
