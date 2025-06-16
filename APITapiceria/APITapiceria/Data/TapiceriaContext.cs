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

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Clientes> Clientes { get; set; }
        public DbSet<Servicios> Servicios { get; set; }
        public DbSet<Empleados> Empleados { get; set; }
        public DbSet<EmpleadoDisponibilidad> EmpleadoDisponibilidad { get; set; }
        public DbSet<Citas> Citas { get; set; }
        public DbSet<Pagos> Pagos { get; set; }

        public DbSet<HorarioBaseAgendamiento> HorariosBaseAgendamiento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Usuarios>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(e => e.IdUsuario);
            });

            modelBuilder.Entity<Clientes>(entity =>
            {
                entity.ToTable("clientes");
                entity.HasKey(e => e.IdCliente);
            });

            modelBuilder.Entity<Servicios>(entity =>
            {
                entity.ToTable("servicios");
                entity.HasKey(e => e.IdServicio);
            });

            modelBuilder.Entity<Empleados>(entity =>
            {
                entity.ToTable("empleados");
                entity.HasKey(e => e.IdEmpleado);

                entity.Property(e => e.IdEmpleado)
                    .HasColumnName("IdEmpleado")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.NombreCompleto)
                    .HasColumnName("NombreCompleto")
                    .IsRequired();

                entity.Property(e => e.Especialidad)
                    .HasColumnName("Especialidad");

                entity.Property(e => e.Contacto)
                    .HasColumnName("Contacto");


            });

            modelBuilder.Entity<EmpleadoDisponibilidad>(entity =>
            {
                entity.ToTable("empleado_disponibilidad");

                entity.HasKey(e => e.IdEmpleadoDisponibilidad);

                entity.Property(e => e.IdEmpleadoDisponibilidad)
                    .HasColumnName("IdEmpleadoDisponibilidad")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdEmpleado)
                    .HasColumnName("IdEmpleado")
                    .IsRequired();

                entity.Property(e => e.DiaSemana)
                    .HasColumnName("DiaSemana")
                    .IsRequired();

                entity.Property(e => e.HoraInicio)
                    .HasColumnName("HoraInicio")
                    .IsRequired();

                entity.Property(e => e.HoraFin)
                    .HasColumnName("HoraFin")
                    .IsRequired();


                entity.HasOne(d => d.Empleado)
                    .WithMany()
                    .HasForeignKey(d => d.IdEmpleado)
                    .HasConstraintName("fk_empleado_disponibilidad_empleados1")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Citas>(entity =>
            {
                entity.ToTable("citas");
                entity.HasKey(e => e.IdCita);
            });

            modelBuilder.Entity<Pagos>(entity =>
            {
                entity.ToTable("pagos");
                entity.HasKey(e => e.IdPago);
            });

            modelBuilder.Entity<HorarioBaseAgendamiento>(entity =>
            {
                entity.ToTable("horarios_base_agendamiento");

                entity.HasKey(e => e.IdHorarioBase);

                entity.Property(e => e.IdHorarioBase)
                    .HasColumnName("IdHorarioBase")
                    .ValueGeneratedOnAdd();

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

                entity.HasOne(d => d.Servicio)
                    .WithMany()
                    .HasForeignKey(d => d.IdServicio)
                    .HasConstraintName("fk_horarios_base_servicios1")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.DiaSemana, e.HoraInicio, e.IdServicio })
                    .IsUnique()
                    .HasDatabaseName("UK_DiaHoraServicio");
            });
        }
    }
}