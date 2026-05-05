using System;
using System.Collections.Generic;
using GestionBibliotheque.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Data;

public partial class BibliothequeDbContext : DbContext
{
    public BibliothequeDbContext(DbContextOptions<BibliothequeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Abonnement> Abonnements { get; set; }

    public virtual DbSet<Alerte> Alertes { get; set; }

    public virtual DbSet<Categorie> Categories { get; set; }

    public virtual DbSet<DetailEmprunt> DetailEmprunts { get; set; }

    public virtual DbSet<Emprunt> Emprunts { get; set; }

    public virtual DbSet<Livre> Livres { get; set; }

    public virtual DbSet<Membre> Membres { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<TypeAbonnement> TypeAbonnements { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Abonnement>(entity =>
        {
            entity.HasKey(e => e.IdAbonnment).HasName("PRIMARY");

            entity.ToTable("abonnement");

            entity.HasIndex(e => e.IdType, "FK_CONTENIR");

            entity.HasIndex(e => e.IdMembre, "FK_SOUSCRIRE");

            entity.Property(e => e.IdAbonnment)
                .HasColumnType("int(11)")
                .HasColumnName("ID_ABONNMENT");
            entity.Property(e => e.DateDebut).HasColumnName("DATE_DEBUT");
            entity.Property(e => e.DateFin).HasColumnName("DATE_FIN");
            entity.Property(e => e.IdMembre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_MEMBRE");
            entity.Property(e => e.IdType)
                .HasColumnType("int(11)")
                .HasColumnName("ID_TYPE");
            entity.Property(e => e.Montant).HasColumnName("MONTANT");
            entity.Property(e => e.Statut)
                .HasMaxLength(10)
                .HasColumnName("STATUT");

            entity.HasOne(d => d.IdMembreNavigation).WithMany(p => p.Abonnements)
                .HasForeignKey(d => d.IdMembre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SOUSCRIRE");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.Abonnements)
                .HasForeignKey(d => d.IdType)
                .HasConstraintName("FK_CONTENIR");
        });

        modelBuilder.Entity<Alerte>(entity =>
        {
            entity.HasKey(e => e.IdAlerte).HasName("PRIMARY");

            entity.ToTable("alerte");

            entity.HasIndex(e => e.IdNotification, "FK_ALERTE_NOTIF");

            entity.HasIndex(e => e.IdMembre, "FK_DEMANDER");

            entity.HasIndex(e => e.IdLivre, "FK_RECEVOIR");

            entity.Property(e => e.IdAlerte)
                .HasColumnType("int(11)")
                .HasColumnName("ID_ALERTE_");
            entity.Property(e => e.DateDemande).HasColumnName("DATE_DEMANDE");
            entity.Property(e => e.DateNotification).HasColumnName("DATE_NOTIFICATION");
            entity.Property(e => e.EstTraitee)
                .HasDefaultValueSql("'0'")
                .HasColumnName("EST_TRAITEE");
            entity.Property(e => e.IdLivre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_LIVRE");
            entity.Property(e => e.IdMembre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_MEMBRE");
            entity.Property(e => e.IdNotification)
                .HasColumnType("int(11)")
                .HasColumnName("ID_NOTIFICATION");
            entity.Property(e => e.MoyenNotification)
                .HasMaxLength(10)
                .HasColumnName("MOYEN_NOTIFICATION");

            entity.HasOne(d => d.IdLivreNavigation).WithMany(p => p.Alertes)
                .HasForeignKey(d => d.IdLivre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RECEVOIR");

            entity.HasOne(d => d.IdMembreNavigation).WithMany(p => p.Alertes)
                .HasForeignKey(d => d.IdMembre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DEMANDER");

            entity.HasOne(d => d.IdNotificationNavigation).WithMany(p => p.Alertes)
                .HasForeignKey(d => d.IdNotification)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ALERTE_NOTIF");
        });

        modelBuilder.Entity<Categorie>(entity =>
        {
            entity.HasKey(e => e.IdCategorie).HasName("PRIMARY");

            entity.ToTable("categorie");

            entity.Property(e => e.IdCategorie)
                .HasColumnType("int(11)")
                .HasColumnName("ID_CATEGORIE");
            entity.Property(e => e.Libelle)
                .HasMaxLength(20)
                .HasColumnName("LIBELLE");
        });

        modelBuilder.Entity<DetailEmprunt>(entity =>
        {
            entity.HasKey(e => new { e.IdEmprunt, e.IdLivre })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("detail_emprunt");

            entity.HasIndex(e => e.IdLivre, "FK_CIF1");

            entity.Property(e => e.IdEmprunt)
                .HasColumnType("int(11)")
                .HasColumnName("ID_EMPRUNT");
            entity.Property(e => e.IdLivre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_LIVRE");
            entity.Property(e => e.DateRetourPrevu).HasColumnName("DATE_RETOUR_PREVU");
            entity.Property(e => e.DateRetourReel).HasColumnName("DATE_RETOUR_REEL");
            entity.Property(e => e.MontantPenalite)
                .HasPrecision(10, 2)
                .HasColumnName("MONTANT_PENALITE");
            entity.Property(e => e.NbJourRetard)
                .HasColumnType("int(11)")
                .HasColumnName("NB_JOUR_RETARD");

            entity.HasOne(d => d.IdEmpruntNavigation).WithMany(p => p.DetailEmprunts)
                .HasForeignKey(d => d.IdEmprunt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIF2");

            entity.HasOne(d => d.IdLivreNavigation).WithMany(p => p.DetailEmprunts)
                .HasForeignKey(d => d.IdLivre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CIF1");
        });

        modelBuilder.Entity<Emprunt>(entity =>
        {
            entity.HasKey(e => e.IdEmprunt).HasName("PRIMARY");

            entity.ToTable("emprunt");

            entity.HasIndex(e => e.IdMembre, "FK_EFFECTUER");

            entity.HasIndex(e => e.IdUserEmprunt, "FK_ENREGISTRER");

            entity.HasIndex(e => e.IdUserRetour, "FK_VALIDER");

            entity.Property(e => e.IdEmprunt)
                .HasColumnType("int(11)")
                .HasColumnName("ID_EMPRUNT");
            entity.Property(e => e.DateEmprunt).HasColumnName("DATE_EMPRUNT");
            entity.Property(e => e.IdMembre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_MEMBRE");
            entity.Property(e => e.IdUserEmprunt)
                .HasColumnType("int(11)")
                .HasColumnName("ID_USER_EMPRUNT");
            entity.Property(e => e.IdUserRetour)
                .HasColumnType("int(11)")
                .HasColumnName("ID_USER_RETOUR");
            entity.Property(e => e.Statut)
                .HasDefaultValueSql("'en_cours'")
                .HasColumnType("enum('en_cours','retourne','en_retard')")
                .HasColumnName("STATUT");

            entity.HasOne(d => d.IdMembreNavigation).WithMany(p => p.Emprunts)
                .HasForeignKey(d => d.IdMembre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EFFECTUER");

            entity.HasOne(d => d.IdUserEmpruntNavigation).WithMany(p => p.EmpruntIdUserEmpruntNavigations)
                .HasForeignKey(d => d.IdUserEmprunt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ENREGISTRER");

            entity.HasOne(d => d.IdUserRetourNavigation).WithMany(p => p.EmpruntIdUserRetourNavigations)
                .HasForeignKey(d => d.IdUserRetour)
                .HasConstraintName("FK_VALIDER");
        });

        modelBuilder.Entity<Livre>(entity =>
        {
            entity.HasKey(e => e.IdLivre).HasName("PRIMARY");

            entity.ToTable("livre");

            entity.HasIndex(e => e.IdCategorie, "ID_CATEGORIE");

            entity.Property(e => e.IdLivre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_LIVRE");
            entity.Property(e => e.AnneePublication).HasColumnName("ANNEE_PUBLICATION");
            entity.Property(e => e.Auteur)
                .HasMaxLength(100)
                .HasColumnName("AUTEUR");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Editeur)
                .HasMaxLength(20)
                .HasColumnName("EDITEUR");
            entity.Property(e => e.Emplacement)
                .HasMaxLength(20)
                .HasColumnName("EMPLACEMENT");
            entity.Property(e => e.IdCategorie)
                .HasColumnType("int(11)")
                .HasColumnName("ID_CATEGORIE");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("ISBN");
            entity.Property(e => e.Langue)
                .HasMaxLength(10)
                .HasColumnName("LANGUE");
            entity.Property(e => e.QuantiteDisponible)
                .HasPrecision(20)
                .HasColumnName("QUANTITE_DISPONIBLE");
            entity.Property(e => e.QuantiteTotal)
                .HasPrecision(20)
                .HasColumnName("QUANTITE_TOTAL");
            entity.Property(e => e.Statut)
                .HasMaxLength(20)
                .HasColumnName("STATUT");
            entity.Property(e => e.Titre)
                .HasMaxLength(100)
                .HasColumnName("TITRE");

            entity.HasOne(d => d.IdCategorieNavigation).WithMany(p => p.Livres)
                .HasForeignKey(d => d.IdCategorie)
                .HasConstraintName("livre_ibfk_1");
        });

        modelBuilder.Entity<Membre>(entity =>
        {
            entity.HasKey(e => e.IdMembre).HasName("PRIMARY");

            entity.ToTable("membre");

            entity.Property(e => e.IdMembre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_MEMBRE");
            entity.Property(e => e.Adresse)
                .HasMaxLength(20)
                .HasColumnName("ADRESSE");
            entity.Property(e => e.DateInscription).HasColumnName("DATE_INSCRIPTION");
            entity.Property(e => e.Mail)
                .HasMaxLength(100)
                .HasColumnName("MAIL");
            entity.Property(e => e.Nom)
                .HasMaxLength(10)
                .HasColumnName("NOM");
            entity.Property(e => e.Prenom)
                .HasMaxLength(20)
                .HasColumnName("PRENOM");
            entity.Property(e => e.Sexe)
                .HasMaxLength(10)
                .HasColumnName("SEXE");
            entity.Property(e => e.Statut).HasColumnName("STATUT");
            entity.Property(e => e.Telephone)
                .HasMaxLength(15)
                .HasColumnName("TELEPHONE");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("PRIMARY");

            entity.ToTable("notification");

            entity.HasIndex(e => e.IdMembre, "FK_NOTIF_MEMBRE");

            entity.Property(e => e.IdNotification)
                .HasColumnType("int(11)")
                .HasColumnName("ID_NOTIFICATION");
            entity.Property(e => e.DateCreation)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("DATE_CREATION");
            entity.Property(e => e.DateLue)
                .HasColumnType("datetime")
                .HasColumnName("DATE_LUE");
            entity.Property(e => e.EstLue)
                .HasDefaultValueSql("'0'")
                .HasColumnName("EST_LUE");
            entity.Property(e => e.IdMembre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_MEMBRE");
            entity.Property(e => e.Lien)
                .HasMaxLength(255)
                .HasColumnName("LIEN");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("MESSAGE");
            entity.Property(e => e.Titre)
                .HasMaxLength(100)
                .HasColumnName("TITRE");
            entity.Property(e => e.Type)
                .HasMaxLength(30)
                .HasColumnName("TYPE");

            entity.HasOne(d => d.IdMembreNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdMembre)
                .HasConstraintName("FK_NOTIF_MEMBRE");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.IdReservation).HasName("PRIMARY");

            entity.ToTable("reservation");

            entity.HasIndex(e => e.IdLivre, "FK_CIF");

            entity.HasIndex(e => e.IdMembre, "FK_FAIRE");

            entity.Property(e => e.IdReservation)
                .HasColumnType("int(11)")
                .HasColumnName("ID_RESERVATION");
            entity.Property(e => e.DateReservation).HasColumnName("DATE_RESERVATION");
            entity.Property(e => e.IdLivre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_LIVRE");
            entity.Property(e => e.IdMembre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_MEMBRE");
            entity.Property(e => e.Statut)
                .HasMaxLength(10)
                .HasColumnName("STATUT");

            entity.HasOne(d => d.IdLivreNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.IdLivre)
                .HasConstraintName("FK_CIF");

            entity.HasOne(d => d.IdMembreNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.IdMembre)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FAIRE");
        });

        modelBuilder.Entity<TypeAbonnement>(entity =>
        {
            entity.HasKey(e => e.IdType).HasName("PRIMARY");

            entity.ToTable("type_abonnement");

            entity.Property(e => e.IdType)
                .HasColumnType("int(11)")
                .HasColumnName("ID_TYPE");
            entity.Property(e => e.Libelle)
                .HasMaxLength(10)
                .HasColumnName("LIBELLE");
            entity.Property(e => e.Tarif).HasColumnName("TARIF");
        });

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.IdUtilisateur).HasName("PRIMARY");

            entity.ToTable("utilisateur");

            entity.HasIndex(e => e.IdMembre, "FK_USER_MEMBRE");

            entity.Property(e => e.IdUtilisateur)
                .HasColumnType("int(11)")
                .HasColumnName("ID_UTILISATEUR");
            entity.Property(e => e.IdMembre)
                .HasColumnType("int(11)")
                .HasColumnName("ID_MEMBRE");
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .HasColumnName("LOGIN");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("ROLE");

            entity.HasOne(d => d.IdMembreNavigation).WithMany(p => p.Utilisateurs)
                .HasForeignKey(d => d.IdMembre)
                .HasConstraintName("FK_USER_MEMBRE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
