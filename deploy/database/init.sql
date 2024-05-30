CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


-- ### Pagamento ###
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240113150643_BaseInicialPagamentos') THEN
    CREATE TABLE "Pagamentos" (
        "Id" uuid NOT NULL,
        "PedidoId" uuid NOT NULL,
        "Tipo" integer NOT NULL,
        "DataCriacao" timestamp with time zone NOT NULL,
        "DataAtualizacao" timestamp with time zone,
        "Valor" numeric NOT NULL,
        CONSTRAINT "PK_Pagamentos" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240113150643_BaseInicialPagamentos') THEN
    CREATE TABLE "Transacoes" (
        "Id" uuid NOT NULL,
        "PagamentoId" uuid NOT NULL,
        "Data" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Transacoes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Transacoes_Pagamentos_PagamentoId" FOREIGN KEY ("PagamentoId") REFERENCES "Pagamentos" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240113150643_BaseInicialPagamentos') THEN
    CREATE INDEX "IX_Transacoes_PagamentoId" ON "Transacoes" ("PagamentoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240113150643_BaseInicialPagamentos') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240113150643_BaseInicialPagamentos', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240303195513_AdicionandoStatusPagamento') THEN
    ALTER TABLE "Pagamentos" ADD "Status" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20240303195513_AdicionandoStatusPagamento') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20240303195513_AdicionandoStatusPagamento', '8.0.0');
    END IF;
END $EF$;
COMMIT;

