using japa.parser.ast.expr;
using Roslyn.Compilers.CSharp;
using System;

namespace JavaToCSharp.Expressions
{
    public class IntegerLiteralExpressionVisitor : ExpressionVisitor<IntegerLiteralExpr>
    {
        public override ExpressionSyntax Visit(ConversionContext context, IntegerLiteralExpr expr)
        {
            string value = expr.toString();

            if (value.StartsWith("0x"))
                return Syntax.LiteralExpression(SyntaxKind.NumericLiteralExpression, Syntax.Literal(value, Convert.ToInt32(value.Substring(2), 16)));

            int intValue;
            if (int.TryParse(value, out intValue))
                return Syntax.LiteralExpression(SyntaxKind.NumericLiteralExpression, Syntax.Literal(intValue));
            long longValue;
            if (long.TryParse(value, out longValue))
                return Syntax.LiteralExpression(SyntaxKind.NumericLiteralExpression, Syntax.Literal(longValue));

            return Syntax.LiteralExpression(SyntaxKind.NumericLiteralExpression, Syntax.Literal(value));
        }
    }
}
