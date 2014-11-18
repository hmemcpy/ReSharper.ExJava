using japa.parser.ast.expr;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaToCSharp.Expressions
{
    public class FieldAccessExpressionVisitor : ExpressionVisitor<FieldAccessExpr>
    {
        public override ExpressionSyntax Visit(ConversionContext context, FieldAccessExpr fieldAccessExpr)
        {
            var scope = fieldAccessExpr.getScope();
            ExpressionSyntax scopeSyntax = null;

            if (scope != null)
            {
                scopeSyntax = VisitExpression(context, scope);
            }

            var identifierScope = scopeSyntax as IdentifierNameSyntax;
            if (identifierScope != null)
            {
                scopeSyntax = Syntax.IdentifierName(TypeHelper.ConvertType(identifierScope.Identifier.ValueText));
            }

            string field = TypeHelper.ConvertScopedIdentifierName(scopeSyntax, fieldAccessExpr.getField());

            return Syntax.MemberAccessExpression(SyntaxKind.MemberAccessExpression, scopeSyntax, Syntax.IdentifierName(field));
        }
    }
}
