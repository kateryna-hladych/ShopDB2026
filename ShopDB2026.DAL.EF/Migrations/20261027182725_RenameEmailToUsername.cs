//using Microsoft.EntityFrameworkCore.Migrations;

//#nullable disable

//namespace ShopDB2026.DAL.EF.Migrations
//{
//    /// <inheritdoc />
//    public partial class RenameEmailToUsername : Migration
//    {
//        /// <inheritdoc />
//        protected override void Up(MigrationBuilder migrationBuilder)
//        {

//        }

//        /// <inheritdoc />
//        protected override void Down(MigrationBuilder migrationBuilder)
//        {

//        }
//    }
//}
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopDB2026.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmailToUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "Email");
        }
    }
}