using Microsoft.EntityFrameworkCore;
using LibraryApp.Controllers;
using LibraryApp.Data;
using Library.Domain;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Tests;

public class LoanTests
{
    private ApplicationDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new ApplicationDbContext(options);
        databaseContext.Database.EnsureCreated();
        return databaseContext;
    }

    [Fact]
    public async Task CreateLoan_ShouldFail_IfBookIsAlreadyOnLoan()
    {
        // Arrange 
        var context = GetDatabaseContext();
        var controller = new LoansController(context);

        var book = new Book { Id = 1, Title = "Test Book", IsAvailable = false };
        context.Books.Add(book);
        context.Members.Add(new Member { Id = 1, FullName = "Test Member" });
        context.Loans.Add(new Loan { BookId = 1, MemberId = 1, ReturnedDate = null }); // Préstamo activo
        await context.SaveChangesAsync();

        var newLoan = new Loan { BookId = 1, MemberId = 1 };

        // Act 
        var result = await controller.Create(newLoan);

        // Assert 
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CreateLoan_ShouldSucceed_IfBookIsAvailable()
    {
        // Arrange (
        var context = GetDatabaseContext();
        var controller = new LoansController(context);

        // a book is available
        var book = new Book { Id = 2, Title = "Available Book", IsAvailable = true };
        context.Books.Add(book);
        context.Members.Add(new Member { Id = 2, FullName = "Another Member" });
        await context.SaveChangesAsync();

        var newLoan = new Loan { BookId = 2, MemberId = 2 };

        // Act 
        var result = await controller.Create(newLoan);

        // Assert 
        //
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }

}