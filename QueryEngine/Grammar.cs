using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Irony.Parsing;

namespace QueryEngine
{
    [Language("SimpleSql", "1.0", "A simple SQL-like language")]
    public class SimpleSqlGrammar : Grammar
    {
        public SimpleSqlGrammar()
        {
            var identifier = new IdentifierTerminal("identifier", "[a-zA-Z_][a-zA-Z0-9_]*");
            var number = new NumberLiteral("number");
            var floatNumber = new NumberLiteral("float", NumberOptions.AllowSign | NumberOptions.AllowStartEndDot);
            var stringLiteral = new StringLiteral("string", "'");
            var boolLiteral = new ConstantTerminal("bool");
            var dateTimeLiteral = new RegexBasedTerminal("datetime", @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
            var comma = ToTerm(",");
            var Id = identifier; 

            var selectStmt = new NonTerminal("selectStmt");
            var insertStmt = new NonTerminal("insertStmt");
            var createTableStmt = new NonTerminal("createTableStmt");
            var updateStmt = new NonTerminal("updateStmt");
            var deleteStmt = new NonTerminal("deleteStmt");
            var dropTableStmt = new NonTerminal("dropTableStmt");

            var selRestrOpt = new NonTerminal("selRestrOpt");
            var selList = new NonTerminal("selList");
            var columnItemList = new NonTerminal("columnItemList");
            var columnItem = new NonTerminal("columnItem");
            var aliasOpt = new NonTerminal("aliasOpt");
            var asOpt = new NonTerminal("asOpt");
            var columnSource = new NonTerminal("columnSource");
            var intoClauseOpt = new NonTerminal("intoClauseOpt");
            var fromClause = new NonTerminal("fromClause");
            var whereClauseOpt = new NonTerminal("whereClauseOpt");
            var idlist = new NonTerminal("idlist");

            var expression = new NonTerminal("expression");
            var term = new NonTerminal("term");
            var binExpr = new NonTerminal("binExpr");
            var parExpr = new NonTerminal("parExpr");
            var unExpr = new NonTerminal("unExpr");

            var valueList = new NonTerminal("valueList");
            var valueItem = new NonTerminal("valueItem");

            var columnDefList = new NonTerminal("columnDefList");
            var columnDef = new NonTerminal("columnDef");
            var dataType = new NonTerminal("dataType");

            var columnAttributes = new NonTerminal("columnAttributes");
            var primaryKeyOpt = new NonTerminal("primaryKeyOpt");
            var uniqueOpt = new NonTerminal("uniqueOpt");
            var allowNullOpt = new NonTerminal("allowNullOpt");

            var updateItem = new NonTerminal("updateItem");
            var updateItemList = new NonTerminal("updateItemList");

            selRestrOpt.Rule = Empty | "DISTINCT";
            selList.Rule = columnItemList | "*";
            columnItemList.Rule = MakePlusRule(columnItemList, comma, columnItem);
            columnItem.Rule = columnSource + aliasOpt;
            aliasOpt.Rule = Empty | asOpt + Id;
            asOpt.Rule = Empty | "AS";
            columnSource.Rule = Id;
            intoClauseOpt.Rule = Empty | "INTO" + Id;
            fromClause.Rule = "FROM" + idlist;
            idlist.Rule = MakePlusRule(idlist, comma, Id);
            whereClauseOpt.Rule = Empty | "WHERE" + expression;

            expression.Rule = term | binExpr | parExpr | unExpr;
            term.Rule = term.Rule = number | floatNumber | stringLiteral | boolLiteral | dateTimeLiteral | identifier;
            binExpr.Rule = expression + ToTerm("+") + expression
                         | expression + ToTerm("-") + expression
                         | expression + ToTerm("*") + expression
                         | expression + ToTerm("/") + expression
                         | expression + ToTerm("=") + expression
                         | expression + ToTerm("<") + expression
                         | expression + ToTerm(">") + expression
                         | expression + ToTerm("<=") + expression
                         | expression + ToTerm(">=") + expression
                         | expression + ToTerm("<>") + expression;
            parExpr.Rule = "(" + expression + ")";
            unExpr.Rule = ToTerm("-") + expression | ToTerm("NOT") + expression;

            valueList.Rule = MakePlusRule(valueList, comma, valueItem);
            valueItem.Rule = number | floatNumber | stringLiteral | boolLiteral | dateTimeLiteral;

            columnDefList.Rule = MakePlusRule(columnDefList, comma, columnDef);
            primaryKeyOpt.Rule = Empty | "PRIMARY KEY";
            uniqueOpt.Rule = Empty | "UNIQUE";
            allowNullOpt.Rule = Empty | "NULL" | "NOT NULL";

            columnAttributes.Rule = primaryKeyOpt + uniqueOpt + allowNullOpt;
            columnDef.Rule = Id + dataType + columnAttributes;
            dataType.Rule = ToTerm("INT") | ToTerm("FLOAT") | ToTerm("BOOL") | ToTerm("DATETIME") | ToTerm("STRING");

            updateItem.Rule = Id + "=" + valueItem;
            updateItemList.Rule = MakePlusRule(updateItemList, comma, updateItem);

            selectStmt.Rule = "SELECT" + selRestrOpt + selList + fromClause + whereClauseOpt;
            insertStmt.Rule = "INSERT" + "INTO" + Id + "(" + idlist + ")" + "VALUES" + "(" + valueList + ")";
            createTableStmt.Rule = "CREATE" + "TABLE" + Id + "(" + columnDefList + ")";
            updateStmt.Rule = "UPDATE" + Id + "SET" + updateItemList + whereClauseOpt;
            deleteStmt.Rule = "DELETE" + "FROM" + Id + whereClauseOpt;
            dropTableStmt.Rule = "DROP" + "TABLE" + Id;

            var root = new NonTerminal("root");
            root.Rule = createTableStmt | selectStmt | insertStmt | updateStmt | deleteStmt | dropTableStmt;

            Root = root;

            MarkPunctuation("SELECT", "FROM", "SET", "WHERE", "INTO", "AS", "(", ")", ",");
            RegisterBracePair("(", ")");
            MarkTransient(selRestrOpt, aliasOpt, asOpt, columnSource);
        }
    }
}