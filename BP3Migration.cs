using System;
using FluentMigrator;
using FluentMigrator.Runner.Extensions;

namespace BP3Migrations.Migrations
{
    public abstract class BP3Migration : Migration
    {
        protected StoredProcedureHelper SPHelper;

        protected BP3Migration(String folder) {
            SPHelper = new StoredProcedureHelper(folder);
            SPHelper.SetMigrationContext(this);
        }

        protected void AddForeignKey(string fromTable, string foreignColumn, string toTable, string primaryColumn) {
            Create.ForeignKey("fk_" + fromTable + "_" + foreignColumn + "_" + toTable + "_" + primaryColumn)
                .FromTable(fromTable).ForeignColumn(foreignColumn)
                .ToTable(toTable).PrimaryColumn(primaryColumn);
        }

        protected void DeleteTableRows(string name, int from, int to) {
            Execute.Sql("DELETE FROM " + name + " WHERE " + name + "Id >= " + from + " AND " + name + "Id <= " + to);
        }
    }
}
