using StorageLayer;
using Newtonsoft.Json;
using Xunit;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using StorageLayer.Indexes;

namespace SZBD.Tests;

public class JsonStorageLayerTests
{
    private readonly string _testStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "TestStorage");
    [Fact]
    public void CreateTableTest(){
        if(Directory.Exists(_testStoragePath)){
            Directory.Delete(_testStoragePath, true);
        }

        var storageLayer = new JsonStorageLayer(_testStoragePath);
        var columns = new List<Column>{
            new Column { Name = "Id", Type = "int", PrimaryKey = true, Unique = true},
            new Column { Name = "First_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Last_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Username", Type = "string", PrimaryKey = false, Unique = true}
        };
        string tableName = "TestTable";
        string filePath = Path.Combine(_testStoragePath, $"{tableName}.json");
        if(File.Exists(filePath)){
            File.Delete(filePath);
        }
        storageLayer.CreateTable(tableName, columns);

        Assert.True(File.Exists(filePath));

        string content = File.ReadAllText(filePath);
        if (!string.IsNullOrWhiteSpace(content)){
            dynamic tableDefinition = JsonConvert.DeserializeObject(content) 
            ?? throw new InvalidOperationException("Deserialization failed: JSON is invalid or null.");

            Assert.Equal(tableName, (string)tableDefinition.TableName);             
        }
    }

    [Fact]
    public void ReadTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        string tableName = "TestDeleteTable";
        MakeTestTable(tableName);

        JObject result = storageLayer.Read(tableName, 1);

        Assert.NotNull(result);
        Assert.Equal("Kisuke", result["First_Name"]);
    }

    [Fact]
    public void UpdateTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        string tableName = "TestUpdateTable";
        MakeTestTable(tableName);

        var new_row = new {Id = 1, First_Name = "Jushiro", Last_Name = "Ukitake", Username = "RightHand123"};
        storageLayer.Update(tableName, 1, new_row);

        JObject result = storageLayer.Read(tableName, 1);

        Assert.NotNull(result);
        Assert.Equal("RightHand123", result["Username"]);

        BTree btree = storageLayer.GetIndex(tableName, "Id");
        Assert.NotNull(btree);

        List<int> leafKeys = btree.GetAllLeafKeys();
        List<int> expectedLeafKeys = new List<int> { 1, 2, 3, 4, 5 };

        for(int i = 0; i < expectedLeafKeys.Count; i++){
            Assert.Equal(expectedLeafKeys[i], leafKeys[i]);
        }
    }

    [Fact]
    public void DeleteTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        string tableName = "TestDeleteTable";
        MakeTestTable(tableName);

        storageLayer.Delete(tableName, 2);//

        var result = storageLayer.Query(tableName, row => (string)row["Id"] == "2");       
        Assert.Empty(result);

        BTree btree = storageLayer.GetIndex(tableName, "Id");
        Assert.NotNull(btree);

        List<int> leafKeys = btree.GetAllLeafKeys();
        List<int> expectedLeafKeys = new List<int> { 1, 3, 4, 5 };

        for(int i = 0; i < expectedLeafKeys.Count; i++){
            Assert.Equal(expectedLeafKeys[i], leafKeys[i]);
        }
    }

    [Fact]
    public void ReadAllTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        string tableName = "TestRadAllTable";
        MakeTestTable(tableName);

        JArray result = storageLayer.ReadAll(tableName);

        Assert.NotEmpty(result);
        Assert.Equal(6, result.Count());

        BTree btree = storageLayer.GetIndex(tableName, "Id");
        Assert.NotNull(btree);

        List<int> leafKeys = btree.GetAllLeafKeys();
        List<int> expectedLeafKeys = new List<int> { 1, 2, 3, 4, 5 };

        for(int i = 0; i < expectedLeafKeys.Count; i++){
            Assert.Equal(expectedLeafKeys[i], leafKeys[i]);
        }
    }

    [Fact]
    public void QueryTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        MakeTestTable("TestQueryTable");

        var results = storageLayer.Query("TestQueryTable", row => (string)row["First_Name"] == "Ichigo");

        Assert.NotNull(results); 
        Assert.Single(results);  
        Assert.Equal("Ichigo", results.First()["First_Name"]); 
        Assert.Equal("Kurosaki", results.First()["Last_Name"]); 
        Assert.Equal("Bankai", results.First()["Username"]);
    }

    [Fact]
    public void QueryIdTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        MakeTestTable("TestQueryIdTable");

        var results = storageLayer.Query("TestQueryIdTable", row => row["Id"] != null && (int)row["Id"] == 4);

        Assert.NotNull(results); 
        Assert.Single(results);  
        Assert.Equal("Ichigo", results.First()["First_Name"]); 
        Assert.Equal("Kurosaki", results.First()["Last_Name"]); 
        Assert.Equal("Bankai", results.First()["Username"]);
    }

    [Fact]
     public void QueryNoMatchTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        MakeTestTable("TestQueryNoMatch");

        var results = storageLayer.Query("TestQueryNoMatch", row => (string)row["First_Name"] == "Chad");
 
        Assert.Empty(results);  
    }

    [Fact]
    private void CreateIndexTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        string tableName = "TestIndexTable";
        MakeTestTable(tableName);

        string tableFilePath = Path.Combine(_testStoragePath, $"{tableName}.json");
        Assert.True(File.Exists(tableFilePath), $"Table file {tableFilePath} does not exist.");

        string indexFilePath = Path.Combine(_testStoragePath, $"{tableName}_Id_index.json");
        Assert.True(File.Exists(indexFilePath), $"Index file {indexFilePath} does not exist.");

        BTree btree = storageLayer.GetIndex(tableName, "Id");
        Assert.NotNull(btree);

        List<int> leafKeys = btree.GetAllLeafKeys();
        List<int> expectedLeafKeys = new List<int> { 1, 2, 3, 4, 5 };

        for(int i = 0; i < expectedLeafKeys.Count; i++){
            Assert.Equal(expectedLeafKeys[i], leafKeys[i]);
        }   
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