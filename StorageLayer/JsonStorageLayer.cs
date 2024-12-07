using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.XPath;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StorageLayer
{
    public class JsonStorageLayer : IStorageLayer
    {

        private readonly string _storagePath;

        public JsonStorageLayer(string storagePath){
            _storagePath = storagePath;

             if (!Directory.Exists(_storagePath)){
                Directory.CreateDirectory(_storagePath);  
            }
        }
        
        public void CreateTable(string tableName, List<Column> columns){
            var tableDefinition = new{
                TableName = tableName,
                Columns = JArray.FromObject(columns),
                Data = new JArray()
            };

            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            if (File.Exists(filePath)){
                throw new InvalidOperationException($"{tableName} already exists");
            }

            string jsonTableDefinition = JsonConvert.SerializeObject(tableDefinition, Formatting.Indented);
            File.WriteAllText(filePath, jsonTableDefinition);
        }

        public void Insert(string tableName, object row){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            string stringRow = Utils.CreateJsonObject(row);
            JObject jsonRow = JsonConvert.DeserializeObject<JObject>(stringRow);

            JArray tableData = Utils.ReadDataFromFile(filePath);
            
            Utils.ValidateRow(tableName, jsonRow, filePath);

            tableData.Add(jsonRow);  
            Utils.WriteDataToFile(filePath, tableData);
        }

        public JObject Read(string tableName, int id){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            JArray tableData = Utils.ReadDataFromFile(filePath);

            if(id >= 0 && id < tableData.Count){
                 JObject item =  (JObject)tableData[id];
                 return item;
            }
            else
                return new JObject();
        }

        public JArray ReadAll(string tableName){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            return Utils.ReadDataFromFile(filePath);
        }

        public IEnumerable<JObject> Query(string tableName, Func<JObject, bool> predicate){
            JArray tableData = ReadAll(tableName);
            var results =  tableData.OfType<JObject>().Where(predicate);
            return results;
        }

        public void Update(string tableName, int id, object newRow){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            string stringNewRow = Utils.CreateJsonObject(newRow);
            JObject jsonNewRow = JsonConvert.DeserializeObject<JObject>(stringNewRow);

            JArray tableData = Utils.ReadDataFromFile(filePath);
            if(id >= 0 && id < tableData.Count){
                tableData[id] = jsonNewRow;
                Utils.WriteDataToFile(filePath, tableData);
            }
            else{
                throw new ArgumentOutOfRangeException("Wrong idex, use integers only");
            }
        }

        public void Delete(string tableName, int id){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            JArray tableData = Utils.ReadDataFromFile(filePath);
            if(id >= 0 && id < tableData.Count){
                tableData.RemoveAt(id);
                Utils.WriteDataToFile(filePath, tableData);                
            }
        }       

       
    }
}