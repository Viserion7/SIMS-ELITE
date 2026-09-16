namespace sims_identity.Extensions;

using Microsoft.EntityFrameworkCore;
using sims_identity.Data;


//dotnet tool install --global dotnet-ef

/*
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design 
*/

//dotnet ef migrations add "InitialCreate" //migration erstellt, im dir ist ein ordner migrations nun drinnen
//nicht init nennen weil könnte in einem anderen kontext erneut vorkommen = hinweis in dotnet error
//dotnet ef database update // nun spirhct ef mit der db und upddated das shema
//dotnet run // für echte einträge eintragen

public static class EfExtensions
{
    public static void AddEf(this WebApplicationBuilder builder)
    {


        try
        {
            string dbString = builder.Configuration["CONNECTIONSTRING"];
            if (!string.IsNullOrEmpty(dbString))
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(dbString));

            }
            else
            {
                throw new Exception("Kein ConnectionString Gefunden");
            }


        }
        catch (Exception e)
        {
            throw new Exception("Konnte keine DB verbindung aufnehmen", e);
        }



    }
}