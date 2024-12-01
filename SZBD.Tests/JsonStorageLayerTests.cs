using StorageLayer;
using Newtonsoft.Json;
using Xunit;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

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
    public void InsertTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        var columns = new List<Column>{
            new Column { Name = "Id", Type = "int", PrimaryKey = true, Unique = true},
            new Column { Name = "First_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Last_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Username", Type = "string", PrimaryKey = false, Unique = true}
        };
        string tableName = "TestInsertTable";
        string filePath = Path.Combine(_testStoragePath, $"{tableName}.json");
            if(File.Exists(filePath)){
            File.Delete(filePath);
        }
        storageLayer.CreateTable(tableName, columns);

        var row = new {Id = 1, First_Name = "Shunsui", Last_Name = "Kyoraku", Username = "HeadCaptain123"};

        storageLayer.Insert(tableName, row);
        //string content = File.ReadAllText(filePath);
        JObject result = storageLayer.Read(tableName, 1);
        Assert.Equal(1, result["Id"]);
        Assert.Equal("Shunsui", result["First_Name"]);
        Assert.Equal("Kyoraku", result["Last_Name"]);
        Assert.Equal("HeadCaptain123", result["Username"]);
    }

    [Fact]
    public void ReadTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        var columns = new List<Column>{
            new Column { Name = "Id", Type = "int", PrimaryKey = true, Unique = true},
            new Column { Name = "First_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Last_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Username", Type = "string", PrimaryKey = false, Unique = true}
        };
        string tableName = "TestReadTable";
        string filePath = Path.Combine(_testStoragePath, $"{tableName}.json");
            if(File.Exists(filePath)){
            File.Delete(filePath);
        }
        storageLayer.CreateTable(tableName, columns);

        var row = new {Id = 1, First_Name = "Ichigo", Last_Name = "Kurosaki", Username = "Bankai"};

        storageLayer.Insert(tableName, row);

        JObject result = storageLayer.Read(tableName, 1);

        Assert.NotNull(result);
        Assert.Equal("Ichigo", result["First_Name"]);
    }

    [Fact]
    public void UpdateTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        var columns = new List<Column>{
            new Column { Name = "Id", Type = "int", PrimaryKey = true, Unique = true},
            new Column { Name = "First_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Last_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Username", Type = "string", PrimaryKey = false, Unique = true}
        };
        string tableName = "TestUpdateTable";
        string filePath = Path.Combine(_testStoragePath, $"{tableName}.json");
            if(File.Exists(filePath)){
            File.Delete(filePath);
        }
        storageLayer.CreateTable(tableName, columns);

        var row = new {Id = 1, First_Name = "Jushiro", Last_Name = "Ukitake", Username = "MimihagiSama"};
        storageLayer.Insert(tableName, row);

        var new_row = new {Id = 1, First_Name = "Jushiro", Last_Name = "Ukitake", Username = "RightHand123"};
        storageLayer.Update(tableName, 1, new_row);

        JObject result = storageLayer.Read(tableName, 1);

        Assert.NotNull(result);
        Assert.Equal("RightHand123", result["Username"]);
    }

    [Fact]
    public void DeleteTest(){
        var storageLayer = new JsonStorageLayer(_testStoragePath);
        var columns = new List<Column>{
            new Column { Name = "Id", Type = "int", PrimaryKey = true, Unique = true},
            new Column { Name = "First_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Last_Name", Type = "string", PrimaryKey = false, Unique = false},
            new Column { Name = "Username", Type = "string", PrimaryKey = false, Unique = true}
        };
        string tableName = "TestDeleteTable";
        string filePath = Path.Combine(_testStoragePath, $"{tableName}.json");
            if(File.Exists(filePath)){
            File.Delete(filePath);
        }
        storageLayer.CreateTable(tableName, columns);

        var row = new {Id = 1, First_Name = "Kisuke", Last_Name = "Urahara", Username = "Ilikepizza"};
        var row1 = new {Id = 2, First_Name = "Yoruichi", Last_Name = "Shihoin", Username = "BlackCat"};
        storageLayer.Insert(tableName, row);
        storageLayer.Insert(tableName, row1);

        storageLayer.Delete(tableName, 2);

        var result = storageLayer.Read(tableName, 2);        
        Assert.Empty(result);
    }
}