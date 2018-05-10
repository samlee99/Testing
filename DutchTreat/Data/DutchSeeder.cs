using DutchTreat.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DutchTreat.Data
{
    public class DutchSeeder
    {
        private readonly DutchContext context;
        private readonly IHostingEnvironment hosting;
        private readonly UserManager<StoreUser> userManager;

        public DutchSeeder(DutchContext context, IHostingEnvironment hosting, UserManager<StoreUser> userManager)
        {
            this.context = context;
            this.hosting = hosting;
            this.userManager = userManager;
        }

        public async Task Seed()
        {
            context.Database.EnsureCreated();

            var user = await userManager.FindByEmailAsync("sam@sam.com");

            if (user == null)
            {
                user = new StoreUser()
                {
                    FirstName = "Sam",
                    LastName = "Lee",
                    UserName = "sam@sam.com",
                    Email = "sam@sam.com"
                };

                var result = await userManager.CreateAsync(user, "P@ssw0rd!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Failed to create default user.");
                }
            }

            if (!context.Products.Any())
            {
                // create sample data
                var filePath = Path.Combine(hosting.ContentRootPath, "Data/art.json");
                var json = File.ReadAllText(filePath);
                var products = JsonConvert.DeserializeObject<IEnumerable<Product>>(json);
                context.Products.AddRange(products);

                var order = new Order()
                {
                    OrderDate = DateTime.Now,
                    OrderNumber = "12345",
                    User = user,
                    Items = new List<OrderItem>()
                    {
                        new OrderItem()
                        {
                            Product = products.First(),
                            Quantity = 5,
                            UnitPrice = products.First().Price
                        }
                    }
                };

                context.Orders.Add(order);

                context.SaveChanges();
            }
        }
    }
}
