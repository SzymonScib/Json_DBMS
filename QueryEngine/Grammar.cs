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
            var identifier = new IdentifierTerminal("identifier");
            var number = new NumberLiteral("number");
            var stringLiteral = new StringLiteral("string", "'");
            var comma = ToTerm(",");
            var Id = identifier; 

            var selectStmt = new NonTerminal("selectStmt");

            var selRestrOpt = new NonTerminal("selRestrOpt");
            var selList = new NonTerminal("selList");
            var columnItemList = new NonTerminal("columnItemList");
            var columnItem = new NonTerminal("columnItem");
            var aliasOpt = new NonTerminal("aliasOpt");
            var asOpt = new NonTerminal("asOpt");
            var columnSource = new NonTerminal("columnSource");
            var intoClauseOpt = new NonTerminal("intoClauseOpt");
            var fromClauseOpt = new NonTerminal("fromClauseOpt");
            var whereClauseOpt = new NonTerminal("whereClauseOpt");
            var idlist = new NonTerminal("idlist");

            var expression = new NonTerminal("expression");
            var term = new NonTerminal("term");
            var binExpr = new NonTerminal("binExpr");
            var parExpr = new NonTerminal("parExpr");
            var unExpr = new NonTerminal("unExpr");

            selRestrOpt.Rule = Empty | "DISTINCT";
            selList.Rule = columnItemList | "*";
            columnItemList.Rule = MakePlusRule(columnItemList, comma, columnItem);
            columnItem.Rule = columnSource + aliasOpt;
            aliasOpt.Rule = Empty | asOpt + Id;
            asOpt.Rule = Empty | "AS";
            columnSource.Rule = Id;
            intoClauseOpt.Rule = Empty | "INTO" + Id;
            fromClauseOpt.Rule = Empty | "FROM" + idlist;
            idlist.Rule = MakePlusRule(idlist, comma, Id);
            whereClauseOpt.Rule = Empty | "WHERE" + expression;

            expression.Rule = term | binExpr | parExpr | unExpr;
            term.Rule = number | identifier | stringLiteral;
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

            selectStmt.Rule = "SELECT" + selRestrOpt + selList + fromClauseOpt + whereClauseOpt;

            Root = selectStmt;

            MarkPunctuation("SELECT", "FROM", "WHERE", "INSERT INTO", "VALUES", "UPDATE", "SET", "DELETE FROM", "CREATE INDEX", "ON", "(", ")", ",", "AS", "INTO");
            RegisterBracePair("(", ")");
            MarkTransient(selRestrOpt, aliasOpt, asOpt, columnSource);
        }
    }
}