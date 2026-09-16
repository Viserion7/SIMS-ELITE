namespace sims_stix_ingest;

using Scalar.AspNetCore;
using MongoDB.Driver;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        builder.Services.AddOpenApi();
        
        // credentials are passed as environment variables to avoid secrets in code
        var connectionString = builder.Configuration["MongoDbSettings:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "[-] MongoDB connection string empty.\n" + 
                "[!] Set 'MongoDbSettings__ConnectionString'."
            );
        }

        // 'MongoClient' is a connection pool manager, not the database context
        // 'singleton' indicates that the MongoClient should persist across the entire runtime of the application
        //      instead of being created anew on HTTP request
        builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

        // use a table data gateway for flexibility
        //      Row Data Gateway and Data Mapper (EF) need classes for each JSON object, Active Record is overkill
        // 'scoped' indicates that a new instance of the object is created on each request
        builder.Services.AddScoped<IStixGateway, StixGateway>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}