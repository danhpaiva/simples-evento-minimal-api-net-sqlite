using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimplesEventoApi.Models;

namespace SimplesEventoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<SimplesEventoApi.Models.Evento> Evento { get; set; } = default!;
        public DbSet<SimplesEventoApi.Models.Ingresso> Ingresso { get; set; } = default!;
        public DbSet<SimplesEventoApi.Models.Local> Local { get; set; } = default!;
        public DbSet<SimplesEventoApi.Models.Participante> Participante { get; set; } = default!;
    }
}
