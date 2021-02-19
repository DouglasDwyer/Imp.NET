using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.ImpGenerator
{
    public class PropertyBuilder : MemberBuilder
    {
        public IPropertySymbol Symbol { get; }

        public PropertyBuilder(IPropertySymbol symbol, SemanticModel model) : base(model)
        {
            Symbol = symbol;
        }

        public BasePropertyDeclarationSyntax GenerateProperty()
        {
            if (Symbol.IsIndexer)
            {
                return SyntaxFactory.IndexerDeclaration(GetSyntaxForType(Symbol.Type))
                    .WithParameterList(GetParameterListSyntax())
                    .WithAttributeLists(GetAttributeList(Symbol))
                    .WithAccessorList(GetAccessorList());
            }
            else
            {
                return SyntaxFactory.PropertyDeclaration(GetSyntaxForType(Symbol.Type), Symbol.Name)
                    .WithAttributeLists(GetAttributeList(Symbol))
                    .WithAccessorList(GetAccessorList());
            }
        }

        protected BracketedParameterListSyntax GetParameterListSyntax()
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
            return SyntaxFactory.BracketedParameterList(parameters);
        }

        protected AccessorListSyntax GetAccessorList()
        {
            AccessorListSyntax syntax = SyntaxFactory.AccessorList();
            if (Symbol.GetMethod != null && Symbol.GetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                syntax = syntax.AddAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithAttributeLists(GetAttributeList(Symbol.GetMethod))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }
            if (Symbol.SetMethod != null && Symbol.SetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                syntax = syntax.AddAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithAttributeLists(GetAttributeList(Symbol.SetMethod))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }
            return syntax;
        }
    }
}
