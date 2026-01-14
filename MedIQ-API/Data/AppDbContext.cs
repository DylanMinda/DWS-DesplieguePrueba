using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedIQ_Modelos;

    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<MedIQ_Modelos.ChatSession> ChatSessions { get; set; } = default!;

        public DbSet<MedIQ_Modelos.Mensaje> Mensajes { get; set; } = default!;

        public DbSet<MedIQ_Modelos.Usuario> Usuarios { get; set; } = default!;
    }
