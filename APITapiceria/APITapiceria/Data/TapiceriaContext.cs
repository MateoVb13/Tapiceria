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

        // Agregamos la tabla de horarios base
        public DbSet<HorarioBaseAgendamiento> HorariosBaseAgendamiento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar el mapeado de la tabla horarios_base_agendamiento
            modelBuilder.Entity<HorarioBaseAgendamiento>(entity =>
            {
                entity.ToTable("horarios_base_agendamiento");

                entity.HasKey(e => e.IdHorarioBase);

                entity.Property(e => e.IdHorarioBase)
                    .HasColumnName("IdHorarioBase")
                    .ValueGeneratedOnAdd(); // Usar ValueGeneratedOnAdd en lugar de UseIdentityColumn

                entity.Property(e => e.DiaSemana)
                    .HasColumnName("DiaSemana")
                    .IsRequired();

                entity.Property(e => e.HoraInicio)
                    .HasColumnName("HoraInicio")
                    .IsRequired();

                entity.Property(e => e.DuracionMinutos)
                    .HasColumnName("DuracionMinutos")
                    .IsRequired();

                entity.Property(e => e.IdServicio)
                    .HasColumnName("IdServicio")
                    .IsRequired(false);

                entity.Property(e => e.TipoDia)
                    .HasColumnName("TipoDia")
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.Activo)
                    .HasColumnName("Activo")
                    .IsRequired()
                    .HasDefaultValue(true);

                // Configurar FK a Servicios
                entity.HasOne(d => d.Servicio)
                    .WithMany()
                    .HasForeignKey(d => d.IdServicio)
                    .HasConstraintName("fk_horarios_base_servicios1")
                    .OnDelete(DeleteBehavior.SetNull);

                // Índice único compuesto
                entity.HasIndex(e => new { e.DiaSemana, e.HoraInicio, e.IdServicio })
                    .IsUnique()
                    .HasDatabaseName("UK_DiaHoraServicio"); // Usar HasDatabaseName en lugar de HasName
            });
        }
    }
}