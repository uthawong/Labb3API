using Labb3API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Labb3API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Hobby> Hobbies { get; set; }
        public DbSet<Link> Links { get; set; }

    }
}
