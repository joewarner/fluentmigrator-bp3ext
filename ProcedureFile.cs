using System;
using System.IO;

/**
 * The working assumption is that procedure filenames have the following format
 * 
 *      Table_Operation_vN_xxx.sql
 * 
 * where
 * 
 *      Table - is the name of the 'main' table that the SP runs against
 *      Operation - is the 'name' of the SP
 *      vN - is the 'version' of the SP interface (if you change the SP parameters then you need to increment this)
 *      xxx is the 'revision' of the SP 
 */

namespace BP3Migrations.Migrations {

    public class ProcedureFile {
        private string table, operation;
        private int version, revision;
        private string fileName;

        // If no revision is given then assume revision is 0
        public ProcedureFile(string table, string operation, int version) : this(table, operation, version, 0) {
        }

        // If no version is given then assume verion is 1
        public ProcedureFile(string table, string operation) : this(table, operation, 1) {
        }

        public ProcedureFile(string table, string operation, int version, int revision) {
            this.table = table;
            this.operation = operation;
            this.version = version;
            this.revision = revision;
        }

        public ProcedureFile(string fileName) {
            StripDirectory(fileName);
            //Console.WriteLine("fileName: " + fileName);

            // Parse out the table name
            int oIndex = this.fileName.IndexOf('_') + 1;    // '_' after table name
            table = this.fileName.Substring(0, oIndex - 1);

            // Parse out the operation name
            int vIndex = this.fileName.IndexOf('_', oIndex) + 1;     // '_' after operation
            operation = this.fileName.Substring(oIndex, vIndex - (oIndex+1));
            //Console.WriteLine("Operation: " + operation);

            // Parse out the SP version
            int rIndex = this.fileName.IndexOf('_', vIndex) + 1;    // '_' after version
            //Console.WriteLine("index = " + rIndex);
            string ver = this.fileName.Substring(vIndex + 1, rIndex - (vIndex+2));
            //Console.WriteLine(ver);
            version = Convert.ToInt32(ver);
            //Console.WriteLine("Version: " + version);

            // Parse out the SP revision for this version
            int pIndex = this.fileName.IndexOf('.');
            string rev = this.fileName.Substring(rIndex, pIndex - rIndex);
            revision = Convert.ToInt32(rev);
            //Console.WriteLine("Revision: " + revision);
        }

        public string GetFileName() {
            return fileName;    
        }

        public string GetTable() {
            return table;
        }

        public string GetOperation() {
            return operation;
        }

        public int GetVersion() {
            return version;
        }

        public int GetRevision() {
            return revision;
        }

        private string StripDirectory(string fileName) {
            this.fileName = fileName.Substring(fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            return this.fileName;
        }
    }
}
