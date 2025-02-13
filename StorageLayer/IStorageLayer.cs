using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StorageLayer.Indexes;

namespace StorageLayer
{
    public interface IStorageLayer
    {
        void CreateTable(string tableName ,List<Column> columns);
        void Insert(string tableName, object row);
        JObject Read(string tableName, int id);
        JArray ReadAll(string tableName);
        JArray ReadColumns(string tableName, List<string> columns);
        JArray QueryColumns(string tableName, List<string> columns, Func<JObject, bool> predicate);
        IEnumerable<JObject> Query(string tableName, Func<JObject, bool> predicate);
        void Update(string tableName, int id, object newRow);
        void Delete(string tableName, int id);
        void DropTable(string tableName);
        void CreateIndex(string tableName, string columnName);
        BTree GetIndex(string tableName, string columnName);
        void DropIndex(string tableName);
        List<string> ListIndexes();
        List<Column> GetTableDefinition(string tableName);
        bool ValidateTableName(string tableName);

    }
    public class Column
    {
    public required string Name { get; set; }
    public required string Type { get; set; }
    public bool PrimaryKey { get; set; }
    public bool Unique { get; set; }
    public bool AllowNull { get; set; } = false;
    }
}
