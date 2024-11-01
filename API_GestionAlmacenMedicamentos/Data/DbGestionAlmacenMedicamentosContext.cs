using System;
using System.Collections.Generic;
using API_GestionAlmacenMedicamentos.Models;
using Microsoft.EntityFrameworkCore;

namespace API_GestionAlmacenMedicamentos.Data;

public partial class DbGestionAlmacenMedicamentosContext : DbContext
{
    public DbGestionAlmacenMedicamentosContext()
    {
    }

    public DbGestionAlmacenMedicamentosContext(DbContextOptions<DbGestionAlmacenMedicamentosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<DetailMedicationHandlingUnit> DetailMedicationHandlingUnits { get; set; }

    public virtual DbSet<HandlingUnit> HandlingUnits { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<MedicationHandlingUnit> MedicationHandlingUnits { get; set; }

    public virtual DbSet<Movement> Movements { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Shelf> Shelves { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<TypeOfMovement> TypeOfMovements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<UserWarehouse> UserWarehouses { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-4UGNMD5\\SQLEXPRESS;Initial Catalog=DB_GestionAlmacenMedicamentos;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("Alert");

            entity.Property(e => e.AlertId).HasColumnName("alert_ID");
            entity.Property(e => e.AlertType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("alertType");
            entity.Property(e => e.BatchId).HasColumnName("batch_ID");
            entity.Property(e => e.GenerationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("generationDate");
            entity.Property(e => e.Message)
                .IsUnicode(false)
                .HasColumnName("message");
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.ToTable("Batch");

            entity.Property(e => e.BatchId).HasColumnName("batch_ID");
            entity.Property(e => e.BatchCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("batchCode");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CurrentQuantity).HasColumnName("currentQuantity");
            entity.Property(e => e.ExpirationDate).HasColumnName("expirationDate");
            entity.Property(e => e.FabricationDate).HasColumnName("fabricationDate");
            entity.Property(e => e.InitialQuantity).HasColumnName("initialQuantity");
            entity.Property(e => e.MinimumStock).HasColumnName("minimumStock");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.MedicationHandlingUnitId).HasColumnName("medication_HandlingUnit_ID");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_ID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<DetailMedicationHandlingUnit>(entity =>
        {
            entity.ToTable("Detail_Medication_HandlingUnit");

            entity.Property(e => e.DetailMedicationHandlingUnitId)
                .ValueGeneratedOnAdd()
                .HasColumnName("Detail_Medication_HandlingUnit_ID");
            entity.Property(e => e.Controlled)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("controlled");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.Oncological)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("oncological");
            entity.Property(e => e.PhotoSensitiveStorage)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("photoSensitiveStorage");
            entity.Property(e => e.StorageColdChain)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("storageColdChain");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.DetailMedicationHandlingUnitNavigation).WithOne(p => p.DetailMedicationHandlingUnit)
                .HasForeignKey<DetailMedicationHandlingUnit>(d => d.DetailMedicationHandlingUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detail_Medication_HandlingUnit_Medication_HandlingUnit");
        });

        modelBuilder.Entity<HandlingUnit>(entity =>
        {
            entity.HasKey(e => e.HandlingUnitId).HasName("PK_HandlingUnit_1");

            entity.ToTable("HandlingUnit");

            entity.Property(e => e.HandlingUnitId).HasColumnName("handlingUnitID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.NameUnit)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nameUnit");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.ToTable("Medication");

            entity.Property(e => e.MedicationId).HasColumnName("medication_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.NameMedicine)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nameMedicine");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<MedicationHandlingUnit>(entity =>
        {
            entity.ToTable("Medication_HandlingUnit");

            entity.Property(e => e.MedicationHandlingUnitId).HasColumnName("medication_HandlingUnit_ID");
            entity.Property(e => e.Concentration)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("concentration");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.HandlingUnitId).HasColumnName("handlingUnitID");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.MedicationId).HasColumnName("medication_ID");
            entity.Property(e => e.ShelfId).HasColumnName("shelf_ID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<Movement>(entity =>
        {
            entity.ToTable("Movement");

            entity.Property(e => e.MovementId).HasColumnName("movement_ID");
            entity.Property(e => e.BatchId).HasColumnName("batch_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DateOfMoviment).HasColumnName("dateOfMoviment");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TypeOfMovementId).HasColumnName("typeOfMovement_ID");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("Person");

            entity.Property(e => e.PersonId).HasColumnName("person_ID");
            entity.Property(e => e.Address)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.CellPhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("cellPhoneNumber");
            entity.Property(e => e.Ci)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CI");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("gender");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("lastName");
            entity.Property(e => e.Names)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("names");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.Photo)
                .IsUnicode(false)
                .HasColumnName("photo");
            entity.Property(e => e.SecondLastName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("secondLastName");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Report");

            entity.Property(e => e.ReportId).HasColumnName("report_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.ReportName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("reportName");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<Shelf>(entity =>
        {
            entity.ToTable("Shelf");

            entity.Property(e => e.ShelfId).HasColumnName("shelf_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .HasColumnName("is_deleted");
            entity.Property(e => e.NameShelf)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nameShelf");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_ID");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Supplier");

            entity.Property(e => e.SupplierId).HasColumnName("supplier_ID");
            entity.Property(e => e.AddressSupplier)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("addressSupplier");
            entity.Property(e => e.CellPhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("cellPhoneNumber");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.NameSupplier)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nameSupplier");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<TypeOfMovement>(entity =>
        {
            entity.ToTable("TypeOfMovement");

            entity.Property(e => e.TypeOfMovementId).HasColumnName("typeOfMovement_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DescriptionOfMovement)
                .IsUnicode(false)
                .HasColumnName("descriptionOfMovement");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.NameOfMovement)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("Entrada por compra a los proveedores\r\nEntrada por cambio de producto\r\nEntrada por bonificacion\r\n\r\nSalida para venta\r\nSalida por devolucion\r\nSalida por baja (Vencimineto)\r\n\r\n")
                .HasColumnName("nameOfMovement");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.UserId)
                .ValueGeneratedOnAdd()
                .HasColumnName("user_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.Password)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("userName");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouse");

            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_ID");
            entity.Property(e => e.AddressWarehouse)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("addressWarehouse");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
            entity.Property(e => e.NameWarehouse)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nameWarehouse");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        });

        modelBuilder.Entity<UserWarehouse>(entity =>
        {
            entity.ToTable("User_Warehouse");

            entity.HasKey(e => e.UserWarehouseId).HasName("PK_UserWarehouse");

            entity.Property(e => e.UserWarehouseId).HasColumnName("User_Warehouse_ID");

            entity.Property(e => e.UserId).HasColumnName("user_ID");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_ID");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.Property(e => e.IsDeleted)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))")
                .IsFixedLength()
                .HasColumnName("is_deleted");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}