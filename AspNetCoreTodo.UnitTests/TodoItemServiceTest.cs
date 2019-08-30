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

        private async void ClearDataBase(DbContextOptions<AspNetCoreTodo.Data.ApplicationDbContext> options){
            using(var context = new ApplicationDbContext(options)){
                await context.Database.EnsureDeletedAsync();
            }
        }
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
            ClearDataBase(options);
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
            ClearDataBase(options);
         }


         [Fact]
         public async Task GetIncompleteItemsAsync () {

             var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DefaultConnection").Options;

            
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
                
                TodoItem todoItemMockComplete1 = new TodoItem();
                todoItemMockComplete1.Title = "Testing?";
                todoItemMockComplete1.DueAt = DateTime.Today;
                todoItemMockComplete1.UserId = fakeUser.Id;
                todoItemMockComplete1.IsDone = true;

                TodoItem todoItemMockIncomplete1 = new TodoItem();
                todoItemMockIncomplete1.Title = "Testing2";
                todoItemMockIncomplete1.DueAt = DateTime.Today.AddDays(3);
                todoItemMockIncomplete1.UserId = fakeUser.Id;
                todoItemMockIncomplete1.IsDone = false;

                TodoItem todoItemCompleto3Mock = new TodoItem();
                todoItemCompleto3Mock.Title = "Testing3";
                todoItemCompleto3Mock.DueAt = DateTime.Today.AddDays(-3);
                todoItemCompleto3Mock.UserId = fakeUser2.Id;
                todoItemCompleto3Mock.IsDone = true;
 

            // Set up a context (connection to the "DB") for writing
            using (var context = new ApplicationDbContext(options))
            {
                
                await context.Items.AddAsync(todoItemMockComplete1);  
                await context.Items.AddAsync(todoItemMockIncomplete1);  
                await context.Items.AddAsync(todoItemCompleto3Mock); 

                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
    

                var todoItemTest1 = await service.GetIncompleteItemsAsync(fakeUser);
                var todoItemTest2 = await service.GetIncompleteItemsAsync(fakeUser2);
                var cantidadItemUsuario1 = todoItemTest1.Length;
                var cantidadItemUsuario2 = todoItemTest2.Length;
                var item1 = todoItemTest1[0];

                Assert.Equal(1, cantidadItemUsuario1);
                Assert.Equal(0, cantidadItemUsuario2);
                Assert.False(item1.IsDone);
                Assert.Equal(item1.Title, todoItemMockIncomplete1.Title);
                Assert.Equal(item1.DueAt, todoItemMockIncomplete1.DueAt);
                Assert.Equal(item1.UserId, todoItemMockIncomplete1.UserId);
            }
            ClearDataBase(options);
         }
    }
}