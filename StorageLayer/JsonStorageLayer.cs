using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                Columns = columns,
                Data = new List<object>()
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
            string jsonRow = CreateJsonObject(row);

            var tableData = ReadDataFromFile(filePath);
            tableData["Data"].Add(jsonRow);  
            WriteDataToFile(filePath, tableData);
        }

        public object Read(string tableName, int id){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            var tableData = ReadDataFromFile(filePath);

            if(id >= 0 && id < tableData["Data"].Count){
                return tableData["Data"][id];
            }
            else
                return new { Data = new List<object>() };
        }

        public void Update(string tableName, int id, object newRow){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            string jsonNewRow = CreateJsonObject(newRow);

            var tableData = ReadDataFromFile(filePath);
            if(id >= 0 && tableData["Data"].Count){
                tableData["Data"][id] = jsonNewRow;
                WriteDataToFile(filePath, tableData);
            }
            else{
                throw new ArgumentOutOfRangeException("Wrong idex, use integers only");
            }
        }

        public void Delete(string tableName, int id){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            var tableData = ReadDataFromFile(filePath);
            if(id >= 0 && id < tableData["Data"].Count){
                tableData["Data"].RemoveAt(id);
                WriteDataToFile(filePath, tableData);
            }
        }

        public string CreateJsonObject(object row){
            return JsonConvert.SerializeObject(row, Formatting.Indented);  
        }

        private dynamic ReadDataFromFile(string filePath){
            if(File.Exists(filePath)){
                var jsonData = File.ReadAllText(filePath);

                if(string.IsNullOrEmpty(jsonData)){
                    return new { Data = new List<object>() };
                }

                try{
                    var deserializedData = JsonConvert.DeserializeObject<dynamic>(jsonData);

                    if(deserializedData != null){
                        return deserializedData;
                    }
                    else{
                        return new { Data = new List<object>() };
                    }                
                }
                catch(JsonException ex){
                    throw new InvalidOperationException("Error deserializing the JSON file.", ex);
                }
            }
            else{
                throw new FileNotFoundException($"{filePath} not found");
            }
       }

        private void WriteDataToFile(string filePath, dynamic data){
            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }
    }
}