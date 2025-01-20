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
        private readonly string _testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage");

        public QueryEngineTests(){
            if (Directory.Exists(_testStoragePath)){
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

        [Fact]
        public void TestCreateTable(){
            var queryEngine = new SqlQueryEngine(_testStoragePath);

            var query = "CREATETABLE testCreateTable (Id INT PRIMARY KEY UNIQUE, First_Name STRING, Last_Name STRING, Username STRING UNIQUE)";

            string filePath = Path.Combine(_testStoragePath, $"testCreateTable.json");
                if(File.Exists(filePath)){
                File.Delete(filePath);
            }

            var result = queryEngine.ExecuteQuery(query);

            Assert.Equal("Table testCreateTable created", result);
            Assert.True(File.Exists(Path.Combine(_testStoragePath, "testCreateTable.json")));

            var tableDefinition = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filePath));
            var columns = tableDefinition["Columns"].ToObject<List<Column>>();

            Assert.Equal(4, columns.Count);

            var idColumn = columns.FirstOrDefault(c => c.Name == "Id");
            Assert.NotNull(idColumn);
            Assert.Equal("INT", idColumn.Type);
            Assert.True(idColumn.PrimaryKey);
            Assert.True(idColumn.Unique);
            Assert.False(idColumn.AllowNull);

            var firstNameColumn = columns.FirstOrDefault(c => c.Name == "First_Name");
            Assert.NotNull(firstNameColumn);
            Assert.Equal("STRING", firstNameColumn.Type);
            Assert.False(firstNameColumn.PrimaryKey);
            Assert.False(firstNameColumn.Unique);
            Assert.False(firstNameColumn.AllowNull);

            var lastNameColumn = columns.FirstOrDefault(c => c.Name == "Last_Name");
            Assert.NotNull(lastNameColumn);
            Assert.Equal("STRING", lastNameColumn.Type);
            Assert.False(lastNameColumn.PrimaryKey);
            Assert.False(lastNameColumn.Unique);
            Assert.False(lastNameColumn.AllowNull);

            var usernameColumn = columns.FirstOrDefault(c => c.Name == "Username");
            Assert.NotNull(usernameColumn);
            Assert.Equal("STRING", usernameColumn.Type);
            Assert.False(usernameColumn.PrimaryKey);
            Assert.True(usernameColumn.Unique);
            Assert.False(usernameColumn.AllowNull);
        }

        [Fact]
        public void TestInsertQuery(){
            var queryEngine = new SqlQueryEngine(_testStoragePath);

            var createTableQuery = "CREATETABLE testInsertTable (Id INT PRIMARY KEY, First_Name STRING, Last_Name STRING, Username STRING UNIQUE)";
            queryEngine.ExecuteQuery(createTableQuery);

            var insertQuery = "INSERTINTO testInsertTable (Id, First_Name, Last_Name, Username) VALUES (1, 'Kisuke', 'Urahara', 'Ilikepizza')";
            var result = queryEngine.ExecuteQuery(insertQuery);

            Assert.Equal("Row inserted into testInsertTable", result);

            var selectQuery = "SELECT * FROM testInsertTable";
            var selectResult = queryEngine.ExecuteQuery(selectQuery);

            var expectedOutput = new JArray
            {
                new JObject { { "Id", 1 }, { "First_Name", "Kisuke" }, { "Last_Name", "Urahara" }, { "Username", "Ilikepizza" } }
            };

            string expectedResult = JsonConvert.SerializeObject(expectedOutput, Formatting.None); 

            Assert.Equal(expectedResult, selectResult);
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
