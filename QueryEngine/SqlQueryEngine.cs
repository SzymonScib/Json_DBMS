﻿using Irony.Parsing;
using StorageLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using Serilog;

namespace QueryEngine
{

    public class SqlQueryEngine
    {
        private readonly IStorageLayer _storageLayer;
        private readonly Parser _parser;
        private readonly ILogger _logger;

        public SqlQueryEngine(IStorageLayer storageLayer, ILogger logger){
            _storageLayer = storageLayer;
            var grammar = new SimpleSqlGrammar();
            _parser = new Parser(grammar);
            _logger = logger;
        }

        public string ExecuteQuery(string query){
            var parseTree = _parser.Parse(query);

            if (parseTree.HasErrors()){
                var errorMessages = parseTree.ParserMessages.Select(message => message.Message).ToList();
                var errorMessage = "Error parsing query: " + string.Join("; ", errorMessages);
                _logger.Error(errorMessage);
                return errorMessage;
            }

            var root = parseTree.Root.ChildNodes[0];
            if (root == null){
                var errorMessage = "No parse tree root found";
                _logger.Error(errorMessage);
                return errorMessage;
            }


            var command = root.Term.Name;
            try {
                switch (command)
                {
                    case "selectStmt":
                        return JsonConvert.SerializeObject(ExecuteSelect(root), Formatting.None);
                    case "createTableStmt":
                        return  ExcecuteCreateTable(root);
                    case "insertStmt":
                        return ExcecuteInsert(root);
                    case "deleteStmt":
                        return ExcecuteDelete(root);
                    case "dropTableStmt":
                        return ExecuteDropTable(root);
                    case "updateStmt":
                        return ExcecuteUpdate(root);
                    default:
                        var errorMessage = "Unknown command: " + command;
                        _logger.Error(errorMessage);
                        return errorMessage;
                }
            }catch(Exception ex){
                var errorMessage = "Error executing command: " + ex.Message;
                _logger.Error(errorMessage);
                return errorMessage;
            }
        }


        private JArray ExecuteSelect(ParseTreeNode root){
            var selList = root.ChildNodes[0];
            var fromClause = root.ChildNodes[1];
            var whereClauseOpt = root.ChildNodes.Count > 2 ? root.ChildNodes[2] : null;

            var tableName = fromClause.ChildNodes[0].FindTokenAndGetText();
            if (!_storageLayer.ValidateTableName(tableName)){
                throw new InvalidOperationException($"Table {tableName} does not exist.");
            }

            var columns = selList.ChildNodes[0].Term.Name == "*" ? null : selList.ChildNodes[0].ChildNodes.Select(node => node.FindTokenAndGetText()).ToList();
            if (columns != null){
                ValidateColumns(tableName, columns);
            }

            var whereExpression = whereClauseOpt != null && whereClauseOpt.ChildNodes.Count >= 1 ? whereClauseOpt.ChildNodes[0].ChildNodes[0] : null;

            JArray results;

            if (whereExpression != null){
                if(columns != null){
                    results = _storageLayer.QueryColumns(tableName, columns ,row => EvaluateExpression(row, whereExpression));
                }
                else{
                    results = new JArray(_storageLayer.Query(tableName, row => EvaluateExpression(row, whereExpression)));
                }
            }
            else{
                if(columns != null){
                    results = _storageLayer.ReadColumns(tableName, columns);
                }
                else{
                    results = _storageLayer.ReadAll(tableName);
                }
            }

            var dataOnly = new JArray();
            foreach (var result in results){
                if (result is JObject obj && obj.ContainsKey("TableName")){
                    continue;
                }
                dataOnly.Add(result);
            }
            var message = "Select executed";
            _logger.Information(message);

            return dataOnly;
        }

        private string ExcecuteCreateTable(ParseTreeNode root){
            var tableName = root.ChildNodes[1].FindTokenAndGetText();
            var columnDefs = root.ChildNodes[2].ChildNodes;

            var columns = new List<Column>();

            foreach (var columnDef in columnDefs){
                var columnName = columnDef.ChildNodes[0].FindTokenAndGetText();
                var dataType = columnDef.ChildNodes[1].FindTokenAndGetText();

                var primaryKey = columnDef.ChildNodes[2].ChildNodes.Any(node => node.FindTokenAndGetText() == "PRIMARY KEY");
                var unique = columnDef.ChildNodes[2].ChildNodes.Any(node => node.FindTokenAndGetText() == "UNIQUE");
                var allowNull = columnDef.ChildNodes[2].ChildNodes.Any(node => node.FindTokenAndGetText() == "NULL") && 
                        !columnDef.ChildNodes[2].ChildNodes.Any(node => node.FindTokenAndGetText() == "NOT NULL");
                columns.Add(new Column {
                     Name = columnName, 
                     Type = dataType, 
                     PrimaryKey = primaryKey,
                     Unique = unique,
                     AllowNull = allowNull});
            }

            _storageLayer.CreateTable(tableName, columns);
            var message = $"Table {tableName} created";
            _logger.Information(message);
            return message;
        }

        private string ExcecuteInsert(ParseTreeNode root){
            var tableName = root.ChildNodes[1].FindTokenAndGetText();
            if (!_storageLayer.ValidateTableName(tableName)){
                throw new InvalidOperationException($"Table {tableName} does not exist.");
            }

            var columnNames = root.ChildNodes[2].ChildNodes.Select(node => node.FindTokenAndGetText()).ToList();
            var values = root.ChildNodes[4].ChildNodes.Select(node => node.FindTokenAndGetText()).ToList();

            var row = new JObject();
            for (var i = 0; i < columnNames.Count; i++){
                row[columnNames[i]] = JToken.Parse(values[i]);
            }

            _storageLayer.Insert(tableName, row);
            var message = $"Row inserted into {tableName}";
            _logger.Information(message);
            return message;
        }

        private string ExcecuteDelete(ParseTreeNode root){
            var tableName = root.ChildNodes[1].FindTokenAndGetText();
            if (!_storageLayer.ValidateTableName(tableName)){
                throw new InvalidOperationException($"Table {tableName} does not exist.");
            }

            var whereClauseOpt = root.ChildNodes.Count > 2 ? root.ChildNodes[2] : null;
            var whereExpression = whereClauseOpt != null && whereClauseOpt.ChildNodes.Count >= 1 ? whereClauseOpt.ChildNodes[0].ChildNodes[0] : null;

            if (whereExpression != null){
                var rowsToDelete = _storageLayer.Query(tableName, row => EvaluateExpression(row, whereExpression)).ToList();
                foreach (var row in rowsToDelete){
                    var id = (int)row["Id"];
                    _storageLayer.Delete(tableName, id);
                }
                var message = $"Rows deleted from {tableName}";
                _logger.Information(message);
                return message;
            }
            else{
                var message = "No where clause found";
                _logger.Information(message);
                return message;
            }
        }

        private string ExecuteDropTable(ParseTreeNode root){
            var tableName = root.ChildNodes[1].FindTokenAndGetText();
            if (!_storageLayer.ValidateTableName(tableName)){
                throw new InvalidOperationException($"Table {tableName} does not exist.");
            }

            _storageLayer.DropTable(tableName);
            var message = $"Table {tableName} dropped";
            _logger.Information(message);
            return message;
        }

        private string ExcecuteUpdate(ParseTreeNode root){
            var tableName = root.ChildNodes[1].FindTokenAndGetText();
            if (!_storageLayer.ValidateTableName(tableName)){
                throw new InvalidOperationException($"Table {tableName} does not exist.");
            }

            var columnItemList = root.ChildNodes[2].ChildNodes;
            var columnNames = columnItemList.Select(item => item.ChildNodes[0].FindTokenAndGetText()).ToList();
            ValidateColumns(tableName, columnNames);
            
            var whereClauseOpt = root.ChildNodes.Count > 3 ? root.ChildNodes[3] : null;
            var whereExpression = whereClauseOpt != null && whereClauseOpt.ChildNodes.Count >= 1 ? whereClauseOpt.ChildNodes[0].ChildNodes[0] : null;

            if (whereExpression != null){
                var rowsToUpdate = _storageLayer.Query(tableName, row => EvaluateExpression(row, whereExpression)).ToList();
                if (rowsToUpdate.Count == 0){
                    var message1 = "No rows found to update";
                    _logger.Information(message1);
                    return message1;
                }
                
                 foreach (var row in rowsToUpdate){
                    var id = (int)row["Id"];
                    var updatedRow = new JObject(row);

                    foreach (var columnItem in columnItemList){
                        var columnName = columnItem.ChildNodes[0].FindTokenAndGetText();
                        var value = columnItem.ChildNodes[2].FindTokenAndGetText();
                        updatedRow[columnName] = JToken.Parse(value);
                    }

                    _storageLayer.Update(tableName, id, updatedRow);
                }
                var message = $"Rows updated in {tableName}";
                _logger.Information(message);
                return message;
            }else{
                var message = "No where clause found";
                _logger.Information(message);
                return message;
            }
        }



        private bool EvaluateExpression(JToken row, ParseTreeNode expression){
            switch (expression.Term.Name){
                case "binExpr":
                    var left = expression.ChildNodes[0].FindTokenAndGetText();
                    var op = expression.ChildNodes[1].FindTokenAndGetText();
                    var right = expression.ChildNodes[2].FindTokenAndGetText();

                    var leftValue = row[left] != null ? row[left].Value<int>() : 0;
                    var rightValue = JToken.Parse(right);                   

                    switch (op){
                        case "=":
                            return JToken.DeepEquals(leftValue, rightValue);
                        case "<":
                            return leftValue < rightValue.Value<int>();
                        case ">":
                            return leftValue > rightValue.Value<int>();
                        case "<=":
                            return leftValue <= rightValue.Value<int>();
                        case ">=":
                            return leftValue >= rightValue.Value<int>();
                        case "<>":
                            return !JToken.DeepEquals(leftValue, rightValue);
                        default:
                            throw new Exception("Unknown operator: " + op);
                    }

                case "term":
                    var termValue = expression.FindTokenAndGetText();
                    return row.ToString() == termValue;

                case "parExpr":
                    return EvaluateExpression(row, expression.ChildNodes[1]);

                case "unExpr":
                    var unaryOp = expression.ChildNodes[0].FindTokenAndGetText();
                    var unaryExpr = expression.ChildNodes[1];
                    var unaryResult = EvaluateExpression(row, unaryExpr);
                    return unaryOp == "NOT" ? !unaryResult : throw new Exception("Unknown unary operator: " + unaryOp);

                default:
                    throw new Exception("Unsupported expression type: " + expression.Term.Name);
            }
        }

        private void ValidateColumns(string tableName, List<string> columnNames){
            var tableDefinition = _storageLayer.GetTableDefinition(tableName);
            var validColumns = tableDefinition.Select(c => c.Name).ToList();

            foreach (var columnName in columnNames){
                if (!validColumns.Contains(columnName)){
                    var errorMessage = $"Column {columnName} does not exist in table {tableName}.";
                    _logger.Error(errorMessage);
                    throw new InvalidOperationException($"Column {columnName} does not exist in table {tableName}.");
                }
            }
        }
    }
}