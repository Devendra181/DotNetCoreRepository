using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().ToTable("Persons");
            modelBuilder.Entity<Country>().ToTable("Countries");

            //Seed to Countries table
            string countriesJson = System.IO.File.ReadAllText("countries.json");
            List<Country> countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson)!;

            foreach (Country country in countries) {
            modelBuilder.Entity<Country>().HasData(country);
            }


            //Seed to Countries table
            string personsJson = System.IO.File.ReadAllText("persons.json");
            List<Person> persons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personsJson)!;

            foreach (Person person in persons)
                modelBuilder.Entity<Person>().HasData(person);

            //Fluent API
            modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABC12341");

            //modelBuilder.Entity<Person>()
            //    .HasIndex(temp => temp.TIN).IsUnique();

            modelBuilder.Entity<Person>()
               .ToTable(t => t.HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8"));

            //Table Relations
            //modelBuilder.Entity<Person>(entity =>
            //{
            //    entity.HasOne<Country>(c => c.Country)
            //    .WithMany(p => p.Persons)
            //    .HasForeignKey(p => p.CountryID);
            //});
        }

        public IQueryable<Person> sp_GetAllPersons()
        {
           return Persons.FromSqlRaw("Execute [dbo].[GelAllPersons]");
        }

        public int sp_InsertPersons(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PersonID", person.PersonID),
                new SqlParameter("@PersonName", person.PersonName),
                new SqlParameter("@Email", person.Email),
                new SqlParameter("@DateOfBirth", person.DateOfBirth),
                new SqlParameter("@Gender", person.Gender),
                new SqlParameter("@CountryID", person.CountryID),
                new SqlParameter("@Address", person.Address),
                new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters)
            };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID , @PersonName , @Email, @DateOfBirth, @Gender, @CountryID , @Address , @ReceiveNewsLetters", parameters);
        }
    }
}
