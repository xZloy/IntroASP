using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
/// <summary>
/// Summary description for PubContext
/// </summary>
public class PubContext : DbContext
{
    public PubContext() : base("name=PubContext")
    {
        Database.SetInitializer<PubContext>(null);
    }

    public DbSet<Beer> Beer { get; set; }
    public DbSet<Brand> Brand { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        modelBuilder.Entity<Beer>()
            .ToTable("Beer")
            .HasKey(b => b.BeerId)
            .HasRequired(b => b.Brand)
            .WithMany(br => br.Beers)
            .HasForeignKey(b => b.BrandId);

        modelBuilder.Entity<Brand>()
            .ToTable("Brand")
            .HasKey(br => br.BrandId);

        base.OnModelCreating(modelBuilder);
    }

}