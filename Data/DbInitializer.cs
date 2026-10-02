using KFP.Models;
using Microsoft.AspNetCore.Identity;

namespace KFP.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        await SeedRolesAndAdminAsync(services);
        SeedMenu(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@kfp.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "KFP Head Office",
                EmailConfirmed = true
            };

            // Demo credentials only - change this before deploying anywhere real.
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    private static void SeedMenu(ApplicationDbContext context)
    {
        if (context.MenuCategories.Any())
        {
            return; // already seeded
        }

        var buckets = new MenuCategory { Name = "Buckets", DisplayOrder = 1 };
        var combos = new MenuCategory { Name = "Combos", DisplayOrder = 2 };
        var sandwiches = new MenuCategory { Name = "Sandwiches", DisplayOrder = 3 };
        var sides = new MenuCategory { Name = "Sides", DisplayOrder = 4 };
        var drinks = new MenuCategory { Name = "Drinks", DisplayOrder = 5 };
        var desserts = new MenuCategory { Name = "Desserts", DisplayOrder = 6 };

        context.MenuCategories.AddRange(buckets, combos, sandwiches, sides, drinks, desserts);

        // PlaceholderText is left at its default ("No Photo Yet") - admins can upload a real
        // photo for each item from the Admin panel, which replaces the placeholder.
        var items = new List<MenuItem>
        {
            new() { Name = "Original Recipe Pork Bucket (8pc)", Description = "Eight pieces of our secret 11-herbs-and-spices crispy fried pork.", Price = 18.99m, IsFeatured = true, MenuCategory = buckets },
            new() { Name = "Extra Crispy Pork Bucket (12pc)", Description = "Twelve pieces of extra crunchy, double-fried pork bites.", Price = 24.99m, IsFeatured = true, MenuCategory = buckets },
            new() { Name = "Family Feast Bucket (16pc)", Description = "Sixteen pieces plus two large sides — feeds the whole family.", Price = 32.99m, IsFeatured = true, MenuCategory = buckets },
            new() { Name = "Spicy Inferno Bucket (8pc)", Description = "Our classic recipe kicked up with fiery chili seasoning.", Price = 19.99m, IsSpicy = true, MenuCategory = buckets },

            new() { Name = "2-Piece Combo", Description = "Two pieces of crispy pork, a side, and a drink.", Price = 9.49m, IsFeatured = true, MenuCategory = combos },
            new() { Name = "3-Piece Combo", Description = "Three pieces of crispy pork, a side, and a drink.", Price = 11.99m, MenuCategory = combos },
            new() { Name = "Crunch Box", Description = "One pork fillet, popcorn bites, fries, and a cookie.", Price = 8.99m, MenuCategory = combos },

            new() { Name = "Zinger Pork Sandwich", Description = "A spicy crispy pork fillet with lettuce and mayo on a toasted bun.", Price = 7.49m, IsSpicy = true, IsFeatured = true, MenuCategory = sandwiches },
            new() { Name = "Classic Pork Fillet Sandwich", Description = "Crispy pork fillet, pickles, and mayo on a soft bun.", Price = 6.99m, MenuCategory = sandwiches },
            new() { Name = "BBQ Pulled Pork Sandwich", Description = "Slow-cooked pulled pork tossed in smoky BBQ sauce.", Price = 7.99m, MenuCategory = sandwiches },

            new() { Name = "Mashed Potato & Gravy", Description = "Creamy mashed potatoes topped with our signature gravy.", Price = 3.49m, MenuCategory = sides },
            new() { Name = "Crispy Fries", Description = "Golden, salted, crispy-cut fries.", Price = 2.99m, MenuCategory = sides },
            new() { Name = "Coleslaw", Description = "Fresh, creamy, and crunchy classic coleslaw.", Price = 2.49m, MenuCategory = sides },
            new() { Name = "Cheesy Popcorn Bites", Description = "Bite-sized crispy pork popcorn topped with cheese sauce.", Price = 4.49m, MenuCategory = sides },

            new() { Name = "Cola (Large)", Description = "Ice-cold cola, large size.", Price = 2.49m, MenuCategory = drinks },
            new() { Name = "Iced Lemon Tea", Description = "Refreshing iced tea with a hint of lemon.", Price = 2.49m, MenuCategory = drinks },
            new() { Name = "Bottled Water", Description = "500ml bottled still water.", Price = 1.49m, MenuCategory = drinks },

            new() { Name = "Chocolate Lava Cake", Description = "Warm chocolate cake with a gooey molten center.", Price = 3.99m, MenuCategory = desserts },
            new() { Name = "Soft Serve Cone", Description = "Classic vanilla soft-serve ice cream cone.", Price = 1.99m, MenuCategory = desserts },
        };

        context.MenuItems.AddRange(items);
    }
}
