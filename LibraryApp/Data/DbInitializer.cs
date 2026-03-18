using Microsoft.AspNetCore.Identity;
using Library.Domain;
using Bogus;

namespace LibraryApp.Data;

public static class DbInitializer
{
    public static async Task Seed(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // ceate admin role and user
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // create admin user if not exists
        var adminEmail = "admin@library.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, "Admin123!"); // ¡Usa una clave segura!
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 3. Seed books and members
        if (context.Books.Any()) return;

        var memberFaker = new Faker<Member>()
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

        var members = memberFaker.Generate(10);
        context.Members.AddRange(members);

        var categories = new[] { "Fiction", "Sci-Fi", "History", "Tech", "Cooking" };
        var bookFaker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Commerce.ProductName())
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Isbn, f => f.Random.Replace("###-##########"))
            .RuleFor(b => b.Category, f => f.PickRandom(categories))
            .RuleFor(b => b.IsAvailable, true);

        var books = bookFaker.Generate(20);
        context.Books.AddRange(books);
        context.SaveChanges();
    }
}