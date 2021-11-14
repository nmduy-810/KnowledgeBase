using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KnowledgeBase.BackendServer.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        //Instance db context
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            //Get current environment name, build configuration builder
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            //Read appsettings.json file and appsettings.development.json file
            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json")
                .Build();
            
            //Create builder
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            //Return value of connection string in application.json
            var connectionString = configurationRoot.GetConnectionString("DefaultConnection");
            
            //Cast sql server
            builder.UseSqlServer(connectionString);
            
            return new ApplicationDbContext(builder.Options);
        }
    }
}