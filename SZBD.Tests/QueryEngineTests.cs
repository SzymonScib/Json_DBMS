using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;
using Irony.Parsing;
using QueryEngine;
using StorageLayer;
using Newtonsoft.Json;

namespace SZBD.Tests
{
    public class QueryEngineTests
    {
        private readonly string _testStoragePath = "test_storage";

        public QueryEngineTests()
        {
            if (Directory.Exists(_testStoragePath))
            {
                Directory.Delete(_testStoragePath, true);
            }
            Directory.CreateDirectory(_testStoragePath);
        }

        [Fact]
        public void TestSelectQuery(){
            MakeTestTable("testSelectTable");
            var queryEngine = new SqlQueryEngine(_testStoragePath);

            var query = "SELECT * FROM testSelectTable";

            var result = queryEngine.ExecuteQuery(query);

            var expectedOutput = new JArray
            {
                new JObject { { "Id", 1 }, { "First_Name", "Kisuke" }, { "Last_Name", "Urahara" }, { "Username", "Ilikepizza" } },
                new JObject { { "Id", 2 }, { "First_Name", "Yoruichi" }, { "Last_Name", "Shihoin" }, { "Username", "BlackCat" } },
                new JObject { { "Id", 3 }, { "First_Name", "Jushiro" }, { "Last_Name", "Ukitake" }, { "Username", "MimihagiSama" } },
                new JObject { { "Id", 4 }, { "First_Name", "Ichigo" }, { "Last_Name", "Kurosaki" }, { "Username", "Bankai" } },
                new JObject { { "Id", 5 }, { "First_Name", "Shunsui" }, { "Last_Name", "Kyoraku" }, { "Username", "HeadCaptain123" } }
            };

            string expectedResult = JsonConvert.SerializeObject(expectedOutput, Formatting.None); 

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestSelectQueryWhere(){
            MakeTestTable("testSelectTable");
            var queryEngine = new SqlQueryEngine(_testStoragePath);

            var query = "SELECT Id, First_Name, Last_Name, Username FROM testSelectTable WHERE Id = 1";
            var result = queryEngine.ExecuteQuery(query);

            var expectedOutput = new JArray
            {
                new JObject { { "Id", 1 }, { "First_Name", "Kisuke" }, { "Last_Name", "Urahara" }, { "Username", "Ilikepizza" } }
            };
            
            string expectedResult = JsonConvert.SerializeObject(expectedOutput, Formatting.None); 

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestSelectQueryNotAllColumns(){
            MakeTestTable("testSelectTable");
            var queryEngine = new SqlQueryEngine(_testStoragePath);

            var query = "SELECT Id, First_Name FROM testSelectTable";

            var result = queryEngine.ExecuteQuery(query);

            var expectedOutput = new JArray
            {
                new JObject { { "Id", 1 }, { "First_Name", "Kisuke" } },
                new JObject { { "Id", 2 }, { "First_Name", "Yoruichi" } },
                new JObject { { "Id", 3 }, { "First_Name", "Jushiro" } },
                new JObject { { "Id", 4 }, { "First_Name", "Ichigo" } },
                new JObject { { "Id", 5 }, { "First_Name", "Shunsui" } }
            };

            string expectedResult = JsonConvert.SerializeObject(expectedOutput, Formatting.None); 

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestSelectQueryWhereNotAllColumns(){
            MakeTestTable("testSelectTable");
            var queryEngine = new SqlQueryEngine(_testStoragePath);

            var query = "SELECT Id, First_Name, Last_Name FROM testSelectTable WHERE Id = 1";
            var result = queryEngine.ExecuteQuery(query);

            var expectedOutput = new JArray
            {
                new JObject { { "Id", 1 }, { "First_Name", "Kisuke" }, { "Last_Name", "Urahara" } }
            };
            
            string expectedResult = JsonConvert.SerializeObject(expectedOutput, Formatting.None); 

            Assert.Equal(expectedResult, result);
        }


        void MakeTestTable(string tableName){
            var storageLayer = new JsonStorageLayer(_testStoragePath);
            var columns = new List<Column>{
                new Column { Name = "Id", Type = "int", PrimaryKey = true, Unique = true},
                new Column { Name = "First_Name", Type = "string", PrimaryKey = false, Unique = false},
                new Column { Name = "Last_Name", Type = "string", PrimaryKey = false, Unique = false},
                new Column { Name = "Username", Type = "string", PrimaryKey = false, Unique = true}
            };
            string filePath = Path.Combine(_testStoragePath, $"{tableName}.json");
                if(File.Exists(filePath)){
                File.Delete(filePath);
            }
            storageLayer.CreateTable(tableName, columns);

            var row = new {Id = 1, First_Name = "Kisuke", Last_Name = "Urahara", Username = "Ilikepizza"};
            var row1 = new {Id = 2, First_Name = "Yoruichi", Last_Name = "Shihoin", Username = "BlackCat"};
            var row2 = new {Id = 3, First_Name = "Jushiro", Last_Name = "Ukitake", Username = "MimihagiSama"};
            var row3 = new {Id = 4, First_Name = "Ichigo", Last_Name = "Kurosaki", Username = "Bankai"};
            var row4 = new {Id = 5, First_Name = "Shunsui", Last_Name = "Kyoraku", Username = "HeadCaptain123"};
            
            storageLayer.Insert(tableName, row);
            storageLayer.Insert(tableName, row1);
            storageLayer.Insert(tableName, row2);
            storageLayer.Insert(tableName, row3);
            storageLayer.Insert(tableName, row4);
            storageLayer.CreateIndex(tableName, "Id");
        }
    }
}
