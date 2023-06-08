using Common;
using Common.Identity;
using Common.MassTransit;
using Customers.Service.Contracts.Repositories;
using Customers.Service.Models;
using Customers.Service.Repositories;
using Customers.Service.Settings;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
{
    // Import the configuration from the appsettings.json file.
    var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
    
    builder.Services.AddMongo();
    builder.Services.RegisterMongoDbCollection<Customer>(mongoDbSettings!.CustomersCollectionName);
    builder.Services.RegisterMongoDbCollection<User>(mongoDbSettings!.UsersCollectionName);
    
    // Add MassTransit
    builder.Services.AddMassTransitWithRabbitMq();
    
    // Setup Authentication
    builder.Services.AddJwtBearerAuthentication();

    // Register Automapper
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // Register Repositories
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IUsersRepository, UsersRepository>();
    
    builder.Services.AddControllers();
    
    // Register Swagger UI
    builder.Services.AddEndpointsApiExplorer();
    
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });
        
        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();
}

app.Run();