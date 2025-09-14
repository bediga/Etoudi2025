using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VcBlazor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "administrative_divisions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_divisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_administrative_divisions_administrative_divisions_parent_id",
                        column: x => x.parent_id,
                        principalTable: "administrative_divisions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "candidates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    party = table.Column<string>(type: "text", nullable: false),
                    photo = table.Column<string>(type: "text", nullable: true),
                    program = table.Column<string>(type: "text", nullable: true),
                    age = table.Column<int>(type: "integer", nullable: true),
                    profession = table.Column<string>(type: "text", nullable: true),
                    education = table.Column<string>(type: "text", nullable: true),
                    experience = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    total_votes = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "polling_stations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: true),
                    department = table.Column<string>(type: "text", nullable: true),
                    commune = table.Column<string>(type: "text", nullable: true),
                    arrondissement = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    registered_voters = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    votes_submitted = table.Column<int>(type: "integer", nullable: false),
                    turnout_rate = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scrutineers_count = table.Column<int>(type: "integer", nullable: false),
                    observers_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_polling_stations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role = table.Column<string>(type: "text", nullable: false),
                    permission = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "election_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    polling_station_id = table.Column<int>(type: "integer", nullable: true),
                    votes = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    total_votes = table.Column<int>(type: "integer", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified = table.Column<bool>(type: "boolean", nullable: false),
                    verification_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_election_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_election_results_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_results_polling_stations_polling_station_id",
                        column: x => x.polling_station_id,
                        principalTable: "polling_stations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    polling_station_id = table.Column<int>(type: "integer", nullable: true),
                    avatarPath = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    region = table.Column<string>(type: "text", nullable: true),
                    department = table.Column<string>(type: "text", nullable: true),
                    arrondissement = table.Column<string>(type: "text", nullable: true),
                    commune = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_polling_stations_polling_station_id",
                        column: x => x.polling_station_id,
                        principalTable: "polling_stations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    region_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                    table.ForeignKey(
                        name: "FK_departments_regions_region_id",
                        column: x => x.region_id,
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hourly_turnout",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    polling_station_id = table.Column<int>(type: "integer", nullable: false),
                    hour = table.Column<int>(type: "integer", nullable: false),
                    voters_count = table.Column<int>(type: "integer", nullable: false),
                    cumulative_count = table.Column<int>(type: "integer", nullable: false),
                    turnout_rate = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    recorded_by = table.Column<int>(type: "integer", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hourly_turnout", x => x.id);
                    table.CheckConstraint("CK_HourlyTurnout_Hour", "hour >= 0 AND hour <= 23");
                    table.ForeignKey(
                        name: "FK_hourly_turnout_polling_stations_polling_station_id",
                        column: x => x.polling_station_id,
                        principalTable: "polling_stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hourly_turnout_users_recorded_by",
                        column: x => x.recorded_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "result_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    polling_station_id = table.Column<int>(type: "integer", nullable: false),
                    submitted_by = table.Column<int>(type: "integer", nullable: false),
                    submission_type = table.Column<string>(type: "text", nullable: false),
                    total_votes = table.Column<int>(type: "integer", nullable: false),
                    registered_voters = table.Column<int>(type: "integer", nullable: false),
                    turnout_rate = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_result_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_result_submissions_polling_stations_polling_station_id",
                        column: x => x.polling_station_id,
                        principalTable: "polling_stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_result_submissions_users_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_result_submissions_users_verified_by",
                        column: x => x.verified_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    polling_station_id = table.Column<int>(type: "integer", nullable: false),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    votes = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_by = table.Column<int>(type: "integer", nullable: true),
                    verified = table.Column<bool>(type: "boolean", nullable: false),
                    verification_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_results_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_results_polling_stations_polling_station_id",
                        column: x => x.polling_station_id,
                        principalTable: "polling_stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_results_users_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_associations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    region_id = table.Column<int>(type: "integer", nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    commune_id = table.Column<int>(type: "integer", nullable: true),
                    polling_station_id = table.Column<int>(type: "integer", nullable: true),
                    association_type = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_associations", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_associations_polling_stations_polling_station_id",
                        column: x => x.polling_station_id,
                        principalTable: "polling_stations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_associations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "arrondissements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arrondissements", x => x.id);
                    table.ForeignKey(
                        name: "FK_arrondissements_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "result_submission_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    submission_id = table.Column<int>(type: "integer", nullable: false),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    votes = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_result_submission_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_result_submission_details_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_result_submission_details_result_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "result_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submission_documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    submission_id = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<int>(type: "integer", nullable: true),
                    upload_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checksum = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submission_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_submission_documents_result_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "result_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submission_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    submission_id = table.Column<int>(type: "integer", nullable: false),
                    candidate_id = table.Column<int>(type: "integer", nullable: false),
                    votes_received = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submission_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_submission_results_candidates_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "candidates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_submission_results_result_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "result_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verification_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    submission_id = table.Column<int>(type: "integer", nullable: false),
                    checker_id = table.Column<int>(type: "integer", nullable: true),
                    assigned_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<string>(type: "text", nullable: false),
                    verification_notes = table.Column<string>(type: "text", nullable: true),
                    completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verification_decision = table.Column<string>(type: "text", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verification_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_verification_tasks_result_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "result_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_verification_tasks_users_checker_id",
                        column: x => x.checker_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "communes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    arrondissement_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communes", x => x.id);
                    table.ForeignKey(
                        name: "FK_communes_arrondissements_arrondissement_id",
                        column: x => x.arrondissement_id,
                        principalTable: "arrondissements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "verification_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    checker_id = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verification_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_verification_history_users_checker_id",
                        column: x => x.checker_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_verification_history_verification_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "verification_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "voting_centers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    commune_id = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    polling_stations_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voting_centers", x => x.id);
                    table.ForeignKey(
                        name: "FK_voting_centers_communes_commune_id",
                        column: x => x.commune_id,
                        principalTable: "communes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "polling_stations_hierarchy",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    voting_center_id = table.Column<int>(type: "integer", nullable: false),
                    station_number = table.Column<int>(type: "integer", nullable: false),
                    registered_voters = table.Column<int>(type: "integer", nullable: false),
                    votes_submitted = table.Column<int>(type: "integer", nullable: false),
                    turnout_rate = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_polling_stations_hierarchy", x => x.id);
                    table.ForeignKey(
                        name: "FK_polling_stations_hierarchy_voting_centers_voting_center_id",
                        column: x => x.voting_center_id,
                        principalTable: "voting_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bureau_assignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    polling_station_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_by = table.Column<int>(type: "integer", nullable: false),
                    assignment_type = table.Column<string>(type: "text", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bureau_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_bureau_assignments_polling_stations_hierarchy_polling_stati~",
                        column: x => x.polling_station_id,
                        principalTable: "polling_stations_hierarchy",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bureau_assignments_users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bureau_assignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_administrative_divisions_parent_id",
                table: "administrative_divisions",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_arrondissements_department_id",
                table: "arrondissements",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_bureau_assignments_assigned_by",
                table: "bureau_assignments",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_bureau_assignments_polling_station_id",
                table: "bureau_assignments",
                column: "polling_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_bureau_assignments_user_id_polling_station_id",
                table: "bureau_assignments",
                columns: new[] { "user_id", "polling_station_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_communes_arrondissement_id",
                table: "communes",
                column: "arrondissement_id");

            migrationBuilder.CreateIndex(
                name: "IX_departments_region_id",
                table: "departments",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_results_candidate_id",
                table: "election_results",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_results_polling_station_id",
                table: "election_results",
                column: "polling_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_hourly_turnout_polling_station_id_hour",
                table: "hourly_turnout",
                columns: new[] { "polling_station_id", "hour" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hourly_turnout_recorded_by",
                table: "hourly_turnout",
                column: "recorded_by");

            migrationBuilder.CreateIndex(
                name: "IX_polling_stations_hierarchy_voting_center_id",
                table: "polling_stations_hierarchy",
                column: "voting_center_id");

            migrationBuilder.CreateIndex(
                name: "IX_regions_name",
                table: "regions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_result_submission_details_candidate_id",
                table: "result_submission_details",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_result_submission_details_submission_id_candidate_id",
                table: "result_submission_details",
                columns: new[] { "submission_id", "candidate_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_result_submissions_polling_station_id",
                table: "result_submissions",
                column: "polling_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_result_submissions_submitted_by",
                table: "result_submissions",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "IX_result_submissions_verified_by",
                table: "result_submissions",
                column: "verified_by");

            migrationBuilder.CreateIndex(
                name: "IX_results_candidate_id",
                table: "results",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_results_polling_station_id",
                table: "results",
                column: "polling_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_results_submitted_by",
                table: "results",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_permission",
                table: "role_permissions",
                columns: new[] { "role", "permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_submission_documents_submission_id",
                table: "submission_documents",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_results_candidate_id",
                table: "submission_results",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_results_submission_id",
                table: "submission_results",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_associations_polling_station_id",
                table: "user_associations",
                column: "polling_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_associations_user_id",
                table: "user_associations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_polling_station_id",
                table: "users",
                column: "polling_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_verification_history_checker_id",
                table: "verification_history",
                column: "checker_id");

            migrationBuilder.CreateIndex(
                name: "IX_verification_history_task_id",
                table: "verification_history",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_verification_tasks_checker_id",
                table: "verification_tasks",
                column: "checker_id");

            migrationBuilder.CreateIndex(
                name: "IX_verification_tasks_submission_id",
                table: "verification_tasks",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_voting_centers_commune_id",
                table: "voting_centers",
                column: "commune_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administrative_divisions");

            migrationBuilder.DropTable(
                name: "bureau_assignments");

            migrationBuilder.DropTable(
                name: "election_results");

            migrationBuilder.DropTable(
                name: "hourly_turnout");

            migrationBuilder.DropTable(
                name: "result_submission_details");

            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "submission_documents");

            migrationBuilder.DropTable(
                name: "submission_results");

            migrationBuilder.DropTable(
                name: "user_associations");

            migrationBuilder.DropTable(
                name: "verification_history");

            migrationBuilder.DropTable(
                name: "polling_stations_hierarchy");

            migrationBuilder.DropTable(
                name: "candidates");

            migrationBuilder.DropTable(
                name: "verification_tasks");

            migrationBuilder.DropTable(
                name: "voting_centers");

            migrationBuilder.DropTable(
                name: "result_submissions");

            migrationBuilder.DropTable(
                name: "communes");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "arrondissements");

            migrationBuilder.DropTable(
                name: "polling_stations");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "regions");
        }
    }
}
