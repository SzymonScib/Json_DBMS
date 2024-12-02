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
    }
}