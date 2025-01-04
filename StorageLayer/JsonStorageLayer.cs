using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.XPath;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StorageLayer.Indexes;

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

            UpdateIndexes(tableName, jsonRow, true);
        }

        public JObject Read(string tableName, int id){
            string indexFilePath = Path.Combine(_storagePath, $"{tableName}_Id_index.json");
            if (File.Exists(indexFilePath)){
                BTree btree = GetIndex(tableName, "Id");
                BTreeNode? node = btree.Search(id);
                if (node != null){
                    string filePath = Path.Combine(_storagePath, $"{tableName}.json");
                    JArray tableData = Utils.ReadDataFromFile(filePath);
                    foreach (var row in tableData){
                        if (row["Id"] != null && (int)row["Id"] == id){
                            return (JObject)row;
                        }
                    }
                }
            }
            return new JObject();
        }

        public JArray ReadAll(string tableName){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            return Utils.ReadDataFromFile(filePath);
        }

        public IEnumerable<JObject> Query(string tableName, Func<JObject, bool> predicate){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            JArray tableData = Utils.ReadDataFromFile(filePath);

            var predicateBody = predicate.Method.GetMethodBody();
            if (predicateBody != null){
                foreach (var localVar in predicateBody.LocalVariables){
                    string columnName = localVar.LocalType.Name;
                    string indexFilePath = Path.Combine(_storagePath, $"{tableName}_{columnName}_index.json");
                    if (File.Exists(indexFilePath)){
                        BTree btree = GetIndex(tableName, columnName);
                        List<int> keys = btree.GetAllLeafKeys();
                        var results = new List<JObject>();
                        foreach (var key in keys){
                            foreach (var row in tableData){
                                if ((int)row[columnName] == key && predicate((JObject)row)){
                                    results.Add((JObject)row);
                                }
                            }
                        }
                        return results;
                    }
                }
            }

            return tableData.OfType<JObject>().Where(predicate);
        }

        public void Update(string tableName, int id, object newRow){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            string stringNewRow = Utils.CreateJsonObject(newRow);
            JObject jsonNewRow = JsonConvert.DeserializeObject<JObject>(stringNewRow);

            JArray tableData = Utils.ReadDataFromFile(filePath);
            if(id >= 0 && id < tableData.Count){
                JObject oldRow = (JObject)tableData[id];
                tableData[id] = jsonNewRow;
                Utils.WriteDataToFile(filePath, tableData);

                UpdateIndexes(tableName, oldRow, false);
                UpdateIndexes(tableName, jsonNewRow, true);
            }
            else{
                throw new ArgumentOutOfRangeException("Wrong idex, use integers only");
            }


        }

        public void Delete(string tableName, int id){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");

            JArray tableData = Utils.ReadDataFromFile(filePath);
            if(id >= 0 && id < tableData.Count){
                JObject row = (JObject)tableData[id];
                tableData.RemoveAt(id);
                Utils.WriteDataToFile(filePath, tableData);

                UpdateIndexes(tableName, row, false);          
            }
            else {
                throw new ArgumentOutOfRangeException("Wrong index, use integers only");
            }
        }

        public void CreateIndex(string tableName, string columnName){
            string filePath = Path.Combine(_storagePath, $"{tableName}.json");
            JArray tableData = Utils.ReadDataFromFile(filePath);

            BTree btree = new BTree(10);
            for (int i = 0; i < tableData.Count; i++){
                JObject row = (JObject)tableData[i];

                if(row.TryGetValue(columnName, out var value)){
                    if(!value.Type.Equals(JTokenType.Integer)){
                        throw new InvalidOperationException($"Value {value} is not of type int. Indexing only supports int values.");
                    }
                    int key = (int)value;
                    btree.Insert(key);
                }
            }

            string indexFilePath = Path.Combine(_storagePath, $"{tableName}_{columnName}_index.json");
            string json = btree.Serialize();
            File.WriteAllText(indexFilePath, json);
        }

        public BTree GetIndex(string tableName, string columnName){
            string indexFilePath = Path.Combine(_storagePath, $"{tableName}_{columnName}_index.json");
            if (File.Exists(indexFilePath)){
                string json = File.ReadAllText(indexFilePath);
                return BTree.Deserialize(json);
            }
            else{
                throw new InvalidOperationException($"Index for {columnName} in {tableName} does not exist.");
            }
        }

        public void DropIndex(string tableName){
            string indexFilePath = Path.Combine(_storagePath, $"{tableName}_Id_index.json");
            if (File.Exists(indexFilePath)){
                File.Delete(indexFilePath);
            }
            else{
                throw new InvalidOperationException($"Index for {tableName} does not exist.");
            }
        }

        public List<string> ListIndexes(){
            var indexFiles = Directory.GetFiles(_storagePath, "*_index.json");
            return indexFiles.Select(Path.GetFileNameWithoutExtension).ToList();
        }

        private void UpdateIndexes(string tableName, JObject row, bool isInsert){
            foreach (var property in row.Properties()){
                string columnName = property.Name;
                string indexFilePath = Path.Combine(_storagePath, $"{tableName}_{columnName}_index.json");
                if (File.Exists(indexFilePath)){
                    BTree btree = GetIndex(tableName, columnName);
                    if (row.TryGetValue(columnName, out var value) && value.Type == JTokenType.Integer){
                        if (isInsert){
                            btree.Insert((int)value);
                        } else {
                            btree.Delete((int)value);
                        }
                        string json = btree.Serialize();
                        File.WriteAllText(indexFilePath, json);
                    }
                }
            }
        }     
    }
}