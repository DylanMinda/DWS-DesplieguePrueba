using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DWS.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<DWS.Models.ChatSession> ChatSessions { get; set; } = default!;

        public DbSet<DWS.Models.Mensaje> Mensajes { get; set; } = default!;

        public DbSet<DWS.Models.Usuario> Usuarios { get; set; } = default!;
    }
