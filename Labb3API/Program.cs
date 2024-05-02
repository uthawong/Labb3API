
using Labb3API.Data;
using Labb3API.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Labb3API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); /////kopplar med DefaultConnection i appsettings.json

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            /////////////////////////////////////////////////////////////////////////////////////////
            ////PERSONS

            // Create a new person
            app.MapPost("/persons", async (Person person, ApplicationDbContext context) =>
            {
                context.Persons.Add(person);
                await context.SaveChangesAsync();

                return Results.Created($"/persons/{person.PersonId}", person);
            });

            // Get a person by id
            app.MapGet("/persons", async (ApplicationDbContext context) =>
            {
                //List all Persons in DB
                var persons = await context.Persons
                // If no data in Persons
                 .Include(h => h.Hobbies)              
                 .ThenInclude(l => l.Links).ToListAsync();
                if (persons == null || !persons.Any())
                {
                    return Results.NotFound("No person found");
                }
                return Results.Ok(persons);
            });


            /////////////////////////////////////////////////////////////////////////////////////////////
            ///HOBBIES

            // Create a new hobby
            app.MapPost("/hobbies", async (Hobby hobby, ApplicationDbContext context) =>
            {
                context.Hobbies.Add(hobby);
                await context.SaveChangesAsync();
                return Results.Created($"/hobbies/{hobby.HobbyId}", hobby);
            });

            // Get a hobby by id
            app.MapGet("/hobbies", async (ApplicationDbContext context) =>
            {
                ////List all Hobbies in DB
                var hobbies = await context.Hobbies.ToListAsync();
                //// If no data in Hobbies
                if (hobbies == null || !hobbies.Any())              
                {
                    return Results.NotFound("No interests found");
                }
                return Results.Ok(hobbies);
            });

            /////////////////////////////////////////////////////////////////////////////////////////////////
            /////Get all hobbies for a specific person  

            app.MapGet("/persons/{personId}/hobbies", async (int personId, ApplicationDbContext context) =>
            {
                var person = await context.Persons
                .Include(h => h.Hobbies)
                .FirstOrDefaultAsync(p => p.PersonId == personId);
                if (person == null)
                {
                    return Results.NotFound("No person found");
                }
                var hobbeis = person.Hobbies;
                return Results.Ok(hobbeis);
            });


            /////////////////////////////////////////////////////////////////////////////////
            ////////Get all links for a specific person 

            app.MapGet("/person/{personId}/links", async (int personId, ApplicationDbContext context) =>
            {
                var person = await context.Persons
                .Include(h => h.Hobbies)
                .ThenInclude(l => l.Links)
                .FirstOrDefaultAsync(p => p.PersonId == personId);
                if (person == null)
                {
                    return Results.NotFound("No person found");
                }
                var links = person.Hobbies.SelectMany(l => l.Links);
                return Results.Ok(links);
            });

            /////////////////////////////////////////////////////////////////////////////////////
            ///////  Connect a person to a new hobby  
            app.MapPost("/persons/{personId}/hobbies", async (int personId, Hobby hobby, ApplicationDbContext context) =>
            {
                var person = await context.Persons.FindAsync(personId);

                if (person == null)
                {
                    return Results.NotFound($"Person with ID {personId} not found.");
                }

                //Controlling if a person's hobby list is null and create a new list if needed
                if (person.Hobbies == null)
                {
                    person.Hobbies = new List<Hobby>();
                }

                //Control if hobby already exist in the database 
                var existingHobby = await context.Hobbies.FirstOrDefaultAsync(h => h.HobbyTitle == hobby.HobbyTitle);

                if (existingHobby == null)
                {
                    // If hobby does not exist, create hobby in database
                    existingHobby = new Hobby { HobbyTitle = hobby.HobbyTitle, HobbyDescription = hobby.HobbyDescription };
                    await context.Hobbies.AddAsync(existingHobby);
                }

                // Add hobby that already exist in person's hobby list
                person.Hobbies.Add(existingHobby);

                await context.SaveChangesAsync();
                return Results.Created($"/persons/{personId}/hobbies", person.Hobbies);
            });



            ///////////////////////////////////////////////////////////////////////////////////////////////
            //////// Endpoint for creating a link for a hobby  

            app.MapPost("/hobbies/{hobbyId}/links", async (int hobbyId, Link link, ApplicationDbContext context) =>
            {
                // Hitta hobbyn i databasen
                var hobby = await context.Hobbies.FindAsync(hobbyId);

                if (hobby == null)
                {
                    return Results.NotFound($"Hobby with ID {hobbyId} not found.");
                }
                // Controlling if hobby already has links 
                if (hobby.Links == null)
                {
                    hobby.Links = new List<Link>();
                }

                //Create a new link with the information
                var newLink = new Link { LinkUrl = link.LinkUrl };

                //Add new link to hobby's list of links
                hobby.Links.Add(newLink);

                //Save changes in database 
                await context.SaveChangesAsync();

                return Results.Created($"/hobbies/{hobbyId}/links", newLink);

            });
            app.Run();
        }
    }
}
