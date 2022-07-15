using FizzyLogic.EventStore;
using FizzyLogic.EventStore.SampleApp.Application.CommandHandlers;
using FizzyLogic.EventStore.SampleApp.Application.Commands;
using FizzyLogic.EventStore.SampleApp.Application.Projections;
using FizzyLogic.EventStore.SampleApp.Application.Queries;
using FizzyLogic.EventStore.SampleApp.Application.QueryHandlers;
using FizzyLogic.EventStore.SampleApp.Data;
using FizzyLogic.EventStore.SampleApp.Domain.Events;
using FizzyLogic.EventStore.SampleApp.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultDatabase"));
});

builder.Services.AddEventStore<ApplicationDbContext>(options =>
{
    options.RegisterEvent<ProductRegistered>();
    options.RegisterEvent<ProductInformationUpdated>();
    options.RegisterEvent<ProductDiscontinued>();
    
    options.RegisterProjection<ProductInfoProjection>();
});

builder.Services.AddScoped<RegisterProductCommandHandler>();
builder.Services.AddScoped<UpdateProductInformationCommandHandler>();
builder.Services.AddScoped<DiscontinueProductCommandHandler>();
builder.Services.AddScoped<FindProductByIdQueryHandler>();

var app = builder.Build();

app.MapGet("/products/{id:guid}", async ([FromServices] FindProductByIdQueryHandler handler, Guid id) =>
{
    return await handler.ExecuteAsync(new FindProductById(id)) switch
    {
        { } productInfo => Results.Ok(productInfo),
        _ => Results.NotFound()
    };
}).WithName("ProductDetails");

app.MapPost("/products",
    async ([FromServices] RegisterProductCommandHandler handler, [FromBody] RegisterProductForm cmd) =>
    {
        var id = Guid.NewGuid();
        await handler.ExecuteAsync(new RegisterProduct(id, cmd.Name, cmd.Description));
        return Results.CreatedAtRoute("ProductDetails", new { id });
    });

app.MapPut("/products/{id:guid}", async ([FromServices] UpdateProductInformationCommandHandler handler,
    [FromRoute] Guid id, [FromBody] UpdateProductInformationForm form) =>
{
    await handler.ExecuteAsync(new UpdateProductInformation(id, form.Name, form.Description));
    return Results.AcceptedAtRoute("ProductDetails", new { id });
});

app.MapDelete("/products/{id:guid}", async ([FromServices] DiscontinueProductCommandHandler handler, 
    [FromRoute] Guid id) =>
{
    await handler.ExecuteAsync(new DiscontinueProduct(id));
    return Results.AcceptedAtRoute("ProductDetails", new { id });
});

// Automatically migrate the database.
using (var scope = app.Services.CreateScope())
using (var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
{
    dbContext.Database.Migrate();
}

app.Run();