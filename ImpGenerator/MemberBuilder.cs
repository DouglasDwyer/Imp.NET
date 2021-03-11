using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DouglasDwyer.ImpGenerator
{


    public abstract class MemberBuilder
    {
        public SemanticModel Model { get; private set; }

        public MemberBuilder(SemanticModel model)
        {
            Model = model;
        }

        public virtual void SetModel(SemanticModel model)
        {
            Model = model;
        }

        protected TypeParameterSyntax GetSyntaxForTypeDeclaration(ITypeParameterSymbol symbol)
        {
            return SyntaxFactory.TypeParameter(symbol.Name).WithAttributeLists(GetAttributeList(symbol));
        }

        protected SyntaxTokenList GetModifiersForParameter(IParameterSymbol symbol)
        {
            SyntaxTokenList toReturn = SyntaxFactory.TokenList();
            if (symbol.IsParams)
            {
                toReturn = toReturn.Add(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
            }
            if (symbol.RefKind == RefKind.In)
            {
                toReturn = toReturn.Add(SyntaxFactory.Token(SyntaxKind.InKeyword));
            }
            if (symbol.RefKind == RefKind.Out)
            {
                toReturn = toReturn.Add(SyntaxFactory.Token(SyntaxKind.OutKeyword));
            }
            if (symbol.RefKind == RefKind.Ref)
            {
                toReturn = toReturn.Add(SyntaxFactory.Token(SyntaxKind.RefKeyword));
            }
            return toReturn;
        }

        protected SyntaxList<AttributeListSyntax> GetAttributeList(ISymbol symbol)
        {
            ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
            SyntaxList<AttributeListSyntax> toReturn;
            if (attributes.Count() > 0)
            {
                toReturn = SyntaxFactory.List(attributes.Select(x => SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(GetSyntaxForAttribute(x)))));
            }
            else
            {
                toReturn = SyntaxFactory.List<AttributeListSyntax>();
            }
            if (symbol is IMethodSymbol method)
            {
                attributes = method.GetReturnTypeAttributes();
                if (attributes.Count() > 0)
                {
                    toReturn = toReturn.AddRange(attributes.Select(x => SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(GetSyntaxForAttribute(x))).WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.ReturnKeyword)))));
                }
            }
            return toReturn;
        }

        protected AttributeSyntax GetSyntaxForAttribute(AttributeData attribute)
        {
            return SyntaxFactory.Attribute(GetSyntaxForType(attribute.AttributeClass) as NameSyntax, GetAttributeArgumentSyntax(attribute));
        }

        protected List<KeyValuePair<string, TypedConstant>> GetAttributeArguments(AttributeData attribute)
        {
            List<KeyValuePair<string, TypedConstant>> toReturn = new List<KeyValuePair<string, TypedConstant>>();
            foreach (TypedConstant constant in attribute.ConstructorArguments)
            {
                toReturn.Add(new KeyValuePair<string, TypedConstant>(null, constant));
            }
            foreach (KeyValuePair<string, TypedConstant> constant in attribute.NamedArguments)
            {
                toReturn.Add(constant);
            }
            return toReturn;
        }

        protected AttributeArgumentListSyntax GetAttributeArgumentSyntax(AttributeData data)
        {
            return SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(GetAttributeArguments(data).Select(x => {
                AttributeArgumentSyntax syntax = SyntaxFactory.AttributeArgument(GetConstantSyntax(x.Value));
                if (x.Key != null)
                {
                    syntax = syntax.WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(x.Key)));
                }
                return syntax;
            })));
        }

        protected EqualsValueClauseSyntax GetDefaultValueSyntax(IParameterSymbol symbol)
        {
            if (symbol.IsOptional)
            {
                if (symbol.Type.TypeKind == TypeKind.Enum)
                {
                    return SyntaxFactory.EqualsValueClause(SyntaxFactory.CastExpression(
                                        GetSyntaxForType(symbol.Type),
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal((int)symbol.ExplicitDefaultValue))));
                }
                else if (symbol.ExplicitDefaultValue is null)
                {
                    return SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword)));
                }
                else if (symbol.Type.SpecialType == SpecialType.System_String)
                {
                    return SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal((string)symbol.ExplicitDefaultValue)));
                }
                else if (symbol.Type.SpecialType == SpecialType.System_Char)
                {
                    return SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal((char)symbol.ExplicitDefaultValue)));
                }
                else
                {
                    //Abuse the way dynamic works to to call the Literal() overload whose parameter type exactly matches the type of our number.
                    return SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((dynamic)symbol.ExplicitDefaultValue)));
                }
            }
            else
            {
                return null;
            }
        }

        protected ExpressionSyntax GetConstantSyntax(TypedConstant constant)
        {
            if (constant.Kind == TypedConstantKind.Array)
            {
                return SyntaxFactory.ArrayCreationExpression(
                                                SyntaxFactory.ArrayType(GetSyntaxForType(((IArrayTypeSymbol)constant.Type).ElementType))
                                                .WithRankSpecifiers(
                                                    SyntaxFactory.SingletonList(
                                                        SyntaxFactory.ArrayRankSpecifier(
                                                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                                SyntaxFactory.OmittedArraySizeExpression())))))
                                            .WithInitializer(
                                                SyntaxFactory.InitializerExpression(
                                                    SyntaxKind.ArrayInitializerExpression,
                                                    SyntaxFactory.SeparatedList(constant.Values.Select(x => GetConstantSyntax(x)))));
            }
            else
            {
                if (constant.Value is null)
                {
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                }
                else
                {
                    if (constant.Kind == TypedConstantKind.Type)
                    {
                        NameSyntax name = GetSyntaxForType((INamedTypeSymbol)constant.Value) as NameSyntax;
                        return SyntaxFactory.TypeOfExpression(name);
                    }
                    else
                    {
                        return SyntaxFactory.ParseExpression(constant.ToCSharpString());
                    }
                }
            }
        }

        protected TypeSyntax GetSyntaxForType(ITypeSymbol type)
        {
            if(type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return SyntaxFactory.NullableType(GetSyntaxForType(type.WithNullableAnnotation(NullableAnnotation.None)));
            }
            if(type.SpecialType == SpecialType.System_Void)
            {
                return SyntaxFactory.IdentifierName("void");
            }
            else if (type is INamedTypeSymbol symbol)
            {
                if (symbol.IsGenericType)
                {
                    return SyntaxFactory.GenericName(GetFullNameOfType(symbol)).WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(symbol.TypeArguments.Select(x => (TypeSyntax)(x is INamedTypeSymbol named ? GetSyntaxForType(named) : SyntaxFactory.IdentifierName(x.Name))))));
                }
                else
                {
                    return SyntaxFactory.IdentifierName(GetFullNameOfType(symbol));
                }
            }
            else
            {
                return SyntaxFactory.ParseName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }
        }

        public static string GetFullNameOfType(INamedTypeSymbol symbol)
        {
            return GetFullNameOfType(symbol.Name.Replace("global::", ""), GetFullNamespaceName(symbol.ContainingNamespace));
        }

        public static string GetFullNameOfType(string typeName, string namespaceName)
        {
            return "global::" + (string.IsNullOrEmpty(namespaceName) ? typeName : namespaceName + "." + typeName);
        }

        public static string GetFullNamespaceName(INamespaceSymbol symbol)
        {
            if(symbol is null || symbol.IsGlobalNamespace)
            {
                return null;
            }
            else
            {
                string toAdd = GetFullNamespaceName(symbol.ContainingNamespace);
                return (toAdd is null ? "" : toAdd + ".") + symbol.Name;
            }
        }

        protected static bool IsTypeSymbolAssignable(ITypeSymbol to, ITypeSymbol from)
        {
            if(to.IsAbstract && from.AllInterfaces.Any(x => x.Equals(to) || (to is INamedTypeSymbol namedTo && GetFullNameOfType(namedTo) == GetFullNameOfType(x) && namedTo.TypeParameters.Count() == x.TypeParameters.Count())))
            {
                return true;
            }
            else
            {
                ITypeSymbol baseClass = from;
                while(baseClass != null)
                {
                    if(baseClass.Equals(to))
                    {
                        return true;
                    }
                    else if(to is INamedTypeSymbol namedTo && from is INamedTypeSymbol namedFrom && GetFullNameOfType(namedTo) == GetFullNameOfType(namedFrom) && namedTo.TypeParameters.Count() == namedFrom.TypeParameters.Count())
                    {
                        return true;
                    }
                    else
                    {
                        baseClass = baseClass.BaseType;
                    }
                }
                return false;
            }
        }

        protected TypeParameterConstraintClauseSyntax GetGenericParameterConstraintSyntax(ITypeParameterSymbol symbol)
        {
            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>();
            if (symbol.HasReferenceTypeConstraint)
            {
                constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
            }
            else if (symbol.HasValueTypeConstraint)
            {
                constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
            }
            if (symbol.HasUnmanagedTypeConstraint)
            {
                constraints = constraints.Add(SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName("unmanaged")));
            }
            if (symbol.HasNotNullConstraint)
            {
                constraints = constraints.Add(SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName("notnull")));
            }

            foreach (ITypeSymbol type in symbol.ConstraintTypes)
            {
                constraints = constraints.Add(SyntaxFactory.TypeConstraint(GetSyntaxForType(type)));
            }

            if (symbol.HasConstructorConstraint)
            {
                constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
            }

            return constraints.Count > 0 ? SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(symbol.Name)).WithConstraints(constraints) : null;
        }
    }
}
