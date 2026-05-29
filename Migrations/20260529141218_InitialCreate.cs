using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionBibliotheque.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categorie",
                columns: table => new
                {
                    ID_CATEGORIE = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LIBELLE = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_CATEGORIE);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "membre",
                columns: table => new
                {
                    ID_MEMBRE = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NOM = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PRENOM = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SEXE = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TELEPHONE = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MAIL = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ADRESSE = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DATE_INSCRIPTION = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUT = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_MEMBRE);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "type_abonnement",
                columns: table => new
                {
                    ID_TYPE = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LIBELLE = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TARIF = table.Column<float>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_TYPE);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "livre",
                columns: table => new
                {
                    ID_LIVRE = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ISBN = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TITRE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DESCRIPTION = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LANGUE = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AUTEUR = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EDITEUR = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ANNEE_PUBLICATION = table.Column<DateOnly>(type: "date", nullable: true),
                    ID_CATEGORIE = table.Column<int>(type: "int(11)", nullable: true),
                    QUANTITE_TOTAL = table.Column<decimal>(type: "decimal(20)", precision: 20, nullable: true),
                    QUANTITE_DISPONIBLE = table.Column<decimal>(type: "decimal(20)", precision: 20, nullable: true),
                    EMPLACEMENT = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    STATUT = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_LIVRE);
                    table.ForeignKey(
                        name: "livre_ibfk_1",
                        column: x => x.ID_CATEGORIE,
                        principalTable: "categorie",
                        principalColumn: "ID_CATEGORIE");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    ID_NOTIFICATION = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_MEMBRE = table.Column<int>(type: "int(11)", nullable: true),
                    ID_UTILISATEUR = table.Column<int>(type: "int", nullable: true),
                    TITRE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MESSAGE = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DATE_CREATION = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp()"),
                    DATE_LUE = table.Column<DateTime>(type: "datetime", nullable: true),
                    TYPE = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EST_LUE = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    LIEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_NOTIFICATION);
                    table.ForeignKey(
                        name: "FK_NOTIF_MEMBRE",
                        column: x => x.ID_MEMBRE,
                        principalTable: "membre",
                        principalColumn: "ID_MEMBRE");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "utilisateur",
                columns: table => new
                {
                    ID_UTILISATEUR = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LOGIN = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PASSWORD = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ROLE = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ID_MEMBRE = table.Column<int>(type: "int(11)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_UTILISATEUR);
                    table.ForeignKey(
                        name: "FK_USER_MEMBRE",
                        column: x => x.ID_MEMBRE,
                        principalTable: "membre",
                        principalColumn: "ID_MEMBRE");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "abonnement",
                columns: table => new
                {
                    ID_ABONNMENT = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_MEMBRE = table.Column<int>(type: "int(11)", nullable: false),
                    ID_TYPE = table.Column<int>(type: "int(11)", nullable: true),
                    MONTANT = table.Column<float>(type: "float", nullable: true),
                    DATE_DEBUT = table.Column<DateOnly>(type: "date", nullable: true),
                    DATE_FIN = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUT = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, defaultValueSql: "'inactif'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MODE_PAIEMENT = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    REFERENCE_PAIEMENT = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DATE_PAIEMENT = table.Column<DateTime>(type: "datetime", nullable: true),
                    STATUT_PAIEMENT = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, defaultValueSql: "'en_attente'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CODE_REFERENCE = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_ABONNMENT);
                    table.ForeignKey(
                        name: "FK_CONTENIR",
                        column: x => x.ID_TYPE,
                        principalTable: "type_abonnement",
                        principalColumn: "ID_TYPE");
                    table.ForeignKey(
                        name: "FK_SOUSCRIRE",
                        column: x => x.ID_MEMBRE,
                        principalTable: "membre",
                        principalColumn: "ID_MEMBRE");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "reservation",
                columns: table => new
                {
                    ID_RESERVATION = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_LIVRE = table.Column<int>(type: "int(11)", nullable: true),
                    ID_MEMBRE = table.Column<int>(type: "int(11)", nullable: false),
                    DATE_RESERVATION = table.Column<DateOnly>(type: "date", nullable: true),
                    STATUT = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_RESERVATION);
                    table.ForeignKey(
                        name: "FK_CIF",
                        column: x => x.ID_LIVRE,
                        principalTable: "livre",
                        principalColumn: "ID_LIVRE");
                    table.ForeignKey(
                        name: "FK_FAIRE",
                        column: x => x.ID_MEMBRE,
                        principalTable: "membre",
                        principalColumn: "ID_MEMBRE");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "alerte",
                columns: table => new
                {
                    ID_ALERTE_ = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_MEMBRE = table.Column<int>(type: "int(11)", nullable: false),
                    ID_LIVRE = table.Column<int>(type: "int(11)", nullable: false),
                    DATE_DEMANDE = table.Column<DateOnly>(type: "date", nullable: true),
                    DATE_NOTIFICATION = table.Column<DateOnly>(type: "date", nullable: true),
                    MOYEN_NOTIFICATION = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EST_TRAITEE = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "'0'"),
                    ID_NOTIFICATION = table.Column<int>(type: "int(11)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_ALERTE_);
                    table.ForeignKey(
                        name: "FK_ALERTE_NOTIF",
                        column: x => x.ID_NOTIFICATION,
                        principalTable: "notification",
                        principalColumn: "ID_NOTIFICATION",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DEMANDER",
                        column: x => x.ID_MEMBRE,
                        principalTable: "membre",
                        principalColumn: "ID_MEMBRE");
                    table.ForeignKey(
                        name: "FK_RECEVOIR",
                        column: x => x.ID_LIVRE,
                        principalTable: "livre",
                        principalColumn: "ID_LIVRE");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "emprunt",
                columns: table => new
                {
                    ID_EMPRUNT = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_MEMBRE = table.Column<int>(type: "int(11)", nullable: false),
                    DATE_EMPRUNT = table.Column<DateOnly>(type: "date", nullable: true),
                    ID_USER_EMPRUNT = table.Column<int>(type: "int(11)", nullable: false),
                    ID_USER_RETOUR = table.Column<int>(type: "int(11)", nullable: true),
                    STATUT = table.Column<string>(type: "enum('en_cours','retourne','en_retard')", nullable: true, defaultValueSql: "'en_cours'", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ID_EMPRUNT);
                    table.ForeignKey(
                        name: "FK_EFFECTUER",
                        column: x => x.ID_MEMBRE,
                        principalTable: "membre",
                        principalColumn: "ID_MEMBRE");
                    table.ForeignKey(
                        name: "FK_ENREGISTRER",
                        column: x => x.ID_USER_EMPRUNT,
                        principalTable: "utilisateur",
                        principalColumn: "ID_UTILISATEUR");
                    table.ForeignKey(
                        name: "FK_VALIDER",
                        column: x => x.ID_USER_RETOUR,
                        principalTable: "utilisateur",
                        principalColumn: "ID_UTILISATEUR");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "detail_emprunt",
                columns: table => new
                {
                    ID_EMPRUNT = table.Column<int>(type: "int(11)", nullable: false),
                    ID_LIVRE = table.Column<int>(type: "int(11)", nullable: false),
                    DATE_RETOUR_PREVU = table.Column<DateOnly>(type: "date", nullable: true),
                    DATE_RETOUR_REEL = table.Column<DateOnly>(type: "date", nullable: true),
                    NB_JOUR_RETARD = table.Column<int>(type: "int(11)", nullable: true),
                    MONTANT_PENALITE = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ID_EMPRUNT, x.ID_LIVRE })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_CIF1",
                        column: x => x.ID_LIVRE,
                        principalTable: "livre",
                        principalColumn: "ID_LIVRE");
                    table.ForeignKey(
                        name: "FK_CIF2",
                        column: x => x.ID_EMPRUNT,
                        principalTable: "emprunt",
                        principalColumn: "ID_EMPRUNT");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "FK_CONTENIR",
                table: "abonnement",
                column: "ID_TYPE");

            migrationBuilder.CreateIndex(
                name: "FK_SOUSCRIRE",
                table: "abonnement",
                column: "ID_MEMBRE");

            migrationBuilder.CreateIndex(
                name: "UK_CODE_REFERENCE",
                table: "abonnement",
                column: "CODE_REFERENCE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "FK_ALERTE_NOTIF",
                table: "alerte",
                column: "ID_NOTIFICATION");

            migrationBuilder.CreateIndex(
                name: "FK_DEMANDER",
                table: "alerte",
                column: "ID_MEMBRE");

            migrationBuilder.CreateIndex(
                name: "FK_RECEVOIR",
                table: "alerte",
                column: "ID_LIVRE");

            migrationBuilder.CreateIndex(
                name: "FK_CIF1",
                table: "detail_emprunt",
                column: "ID_LIVRE");

            migrationBuilder.CreateIndex(
                name: "FK_EFFECTUER",
                table: "emprunt",
                column: "ID_MEMBRE");

            migrationBuilder.CreateIndex(
                name: "FK_ENREGISTRER",
                table: "emprunt",
                column: "ID_USER_EMPRUNT");

            migrationBuilder.CreateIndex(
                name: "FK_VALIDER",
                table: "emprunt",
                column: "ID_USER_RETOUR");

            migrationBuilder.CreateIndex(
                name: "ID_CATEGORIE",
                table: "livre",
                column: "ID_CATEGORIE");

            migrationBuilder.CreateIndex(
                name: "FK_NOTIF_MEMBRE",
                table: "notification",
                column: "ID_MEMBRE");

            migrationBuilder.CreateIndex(
                name: "FK_CIF",
                table: "reservation",
                column: "ID_LIVRE");

            migrationBuilder.CreateIndex(
                name: "FK_FAIRE",
                table: "reservation",
                column: "ID_MEMBRE");

            migrationBuilder.CreateIndex(
                name: "FK_USER_MEMBRE",
                table: "utilisateur",
                column: "ID_MEMBRE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abonnement");

            migrationBuilder.DropTable(
                name: "alerte");

            migrationBuilder.DropTable(
                name: "detail_emprunt");

            migrationBuilder.DropTable(
                name: "reservation");

            migrationBuilder.DropTable(
                name: "type_abonnement");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "emprunt");

            migrationBuilder.DropTable(
                name: "livre");

            migrationBuilder.DropTable(
                name: "utilisateur");

            migrationBuilder.DropTable(
                name: "categorie");

            migrationBuilder.DropTable(
                name: "membre");
        }
    }
}
