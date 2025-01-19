using Irony.Parsing;
using StorageLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QueryEngine
{

    public class SqlQueryEngine
    {
        private readonly JsonStorageLayer _storageLayer;
        private readonly Parser _parser;

        public SqlQueryEngine(string storagePath){
            _storageLayer = new JsonStorageLayer(storagePath);
            var grammar = new SimpleSqlGrammar();
            _parser = new Parser(grammar);
        }

        public string ExecuteQuery(string query)
        {
            var parseTree = _parser.Parse(query);

            if (parseTree.HasErrors())
            {
                return "Error parsing query:";
            }

            var root = parseTree.Root;
            if (root == null)
            {
                return "No parse tree root found";
            }


            var command = root.Term.Name;
            switch (command)
            {
                case "selectStmt":
                    return JsonConvert.SerializeObject(ExecuteSelect(root), Formatting.None);
                default:
                    return "Unknown command: " + command;
            }
        }


        private JArray ExecuteSelect(ParseTreeNode root){
            var selList = root.ChildNodes[0];
            var fromClause = root.ChildNodes[1];
            var whereClauseOpt = root.ChildNodes.Count > 2 ? root.ChildNodes[2] : null;

            var tableName = fromClause.ChildNodes[0].FindTokenAndGetText();
            var columns = selList.ChildNodes[0].Term.Name == "*" ? null : selList.ChildNodes[0].ChildNodes.Select(node => node.FindTokenAndGetText()).ToList();
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

            return dataOnly;
        }

        private bool EvaluateExpression(JToken row, ParseTreeNode expression){
            switch (expression.Term.Name){
                case "binExpr":
                    var left = expression.ChildNodes[0].FindTokenAndGetText();
                    var op = expression.ChildNodes[1].FindTokenAndGetText();
                    var right = expression.ChildNodes[2].FindTokenAndGetText();

                    var leftValue = row[left];
                    var rightValue = JToken.Parse(right);

                    switch (op){
                        case "=":
                            return JToken.DeepEquals(leftValue, rightValue);
                        case "<":
                            return leftValue.Value<int>() < rightValue.Value<int>();
                        case ">":
                            return leftValue.Value<int>() > rightValue.Value<int>();
                        case "<=":
                            return leftValue.Value<int>() <= rightValue.Value<int>();
                        case ">=":
                            return leftValue.Value<int>() >= rightValue.Value<int>();
                        case "<>":
                            return !JToken.DeepEquals(leftValue, rightValue);
                        default:
                            throw new Exception("Unknown operator: " + op);
                    }

                case "term":
                    // Handle term expressions (e.g., identifiers, numbers, string literals)
                    var termValue = expression.FindTokenAndGetText();
                    return row.ToString() == termValue;

                case "parExpr":
                    // Handle parenthesized expressions
                    return EvaluateExpression(row, expression.ChildNodes[1]);

                case "unExpr":
                    // Handle unary expressions (e.g., NOT)
                    var unaryOp = expression.ChildNodes[0].FindTokenAndGetText();
                    var unaryExpr = expression.ChildNodes[1];
                    var unaryResult = EvaluateExpression(row, unaryExpr);
                    return unaryOp == "NOT" ? !unaryResult : throw new Exception("Unknown unary operator: " + unaryOp);

                default:
                    throw new Exception("Unsupported expression type: " + expression.Term.Name);
            }
        }
    }
}
