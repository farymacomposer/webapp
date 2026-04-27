using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Faryma.Composer.MigrationsBundle.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:app.AccountTopUpProvider", "unspecified,donationalerts,donatty,twitch_channel_points,manual")
                .Annotation("Npgsql:Enum:app.ComposerStreamStatus", "unspecified,planned,live,completed,canceled")
                .Annotation("Npgsql:Enum:app.ComposerStreamType", "unspecified,donation,debt,charity")
                .Annotation("Npgsql:Enum:app.QueueCategory", "unspecified,out_of_queue,donation,debt")
                .Annotation("Npgsql:Enum:app.ReviewOrderStatus", "unspecified,preorder,pending,in_progress,completed,canceled")
                .Annotation("Npgsql:Enum:app.ReviewOrderType", "unspecified,out_of_queue,donation,free,charity,custom")
                .Annotation("Npgsql:Enum:app.TransactionKind", "unspecified,account_top_up,payment,reversal");

            migrationBuilder.CreateTable(
                name: "app_settings",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewOrderNominalAmount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_app_settings", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TwitchUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TwitchLogin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_DataProtectionKeys", x => x.Id));

            migrationBuilder.CreateTable(
                name: "track_artists",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_track_artists", x => x.Id));

            migrationBuilder.CreateTable(
                name: "track_countries",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_track_countries", x => x.Id));

            migrationBuilder.CreateTable(
                name: "track_genres",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_track_genres", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "app",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "app",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "app",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "app",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "app",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "composer_streams",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<ComposerStreamType>(type: "app.\"ComposerStreamType\"", nullable: false),
                    Status = table.Column<ComposerStreamStatus>(type: "app.\"ComposerStreamStatus\"", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_composer_streams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_composer_streams_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_sources",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transaction_sources_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_nicknames",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NormalizedNickname = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_nicknames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_nicknames_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrackArtistEntityUserEntity",
                schema: "app",
                columns: table => new
                {
                    AssociatedArtistsId = table.Column<long>(type: "bigint", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackArtistEntityUserEntity", x => new { x.AssociatedArtistsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_TrackArtistEntityUserEntity_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackArtistEntityUserEntity_track_artists_AssociatedArtists~",
                        column: x => x.AssociatedArtistsId,
                        principalSchema: "app",
                        principalTable: "track_artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tracks",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CoverUrl = table.Column<string>(type: "text", nullable: true),
                    ExtendedGenres = table.Column<List<string>>(type: "text[]", nullable: false),
                    AddedByUserNicknameId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tracks_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tracks_track_countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "app",
                        principalTable: "track_countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tracks_user_nicknames_AddedByUserNicknameId",
                        column: x => x.AddedByUserNicknameId,
                        principalSchema: "app",
                        principalTable: "user_nicknames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_nickname_accounts",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    Balance = table.Column<long>(type: "bigint", nullable: false),
                    UserNicknameId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_nickname_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_nickname_accounts_user_nicknames_UserNicknameId",
                        column: x => x.UserNicknameId,
                        principalSchema: "app",
                        principalTable: "user_nicknames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_orders",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    MainNickname = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MainNormalizedNickname = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationStreamId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessingStreamId = table.Column<long>(type: "bigint", nullable: true),
                    InProgressAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<ReviewOrderType>(type: "app.\"ReviewOrderType\"", nullable: false),
                    Status = table.Column<ReviewOrderStatus>(type: "app.\"ReviewOrderStatus\"", nullable: false),
                    QueueCategory = table.Column<QueueCategory>(type: "app.\"QueueCategory\"", nullable: false),
                    IsFrozen = table.Column<bool>(type: "boolean", nullable: false),
                    TrackUrl = table.Column<string>(type: "text", nullable: true),
                    TrackDurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    TrackId = table.Column<long>(type: "bigint", nullable: true),
                    NominalAmount = table.Column<long>(type: "bigint", nullable: false),
                    PayableAmount = table.Column<long>(type: "bigint", nullable: false),
                    PricingComment = table.Column<string>(type: "text", nullable: true),
                    UserComment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_review_orders_composer_streams_CreationStreamId",
                        column: x => x.CreationStreamId,
                        principalSchema: "app",
                        principalTable: "composer_streams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_orders_composer_streams_ProcessingStreamId",
                        column: x => x.ProcessingStreamId,
                        principalSchema: "app",
                        principalTable: "composer_streams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_orders_tracks_TrackId",
                        column: x => x.TrackId,
                        principalSchema: "app",
                        principalTable: "tracks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_review_orders_transaction_sources_Id",
                        column: x => x.Id,
                        principalSchema: "app",
                        principalTable: "transaction_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackArtistEntityTrackEntity",
                schema: "app",
                columns: table => new
                {
                    ArtistsId = table.Column<long>(type: "bigint", nullable: false),
                    TracksId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackArtistEntityTrackEntity", x => new { x.ArtistsId, x.TracksId });
                    table.ForeignKey(
                        name: "FK_TrackArtistEntityTrackEntity_track_artists_ArtistsId",
                        column: x => x.ArtistsId,
                        principalSchema: "app",
                        principalTable: "track_artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackArtistEntityTrackEntity_tracks_TracksId",
                        column: x => x.TracksId,
                        principalSchema: "app",
                        principalTable: "tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackEntityTrackGenreEntity",
                schema: "app",
                columns: table => new
                {
                    GenresId = table.Column<long>(type: "bigint", nullable: false),
                    TracksId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackEntityTrackGenreEntity", x => new { x.GenresId, x.TracksId });
                    table.ForeignKey(
                        name: "FK_TrackEntityTrackGenreEntity_track_genres_GenresId",
                        column: x => x.GenresId,
                        principalSchema: "app",
                        principalTable: "track_genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackEntityTrackGenreEntity_tracks_TracksId",
                        column: x => x.TracksId,
                        principalSchema: "app",
                        principalTable: "tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_track_ratings",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RatingValue = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrackId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_track_ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_track_ratings_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_track_ratings_tracks_TrackId",
                        column: x => x.TrackId,
                        principalSchema: "app",
                        principalTable: "tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_top_ups",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Provider = table.Column<AccountTopUpProvider>(type: "app.\"AccountTopUpProvider\"", nullable: false),
                    UserNicknameAccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_top_ups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_top_ups_transaction_sources_Id",
                        column: x => x.Id,
                        principalSchema: "app",
                        principalTable: "transaction_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_top_ups_user_nickname_accounts_UserNicknameAccountId",
                        column: x => x.UserNicknameAccountId,
                        principalSchema: "app",
                        principalTable: "user_nickname_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<TransactionKind>(type: "app.\"TransactionKind\"", nullable: false),
                    Credit = table.Column<long>(type: "bigint", nullable: false),
                    Debit = table.Column<long>(type: "bigint", nullable: false),
                    UserNicknameAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionSourceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transactions_transaction_sources_TransactionSourceId",
                        column: x => x.TransactionSourceId,
                        principalSchema: "app",
                        principalTable: "transaction_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transactions_user_nickname_accounts_UserNicknameAccountId",
                        column: x => x.UserNicknameAccountId,
                        principalSchema: "app",
                        principalTable: "user_nickname_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewOrderEntityUserNicknameEntity",
                schema: "app",
                columns: table => new
                {
                    ReviewOrdersId = table.Column<long>(type: "bigint", nullable: false),
                    UserNicknamesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewOrderEntityUserNicknameEntity", x => new { x.ReviewOrdersId, x.UserNicknamesId });
                    table.ForeignKey(
                        name: "FK_ReviewOrderEntityUserNicknameEntity_review_orders_ReviewOrd~",
                        column: x => x.ReviewOrdersId,
                        principalSchema: "app",
                        principalTable: "review_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewOrderEntityUserNicknameEntity_user_nicknames_UserNick~",
                        column: x => x.UserNicknamesId,
                        principalSchema: "app",
                        principalTable: "user_nicknames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RatingValue = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimestampUrl = table.Column<string>(type: "text", nullable: true),
                    ReviewOrderId = table.Column<long>(type: "bigint", nullable: true),
                    TrackId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reviews_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "app",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reviews_review_orders_ReviewOrderId",
                        column: x => x.ReviewOrderId,
                        principalSchema: "app",
                        principalTable: "review_orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_reviews_tracks_TrackId",
                        column: x => x.TrackId,
                        principalSchema: "app",
                        principalTable: "tracks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "transaction_reversals",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReversedTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    ReversalTransactionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_reversals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transaction_reversals_transaction_sources_Id",
                        column: x => x.Id,
                        principalSchema: "app",
                        principalTable: "transaction_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_reversals_transactions_ReversalTransactionId",
                        column: x => x.ReversalTransactionId,
                        principalSchema: "app",
                        principalTable: "transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_reversals_transactions_ReversedTransactionId",
                        column: x => x.ReversedTransactionId,
                        principalSchema: "app",
                        principalTable: "transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("9c3ddcde-24e7-458c-8d9c-1e5f424d3ddd"), "9C3DDCDE-24E7-458C-8D9C-1E5F424D3DDD", "Composer", "COMPOSER" },
                    { new Guid("ac0b9e85-a06f-4655-822b-9c125d8d7bb4"), "AC0B9E85-A06F-4655-822B-9C125D8D7BB4", "Moderator", "MODERATOR" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "app_settings",
                columns: new[] { "Id", "ReviewOrderNominalAmount" },
                values: new object[] { 1L, 750 });

            migrationBuilder.InsertData(
                schema: "app",
                table: "track_genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "электронное" },
                    { 2L, "фолк" },
                    { 3L, "рок" },
                    { 4L, "разное" },
                    { 5L, "джаз" },
                    { 6L, "метал" },
                    { 7L, "рэп" },
                    { 8L, "поп" },
                    { 9L, "оркестровый" },
                    { 10L, "фанк" },
                    { 11L, "мюзикл/опера" },
                    { 12L, "инди" },
                    { 13L, "поп-рок" },
                    { 14L, "шансон" },
                    { 15L, "специфическое" },
                    { 16L, "баллада" },
                    { 17L, "фортепиано" },
                    { 18L, "инструментал" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_top_ups_UserNicknameAccountId",
                schema: "app",
                table: "account_top_ups",
                column: "UserNicknameAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "app",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "app",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "app",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "app",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "app",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "app",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TwitchUserId",
                schema: "app",
                table: "AspNetUsers",
                column: "TwitchUserId",
                unique: true,
                filter: "\"TwitchUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "app",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_composer_streams_CreatedByUserId",
                schema: "app",
                table: "composer_streams",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_composer_streams_EventDate",
                schema: "app",
                table: "composer_streams",
                column: "EventDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_FamilyId",
                schema: "app",
                table: "refresh_tokens",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash",
                schema: "app",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                schema: "app",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_review_orders_CreationStreamId",
                schema: "app",
                table: "review_orders",
                column: "CreationStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_review_orders_ProcessingStreamId",
                schema: "app",
                table: "review_orders",
                column: "ProcessingStreamId");

            migrationBuilder.CreateIndex(
                name: "IX_review_orders_TrackId",
                schema: "app",
                table: "review_orders",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewOrderEntityUserNicknameEntity_UserNicknamesId",
                schema: "app",
                table: "ReviewOrderEntityUserNicknameEntity",
                column: "UserNicknamesId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_CreatedByUserId",
                schema: "app",
                table: "reviews",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_ReviewOrderId",
                schema: "app",
                table: "reviews",
                column: "ReviewOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reviews_TrackId",
                schema: "app",
                table: "reviews",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_track_artists_NormalizedName",
                schema: "app",
                table: "track_artists",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackArtistEntityTrackEntity_TracksId",
                schema: "app",
                table: "TrackArtistEntityTrackEntity",
                column: "TracksId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackArtistEntityUserEntity_UsersId",
                schema: "app",
                table: "TrackArtistEntityUserEntity",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackEntityTrackGenreEntity_TracksId",
                schema: "app",
                table: "TrackEntityTrackGenreEntity",
                column: "TracksId");

            migrationBuilder.CreateIndex(
                name: "IX_tracks_AddedByUserNicknameId",
                schema: "app",
                table: "tracks",
                column: "AddedByUserNicknameId");

            migrationBuilder.CreateIndex(
                name: "IX_tracks_CountryId",
                schema: "app",
                table: "tracks",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_tracks_CreatedByUserId",
                schema: "app",
                table: "tracks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_reversals_ReversalTransactionId",
                schema: "app",
                table: "transaction_reversals",
                column: "ReversalTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transaction_reversals_ReversedTransactionId",
                schema: "app",
                table: "transaction_reversals",
                column: "ReversedTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transaction_sources_CreatedByUserId",
                schema: "app",
                table: "transaction_sources",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_TransactionSourceId",
                schema: "app",
                table: "transactions",
                column: "TransactionSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_UserNicknameAccountId",
                schema: "app",
                table: "transactions",
                column: "UserNicknameAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_user_nickname_accounts_UserNicknameId",
                schema: "app",
                table: "user_nickname_accounts",
                column: "UserNicknameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_nicknames_NormalizedNickname",
                schema: "app",
                table: "user_nicknames",
                column: "NormalizedNickname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_nicknames_UserId",
                schema: "app",
                table: "user_nicknames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_track_ratings_CreatedByUserId",
                schema: "app",
                table: "user_track_ratings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_track_ratings_TrackId_CreatedByUserId",
                schema: "app",
                table: "user_track_ratings",
                columns: new[] { "TrackId", "CreatedByUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_top_ups",
                schema: "app");

            migrationBuilder.DropTable(
                name: "app_settings",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "app");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys",
                schema: "app");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ReviewOrderEntityUserNicknameEntity",
                schema: "app");

            migrationBuilder.DropTable(
                name: "reviews",
                schema: "app");

            migrationBuilder.DropTable(
                name: "TrackArtistEntityTrackEntity",
                schema: "app");

            migrationBuilder.DropTable(
                name: "TrackArtistEntityUserEntity",
                schema: "app");

            migrationBuilder.DropTable(
                name: "TrackEntityTrackGenreEntity",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transaction_reversals",
                schema: "app");

            migrationBuilder.DropTable(
                name: "user_track_ratings",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "app");

            migrationBuilder.DropTable(
                name: "review_orders",
                schema: "app");

            migrationBuilder.DropTable(
                name: "track_artists",
                schema: "app");

            migrationBuilder.DropTable(
                name: "track_genres",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "composer_streams",
                schema: "app");

            migrationBuilder.DropTable(
                name: "tracks",
                schema: "app");

            migrationBuilder.DropTable(
                name: "transaction_sources",
                schema: "app");

            migrationBuilder.DropTable(
                name: "user_nickname_accounts",
                schema: "app");

            migrationBuilder.DropTable(
                name: "track_countries",
                schema: "app");

            migrationBuilder.DropTable(
                name: "user_nicknames",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "app");
        }
    }
}
