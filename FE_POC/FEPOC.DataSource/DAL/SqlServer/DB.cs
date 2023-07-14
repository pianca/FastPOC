using System;
using System.Collections.Generic;
using FEPOC.DataSource.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FEPOC.DataSource.DAL.SqlServer;

public partial class DB : DbContext
{
    public DB()
    {
    }

    public DB(DbContextOptions<DB> options)
        : base(options)
    {
    }

    public virtual DbSet<Aree> Arees { get; set; }

    public virtual DbSet<ChangedRecordsQueue> ChangedRecordsQueues { get; set; }

    public virtual DbSet<Insediamenti> Insediamentis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=escape_ci_biz;User Id=solari;Password=Udine%123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Aree>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK_AREE_ID")
                .IsClustered(false);

            entity.ToTable("AREE", tb => tb.HasTrigger("Trigger_AreeChanges"));

            entity.HasIndex(e => e.Descr, "IDXDESCR")
                .IsUnique()
                .IsClustered();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Contapostidisp)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("('N')")
                .HasColumnName("CONTAPOSTIDISP");
            entity.Property(e => e.Descr)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("DESCR");
            entity.Property(e => e.Idcalend).HasColumnName("IDCALEND");
            entity.Property(e => e.Idinsediamento)
                .HasDefaultValueSql("(1)")
                .HasColumnName("IDINSEDIAMENTO");
            entity.Property(e => e.Maxpostidisp).HasColumnName("MAXPOSTIDISP");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("('P')")
                .HasColumnName("TIPO");
        });

        modelBuilder.Entity<ChangedRecordsQueue>(entity =>
        {
            entity.ToTable("ChangedRecordsQueue");

            entity.Property(e => e.ChangeType)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.LocalSync)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("('F')");
            entity.Property(e => e.RecordTable)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RecordValue)
                .HasMaxLength(4000)
                .IsUnicode(false);
            entity.Property(e => e.RemoteSync)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("('F')");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Insediamenti>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__INSEDIAMENTI__0BC6C43E");

            entity.ToTable("INSEDIAMENTI", tb => tb.HasTrigger("Trigger_INSEDIAMENTI_Changes"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Codice)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CODICE");
            entity.Property(e => e.Descr)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DESCR");
            entity.Property(e => e.Idcontrollovarchiconfig).HasColumnName("IDCONTROLLOVARCHICONFIG");
            entity.Property(e => e.Idpolicyaccesso).HasColumnName("IDPOLICYACCESSO");
            entity.Property(e => e.Idportineria).HasColumnName("IDPORTINERIA");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
