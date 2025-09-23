using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class GetPersons_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_GetAllPersons = @"
                CREATE PROCEDURE [dbo].[GelAllPersons]
                AS BEGIN
                    SELECT [PersonID], [PersonName], [Email], [DateOfBirth], [Gender], [CountryID], [Address], [ReceiveNewsLetters]
                    From [dbo].[Persons]
                END
                ";
            migrationBuilder.Sql(sp_GetAllPersons);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_GetAllPersons = @"DROP PROCEDURE [dbo].[GelAllPersons]";
            migrationBuilder.Sql(sp_GetAllPersons);
        }
    }
}
