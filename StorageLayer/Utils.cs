using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StorageLayer
{
    public class Utils
    {
        
        static public string CreateJsonObject(object row){
             if (row == null){
                throw new ArgumentNullException(nameof(row), "Row object cannot be null.");
            }
            return JsonConvert.SerializeObject(row);  
        }

        static public JArray ReadDataFromFile(string filePath){
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

        static public void WriteDataToFile(string filePath, dynamic data){
            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }

        static public void ValidateRow(string tableName, JObject row, string filePath){

            if (!File.Exists(filePath)){
                throw new FileNotFoundException($"Table {tableName} does not exist.");
            }

            JArray tableData = Utils.ReadDataFromFile(filePath);
            if (tableData.Count == 0){
                throw new InvalidOperationException($"Table {tableName} has no schema defined.");
            }

            JObject tableDefinition = tableData[0] as JObject;
            if (tableDefinition == null || !tableDefinition.ContainsKey("Columns")){
                throw new InvalidOperationException($"Table {tableName} does not have a valid schema.");
            }
            var columns = tableDefinition["Columns"].ToObject<List<Column>>();

            if (columns == null){
                throw new InvalidOperationException($"Table {tableName} does not have a valid schema.");
            }
            var columnNames = columns.Select(c => c.Name).ToList();

            var rowKeys = row.Properties().Select(p => p.Name).ToList();

            var missingColumns = columnNames.Except(rowKeys).ToList();
            if (missingColumns.Any()){
                throw new InvalidOperationException($"Row is missing required columns: {string.Join(", ", missingColumns)}");
            }

            foreach (var column in columns){
                if (row.TryGetValue(column.Name, out var value)){
                    ValidateColumnType(value, column.Type);
                }
            }
        }
           
        static private void ValidateColumnType(JToken value, string expectedType){
            switch(expectedType.ToLower()){
                case "int":
                    if(!value.Type.Equals(JTokenType.Integer)){
                        throw new InvalidOperationException($"Value {value} is not of type int.");
                    }
                    break;
                case "string":
                    if(!value.Type.Equals(JTokenType.String)){
                        throw new InvalidOperationException($"Value {value} is not of type string.");
                    }
                    break;
                case "bool":
                    if(!value.Type.Equals(JTokenType.Boolean)){
                        throw new InvalidOperationException($"Value {value} is not of type bool.");
                    }
                    break;
                case "float":
                    if(!value.Type.Equals(JTokenType.Float)){
                        throw new InvalidOperationException($"Value {value} is not of type float.");
                    }
                    break;
                case "double":
                    if(!value.Type.Equals(JTokenType.Float)){
                        throw new InvalidOperationException($"Value {value} is not of type double.");
                    }
                    break;
                case "DateTime":
                    if(!value.Type.Equals(JTokenType.Date)){
                        throw new InvalidOperationException($"Value {value} is not of type DateTime.");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown column type {expectedType}.");                       
            }
        }
    }
}