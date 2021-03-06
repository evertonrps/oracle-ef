﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.EntityFrameworkCore.Metadata;
using Oracle_EF;

namespace Oracle_EF.Migrations
{
    [DbContext(typeof(Program.BloggingContext))]
    [Migration("20200209004135_Inicial")]
    partial class Inicial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("Oracle_EF.Program+Blog", b =>
                {
                    b.Property<int>("BlogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("BLOGID")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Url")
                        .HasColumnName("URL");

                    b.HasKey("BlogId");

                    b.ToTable("BLOG");
                });

            modelBuilder.Entity("Oracle_EF.Program+Post", b =>
                {
                    b.Property<int>("PostId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("POSTID");

                    b.Property<int>("BlogId")
                        .HasColumnName("BLOGID");

                    b.Property<string>("Content")
                        .HasColumnName("CONTENT");

                    b.Property<string>("Title")
                        .HasColumnName("TITLE");

                    b.HasKey("PostId");

                    b.HasIndex("BlogId");

                    b.ToTable("POSTS");
                });

            modelBuilder.Entity("Oracle_EF.Program+Post", b =>
                {
                    b.HasOne("Oracle_EF.Program+Blog", "Blog")
                        .WithMany("Posts")
                        .HasForeignKey("BlogId")
                        .HasConstraintName("FK_POSTS_BLOG_BLOGID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
