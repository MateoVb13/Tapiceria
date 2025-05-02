using APITapiceria.Models;
using Microsoft.EntityFrameworkCore;

namespace APITapiceria.Data
{
    public class TapiceriaContext : DbContext
    {
        public TapiceriaContext(DbContextOptions<TapiceriaContext> options)
            : base(options)
        {
        }

        // DbSet por cada tabla/entidad en tu base de datos
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Clientes> Clientes { get; set; }
        public DbSet<Servicios> Servicios { get; set; }
        public DbSet<Empleados> Empleados { get; set; }
        public DbSet<EmpleadoDisponibilidad> EmpleadoDisponibilidad { get; set; }
        public DbSet<Citas> Citas { get; set; }
        public DbSet<Pagos> Pagos { get; set; }


    }
}
