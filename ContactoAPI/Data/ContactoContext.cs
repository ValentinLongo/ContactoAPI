using ContactoAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ContactoAPI.Data
{
    public class ContactoContext : DbContext
    {
        public ContactoContext(DbContextOptions<ContactoContext> options)
            : base(options)
        {
        }

        public DbSet<Contacto> Contacto { get; set; }
    }
}
