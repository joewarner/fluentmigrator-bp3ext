using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentMigrator;

/**
 * A class to help with managing Stored Procedures and their migrations.
 * 
 * Add an instance of this helper to your migration class and pass it a 
 * reference to the migration class via the SetMigrationContext method, e.g.
 * 
 *      class MigrClass {
 *          private StoredProcedureHelper spHelper = new StoredProcedureHelper():
 * 
 *          public MigrClass() {
 *              spHelper.SetMigrationContext(this);
 *          }
 *      }
 * 
 * Then you can use the helper methods in your Up()/Down() methods.
 * 
 * To install a new Stored Procedure, or a new version of an existing Stored Procedure
 * 
 *      public class Up() {
 *          spHelper.CreateStoredProcedure("<table>", "<operation>", <version>);
 *      }
 *      public class Down() {
 *          spHelper.DropStoredProcedure("<table>", "<operation>", <version>);
 *      }
 * 
 * To revise an existing Stored Procedure. This will ensure that the previous revision
 * is automatically reinstated for you.
 * 
 *      public class Up() {
 *          spHelper.UpgradeStoredProcedure("<table>", "<operation>", <version>, <revision>);
 *      }
 *      public class Down() {
 *          spHelper.DowngradeStoredProcedure("<table>", "<operation>", <version>, <revision>);
 *      }
 *
 * ALWAYS write your Stored Procedures as CREATE PROCEDURE - the helper will ensure
 * that dropping the previous revision is done for you.
 */

namespace BP3Migrations.Migrations {
    
    public class StoredProcedureHelper {
        private Migration migr;
        private string proceduresDirectory;
        private Hashtable tables = new Hashtable();

        public StoredProcedureHelper(String folder) {
            proceduresDirectory = Path.Combine(folder, "procedures");
        }

        public Migration SetMigrationContext(Migration migr) {
            /*
             * What we should do here is read the procedures directory and
             * build a list/map of all of the files by table, version and revision.
             * 
             * Then we can use this to automatically calculate how to 'downgrade'
             * procedures to correspond to an upgrade.
             */
            ReadProceduresDirectory(proceduresDirectory);

            return this.migr = migr;
        }

        public void ExecuteProcedureScript(String fileName) {
            String filePath = Path.Combine(proceduresDirectory, fileName + ".sql");
            migr.Execute.Script(filePath);
        }

        public void CreateStoredProcedure(List<ProcedureFile> procedures) {
            foreach (ProcedureFile p in procedures) {
                CreateStoredProcedure(p);
            }            
        }

        public void CreateStoredProcedure(ProcedureFile procedure) {
            CreateStoredProcedure(procedure.GetTable(), procedure.GetOperation(), procedure.GetVersion());
        }

        public void CreateStoredProcedure(String table, String operation, int version) {
            String fileName = table + "_" + operation + "_v" + version + "_000";
            String filePath = Path.Combine(proceduresDirectory, fileName + ".sql");
            migr.Execute.Script(filePath);
        }

        public void DropStoredProcedure(List<ProcedureFile> procedures) {
            foreach (ProcedureFile p in procedures) {
                DropStoredProcedure(p);
            }
        }

        public void DropStoredProcedure(ProcedureFile procedure) {
            DropStoredProcedure(procedure.GetTable(), procedure.GetOperation(), procedure.GetVersion());    
        }

        public void DropStoredProcedure(String table, String operation, int version) {
            String sql = "DROP PROCEDURE ";
            sql += "sp_" + table + "_" + operation + "_v" + version;
            migr.Execute.Sql(sql);
        }

        public void UpgradeStoredProcedure(List<ProcedureFile> procedures) {
            foreach (ProcedureFile p in procedures) {
                UpgradeStoredProcedure(p.GetTable(), p.GetOperation(), p.GetVersion(), p.GetRevision());
            }
        }

        public void UpgradeStoredProcedure(ProcedureFile procedure) {
            UpgradeStoredProcedure(procedure.GetTable(), procedure.GetOperation(), procedure.GetVersion(), procedure.GetRevision());
        }

        public void UpgradeStoredProcedure(string table, string operation, int version, int revision) {
            //Console.WriteLine("UpgradeStoredProcedure: ");

            String fileName = table + "_" + operation + "_v" + version + "_" + revision.ToString("D3");
            String filePath = Path.Combine(proceduresDirectory, fileName + ".sql");

            //Console.WriteLine("  Install from " + filePath);
            DropStoredProcedure(table, operation, version);
            migr.Execute.Script(filePath);
        }

        public void DowngradeStoredProcedure(List<ProcedureFile> procedures) {
            foreach (ProcedureFile p in procedures) {
                DowngradeStoredProcedure(p.GetTable(), p.GetOperation(), p.GetVersion(), p.GetRevision());
            }
        }

        public void DowngradeStoredProcedure(ProcedureFile procedure) {
            DowngradeStoredProcedure(procedure.GetTable(), procedure.GetOperation(), procedure.GetVersion(), procedure.GetRevision());
        }

        public void DowngradeStoredProcedure(string table, string operation, int version, int revision) {
            //Console.WriteLine("DowngradeStoredProcedure: ");
            List<int> revs = (List<int>)((Hashtable)((Hashtable)tables[table])[operation])[version];

            int prev = revs.IndexOf(revision) - 1;
            String fileName = table + "_" + operation + "_v" + version + "_" + revs[prev].ToString("D3");
            String filePath = Path.Combine(proceduresDirectory, fileName + ".sql");

            //Console.WriteLine("  Install from " + filePath);
            DropStoredProcedure(table, operation, version);
            migr.Execute.Script(filePath);
        }

        private void ReadProceduresDirectory(string dir) {
            //Console.WriteLine("ReadProceduresDirectory: " + dir);
            if (Directory.Exists(dir)) {
                //Console.WriteLine("Found " + dir + " directory");
                string[] files = Directory.GetFiles(dir);
                foreach (string f in files) {
                    var revisions = new List<int>();
                    var versions = new Hashtable();
                    var operations = new Hashtable();

                    try {
                        //Console.WriteLine("Found file: " + f);
                        ProcedureFile pf = new ProcedureFile(f);
                        //Console.WriteLine("Table: " + pf.GetTable() + ", Operation: " + pf.GetOperation() +
                        //    ", Version: " + pf.GetVersion() + ", Revision: " + pf.GetRevision());

                        string table = pf.GetTable();
                        string operation = pf.GetOperation();
                        int version = pf.GetVersion();
                        int revision = pf.GetRevision();
                        if (!tables.ContainsKey(table)) {
                            revisions.Add(revision);
                            versions.Add(version, revisions);
                            operations.Add(operation, versions);
                            tables.Add(table, operations);
                        }

                        operations = (Hashtable)tables[table];
                        if (!operations.ContainsKey(operation)) {
                            revisions.Add(revision);
                            versions.Add(version, revisions);
                            operations.Add(operation, versions);
                        }

                        versions = (Hashtable)operations[operation];
                        if (!versions.ContainsKey(version)) {
                            revisions.Add(revision);
                            versions.Add(version, revisions);
                        }

                        revisions = (List<int>)versions[version];
                        if (!revisions.Contains(revision)) {
                            revisions.Add(revision);
                            revisions.Sort();
                        }
                    }
                    catch (Exception) {
                        // Just in case we see a file that doesn't fit the naming convention
                    }
                }
                foreach (string t in tables.Keys) {
                    //Console.WriteLine("Table: " + t);
                    Hashtable op = (Hashtable)tables[t];
                    foreach (string o in op.Keys) {
                        //Console.WriteLine("  Operation: " + o);
                        Hashtable ver = (Hashtable)op[o];
                        foreach(int v in ver.Keys) {
                            //Console.WriteLine("    Version: " + v);
                            List<int> revs = (List<int>)ver[v];
                            foreach(int r in revs) {
                                //Console.WriteLine("      Revision: " + r);
                            }
                        }
                    }
                }
            }
        }
    }
}
