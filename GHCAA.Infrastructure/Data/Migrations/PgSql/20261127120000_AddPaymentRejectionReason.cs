using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class AddPaymentRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 84.92. IF NOT EXISTS because a database built by EnsureCreated already has the column.
            migrationBuilder.Sql("""ALTER TABLE "PaymentHistories" ADD COLUMN IF NOT EXISTS "RejectionReason" character varying(500);""");

            // Payments rejected before this column existed have their reason only at the end of
            // Notes. The audit row of each rejection holds it on its own, so copy it from there and
            // strip the exact " | reason" suffix. Latest audit row wins. A malformed audit row is
            // skipped so it cannot stop startup. Rejections with no audit row keep Notes as is.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    r record;
                    meta jsonb;
                    pid integer;
                    reason text;
                BEGIN
                    FOR r IN SELECT "Metadata" FROM "ActivityLogs"
                             WHERE "ActivityType" = 'Payment Rejected' AND "Metadata" LIKE '{%'
                             ORDER BY "Timestamp" DESC, "Id" DESC
                    LOOP
                        BEGIN
                            meta := r."Metadata"::jsonb;
                            IF (meta ->> 'PaymentId') ~ '^\d{1,9}$' THEN
                                pid := (meta ->> 'PaymentId')::integer;
                                reason := LEFT(btrim(meta ->> 'Reason'), 500);
                                IF reason IS NOT NULL AND reason <> '' THEN
                                    UPDATE "PaymentHistories"
                                    SET "RejectionReason" = reason,
                                        "Notes" = CASE
                                            WHEN "Notes" = reason THEN NULL
                                            WHEN RIGHT("Notes", LENGTH(reason) + 3) = ' | ' || reason
                                                THEN LEFT("Notes", LENGTH("Notes") - LENGTH(reason) - 3)
                                            ELSE "Notes" END
                                    WHERE "Id" = pid AND "Status" = 2 AND "RejectionReason" IS NULL;
                                END IF;
                            END IF;
                        EXCEPTION WHEN others THEN
                            NULL;
                        END;
                    END LOOP;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Put the reason back at the end of Notes, where the older code kept it, before the
            // column goes. Rows that already end with " | reason" or equal it are left alone.
            migrationBuilder.Sql("""
                UPDATE "PaymentHistories"
                SET "Notes" = CASE WHEN "Notes" IS NULL OR "Notes" = '' THEN "RejectionReason"
                                   ELSE "Notes" || ' | ' || "RejectionReason" END
                WHERE "RejectionReason" IS NOT NULL
                  AND ("Notes" IS NULL
                       OR ("Notes" <> "RejectionReason"
                           AND RIGHT("Notes", LENGTH("RejectionReason") + 3) <> ' | ' || "RejectionReason"));
                """);
            migrationBuilder.Sql("""ALTER TABLE "PaymentHistories" DROP COLUMN IF EXISTS "RejectionReason";""");
        }
    }
}
