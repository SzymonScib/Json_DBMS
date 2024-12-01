using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            string stringRow = CreateJsonObject(row);
            JObject jsonRow = JsonConvert.DeserializeObject<JObject>(stringRow);

            JArray tableData = ReadDataFromFile(filePath);
            tableData.Add(jsonRow);  
            WriteDataToFile(filePath, tableData);
        }

        public JObject Read(string tableName, int id){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            JArray tableData = ReadDataFromFile(filePath);

            if(id >= 0 && id < tableData.Count){
                 JObject item =  (JObject)tableData[id];
                 return item;
            }
            else
                return new JObject();
        }

        public void Update(string tableName, int id, object newRow){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            string stringNewRow = CreateJsonObject(newRow);
            JObject jsonNewRow = JsonConvert.DeserializeObject<JObject>(stringNewRow);

            JArray tableData = ReadDataFromFile(filePath);
            if(id >= 0 && id < tableData.Count){
                tableData[id] = jsonNewRow;
                WriteDataToFile(filePath, tableData);
            }
            else{
                throw new ArgumentOutOfRangeException("Wrong idex, use integers only");
            }
        }

        public void Delete(string tableName, int id){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            JArray tableData = ReadDataFromFile(filePath);
            if(id >= 0 && id < tableData.Count){
                tableData.RemoveAt(id);
                WriteDataToFile(filePath, tableData);
            }
        }

        public string CreateJsonObject(object row){
             if (row == null){
                throw new ArgumentNullException(nameof(row), "Row object cannot be null.");
            }
            return JsonConvert.SerializeObject(row);  
        }

        private JArray ReadDataFromFile(string filePath){
            if (File.Exists(filePath)){
                var jsonData = File.ReadAllText(filePath);

                if (string.IsNullOrEmpty(jsonData)){
                    return new JArray();  
                }
                try{           
                    var deserializedData = JsonConvert.DeserializeObject<JToken>(jsonData);

                    if (deserializedData != null){
                        if (deserializedData is JArray dataArray){
                            return dataArray;
                        }
                        else{
                            return  new JArray(deserializedData);
                        }
                    }
                    else{
                        return new JArray();
                    }
                }
                catch (JsonException ex){
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