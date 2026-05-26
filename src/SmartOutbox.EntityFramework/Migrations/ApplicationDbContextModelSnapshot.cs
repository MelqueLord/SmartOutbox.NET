using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SmartOutbox.Core.Entities;

#nullable disable

namespace SmartOutbox.EntityFramework.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("SmartOutbox.Core.Entities.Order", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<string>("CustomerName")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.Property<decimal>("Amount")
                    .HasColumnType("numeric(18,2)");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.ToTable("orders");
            });

            modelBuilder.Entity("SmartOutbox.Core.Entities.OutboxMessage", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<string>("Error")
                    .HasMaxLength(1000)
                    .HasColumnType("character varying(1000)");

                    b.Property<DateTimeOffset?>("NextAttemptAt")
                        .HasColumnType("timestamp with time zone");

                b.Property<DateTimeOffset?>("ProcessedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Payload")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<int>("RetryCount")
                    .HasColumnType("integer")
                    .HasDefaultValue(0);

                b.Property<string>("Type")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.HasIndex("ProcessedAt", "RetryCount");

                b.ToTable("outbox_messages");
            });
        }
    }
}
