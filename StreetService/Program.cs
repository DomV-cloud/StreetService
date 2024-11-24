using Application.DatabaseContext;
using Domain.Entities;
using Infrastructure.DependencyInjection;
using Infrastructure.Mapping.Street;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StreetService.Middleware;

namespace StreetService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            builder.Services.AddLogging();
            builder.Services.AddInfrastructure();

            builder.Services.AddDbContext<StreetContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"),
            x => x.UseNetTopologySuite()));
            builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection("FeatureFlags"));

            builder.Services.AddAutoMapper(typeof(StreetMapperProfile));

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseCors();

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.Run();
        }
    }
}
