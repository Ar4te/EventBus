namespace EventBus.EventLog.EFCore.Extensions;

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
        public static string CheckTableExists = "";
        public static string CreateTable = "";
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
        public static string CheckTableExists = "";
        public static string CreateTable = "";
    }

    public static class PostgreSQL
    {
        public static string CheckTableExists = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_schema = 'public' AND table_name = 'IntegrationEventLog'
                );
            ";
        public static string CreateTable = @"
                    CREATE TABLE ""IntegrationEventLog"" (
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