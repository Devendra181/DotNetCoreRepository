using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class InsertPersons_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_InsertAllPersons = @"
                CREATE PROCEDURE [dbo].[InsertPerson]
                (@PersonID UNIQUEIDENTIFIER, @PersonName NVARCHAR (40), @Email NVARCHAR (40), @DateOfBirth DATETIME2 (7), @Gender NVARCHAR (10), @CountryID UNIQUEIDENTIFIER, @Address NVARCHAR (200), @ReceiveNewsLetters BIT)
                AS BEGIN
                    Insert into [dbo].[Persons] ([PersonID], [PersonName], [Email], [DateOfBirth], [Gender], [CountryID], [Address], [ReceiveNewsLetters])
                    Values (@PersonID , @PersonName , @Email, @DateOfBirth, @Gender, @CountryID , @Address , @ReceiveNewsLetters)
                END
                ";
            migrationBuilder.Sql(sp_InsertAllPersons);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_InsertAllPersons = @"DROP PROCEDURE [dbo].[InsertPerson]";
            migrationBuilder.Sql(sp_InsertAllPersons);
        }
    }
}
