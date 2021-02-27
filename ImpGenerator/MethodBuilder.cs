using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DouglasDwyer.ImpGenerator
{
    public class MethodBuilder : MemberBuilder
    {
        public IMethodSymbol Symbol { get; }

        public MethodBuilder(IMethodSymbol symbol, SemanticModel model) : base(model)
        {
            Symbol = symbol;
        }

        public MethodDeclarationSyntax GenerateMethod()
        {
            TypeSyntax returnSyntax = GetSyntaxForType(Symbol.ReturnType);
            if(Symbol.ReturnsByRef)
            {
                if (Symbol.ReturnsByRefReadonly)
                {
                    returnSyntax = SyntaxFactory.RefType(returnSyntax).WithReadOnlyKeyword(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
                }
                else
                {
                    returnSyntax = SyntaxFactory.RefType(returnSyntax);
                }
            }

            return SyntaxFactory.MethodDeclaration(returnSyntax, SyntaxFactory.Identifier(Symbol.Name))
                .WithLeadingTrivia(GetTriviaListForMethod(Symbol))
                .WithTypeParameterList(GetTypeParameterListSyntax())
                .WithParameterList(GetParameterListSyntax())
                .WithAttributeLists(GetAttributeList(Symbol))
                .WithConstraintClauses(SyntaxFactory.List(Symbol.TypeParameters.Select(x => GetGenericParameterConstraintSyntax(x)).Where(x => x != null)))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private SyntaxTriviaList GetTriviaListForMethod(IMethodSymbol method)
        {
            SyntaxTriviaList leadingTrivia = SyntaxFactory.TriviaList();
            foreach (SyntaxReference reference in method.DeclaringSyntaxReferences)
            {
                leadingTrivia = SyntaxFactory.TriviaList(leadingTrivia.Concat(reference.GetSyntax().GetLeadingTrivia()).Where(x => !string.IsNullOrWhiteSpace(x.ToFullString())));
            }
            if(!leadingTrivia.Any() && method.OverriddenMethod != null)
            {
                return GetTriviaListForMethod(method.OverriddenMethod);
            }
            else
            {
                return leadingTrivia;
            }
        }

        protected ParameterListSyntax GetParameterListSyntax()
        {
            SeparatedSyntaxList<ParameterSyntax> parameters = SyntaxFactory.SeparatedList<ParameterSyntax>();
            foreach (IParameterSymbol param in Symbol.Parameters)
            {
                parameters = parameters.Add(
                    SyntaxFactory.Parameter(
                        GetAttributeList(param),
                        GetModifiersForParameter(param),
                        GetSyntaxForType(param.Type),
                        SyntaxFactory.Identifier(param.Name),
                        GetDefaultValueSyntax(param)));
            }
            return SyntaxFactory.ParameterList(parameters);
        }

        protected TypeSyntax GetReturnTypeSyntax()
        {
            return SyntaxFactory.ParseTypeName(Symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        protected TypeParameterListSyntax GetTypeParameterListSyntax()
        {
            return Symbol.IsGenericMethod ?
                SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(Symbol.TypeParameters.Select(x => SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(x.Name))))) :
                null;
        }
    }
}
