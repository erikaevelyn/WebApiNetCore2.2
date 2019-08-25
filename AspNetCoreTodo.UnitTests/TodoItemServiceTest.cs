using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace AspNetCoreTodo.UnitTests
{
    public class TodoItemServiceTest
    {
        [Fact]
        public async Task AddNewItemAsIncompleteWithDueDate()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DefaultConnection").Options;
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };
                await service.AddItemAsync(new TodoItem
                {
                    Title = "Testing?"
                }, fakeUser);
            }
            using (var context = new ApplicationDbContext(options))
            {
            var itemsInDatabase = await context
                .Items.CountAsync();
                Assert.Equal(1, itemsInDatabase);
                var item = await context.Items.FirstAsync();
                Assert.Equal("Testing?", item.Title);
                Assert.Equal(false, item.IsDone);
                // Item should be due 3 days from now (give or take a second)
                var difference = DateTimeOffset.Now.AddDays(3) - item.DueAt;
                Assert.True(difference < TimeSpan.FromSeconds(2));
            }
        }

        // devuelve falso si se le pasa un ID queno existe
        //devuelve verdadero cuando hace un válidoartículo como completo
        [Fact]
         public async Task MarkDoneAsync () {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DefaultConnection").Options;
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };
                
                TodoItem todoItemMock = new TodoItem();
                todoItemMock.Id = Guid.NewGuid();
                todoItemMock.Title = "Testing?";

                await service.AddItemAsync(todoItemMock, fakeUser);              

                var todoItemTest = await service.MarkDoneAsync(todoItemMock.Id, fakeUser);
                Assert.False(todoItemMock.Id == Guid.NewGuid());
                Assert.True(todoItemTest);
                Assert.True(todoItemMock.IsDone);
            }

         }


         [Fact]
         public async Task GetIncompleteItemsAsync () {
             var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DefaultConnection").Options;
            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@example.com"
                };

                var fakeUser2 = new ApplicationUser
                {
                    Id = "fake-001",
                    UserName = "fake2@example.com"
                };
                
                TodoItem todoItemMock = new TodoItem();
                todoItemMock.Title = "Testing?";

                TodoItem todoItemMock2 = new TodoItem();
                todoItemMock2.Title = "Testing2";

                TodoItem todoItem3Mock = new TodoItem();
                todoItem3Mock.Title = "Testing3";

                await service.AddItemAsync(todoItemMock, fakeUser);     
                await service.AddItemAsync(todoItemMock2, fakeUser);    
                await service.AddItemAsync(todoItem3Mock, fakeUser2);           

                var todoItemTest = await service.GetIncompleteItemsAsync(fakeUser);
                var cantidadItem = todoItemTest.Length;
                Assert.Equal(2, cantidadItem);
            }
         }
    }
}