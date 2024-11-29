using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StorageLayer
{
    public interface IStorageLayer
    {
        void CreateTable(string tableName ,List<Column> columns);
        void Insert(string tableName, object row);
        object Read(string tableName, int id);
        void Update(string tableName, int id, object newRow);
        void Delete(string tableName, int id);
    }
    public class Column
    {
    public required string Name { get; set; }
    public required string Type { get; set; }
    public bool PrimaryKey { get; set; }
    public bool Unique { get; set; }
    }
}
