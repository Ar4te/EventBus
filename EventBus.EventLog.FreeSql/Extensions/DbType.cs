namespace EventBus.EventLog.FreeSql.Extensions;

public enum DbTypeEnum
{
    MySQL,
    Oracle,
    SQLServer,
    SQLite,
    PostgreSQL
}

public static class LogTableSQLStr
{
    public static class MySQL
    {
        public static string CheckTableExists(string tableSchema) => @$"
               SELECT
                    CASE
	                WHEN Count(*) > 0 THEN 1 
                    ELSE 0 
			        END AS table_exist 
	            FROM
		            information_schema.TABLES 
	            WHERE
	            table_schema = '{tableSchema}' 
	            AND table_name = 'IntegrationEventLog'
        ";

        public static string CreateTable(string schemaName) => @$"
            CREATE TABLE IF NOT EXISTS {schemaName}.IntegrationEventLog (
                EventId CHAR(36) NOT NULL PRIMARY KEY,
                TransactionId CHAR(36) NOT NULL,
                EventTypeName VARCHAR(255) NOT NULL,
                State INT NOT NULL,
                TimesSent INT NOT NULL,
                CreationTime DATETIME NOT NULL,
                Content TEXT NOT NULL
            );
        ";
    }

    public static class Oracle
    {
        public static string CheckTableExists = "";
        public static string CreateTable = "";
    }

    public static class SQLServer
    {
        public static string CheckTableExists = "";
        public static string CreateTable = "";
    }

    public static class SQLite
    {
        public static string CheckTableExists = @$"
            SELECT
	            CASE
                WHEN COUNT(*) > 0 THEN 1
                ELSE 0
                END AS table_exist
            FROM
	            sqlite_master 
            WHERE
	            type = 'table' 
	            AND name = 'IntegrationEventLog';
        ";
        public static string CreateTable = @"
            CREATE TABLE IF NOT EXISTS IntegrationEventLog (
                EventId TEXT NOT NULL PRIMARY KEY,
                TransactionId TEXT NOT NULL,
                EventTypeName TEXT NOT NULL,
                State INTEGER NOT NULL,
                TimesSent INTEGER NOT NULL,
                CreationTime DATETIME NOT NULL,
                Content TEXT NOT NULL
            );
        ";
    }

    public static class PostgreSQL
    {
        public static string CheckTableExists(string tableSchema) => @$"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = '{tableSchema}' AND table_name = 'IntegrationEventLog'
            );
        ";

        public static string CreateTableOld = @"
            CREATE TABLE IF NOT EXISTS ""IntegrationEventLog"" (
                ""EventId"" UUID PRIMARY KEY,
                ""EventTypeName"" VARCHAR NOT NULL,
                ""State"" INTEGER NOT NULL,
                ""TimesSent"" INTEGER NOT NULL,
                ""CreationTime"" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                ""Content"" VARCHAR NOT NULL,
                ""TransactionId"" UUID NOT NULL
            );
        ";

        public static string CreateTable(string schemaName) => @$"
            CREATE TABLE IF NOT EXISTS ""{schemaName}"".""public"".""IntegrationEventLog"" (
                ""EventId"" UUID PRIMARY KEY,
                ""EventTypeName"" VARCHAR NOT NULL,
                ""State"" INTEGER NOT NULL,
                ""TimesSent"" INTEGER NOT NULL,
                ""CreationTime"" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                ""Content"" VARCHAR NOT NULL,
                ""TransactionId"" UUID NOT NULL
            );
        ";
    }
}