using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

namespace DouglasDwyer.ImpGenerator
{
    public class SharedTypeBuilder : MemberBuilder
    {
        public INamedTypeSymbol Symbol { get; private set; }
        public INamedTypeSymbol InterfaceSymbol { get; private set; }
        public GeneratorExecutionContext Context { get; }
        public string InterfaceName { get; }
        public string InterfaceNamespace { get; }
        public string FullInterfaceName { get; }

        public SharedTypeBuilder(INamedTypeSymbol symbol, SemanticModel model, GeneratorExecutionContext context, string interfaceName, string interfaceNamespace) : base(model)
        {
            Symbol = symbol;
            Context = context;
            InterfaceName = interfaceName;
            InterfaceNamespace = interfaceNamespace;
            FullInterfaceName = GetFullNameOfType(interfaceName, interfaceNamespace);
        }

        public virtual SyntaxTree GenerateInterfaceDefinition()
        {
            if(Symbol.ContainingType != null)
            {
                Context.ReportDiagnostic(Diagnostic.Create(ImpRules.NestedSharedClassError, Symbol.Locations.FirstOrDefault(), Symbol.Name, Symbol.ContainingType.Name));
            }

            SyntaxTriviaList leadingTrivia = SyntaxFactory.TriviaList();

            foreach (SyntaxReference reference in Symbol.DeclaringSyntaxReferences)
            {
                leadingTrivia = SyntaxFactory.TriviaList(leadingTrivia.Concat(reference.GetSyntax().GetLeadingTrivia()));
            }

            SyntaxNode node = SyntaxFactory.InterfaceDeclaration(InterfaceName)
                .WithTypeParameterList(GetTypeParameterList())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithLeadingTrivia(leadingTrivia);
            if (!string.IsNullOrEmpty(InterfaceNamespace))
            {
                node = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(InterfaceNamespace)).WithMembers(SyntaxFactory.SingletonList(node));
            }
            SyntaxTree tree = Symbol.DeclaringSyntaxReferences[0].SyntaxTree;
            return tree.WithRootAndOptions(tree.GetCompilationUnitRoot().WithUsings(SyntaxFactory.List<UsingDirectiveSyntax>()).WithMembers(SyntaxFactory.List(new[] { GeneratePartialClass(), node })).NormalizeWhitespace(), tree.Options);
        }

        public virtual SyntaxTree GenerateInterfaceInheritance()
        {
            SyntaxNode node = SyntaxFactory.InterfaceDeclaration(InterfaceName)
                .WithTypeParameterList(GetTypeParameterList())
                .WithBaseList(GenerateInterfaceBaseList())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)));
            if (!string.IsNullOrEmpty(InterfaceNamespace))
            {
                node = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(InterfaceNamespace)).WithMembers(SyntaxFactory.SingletonList(node));
            }
            SyntaxTree tree = Symbol.DeclaringSyntaxReferences[0].SyntaxTree;
            return tree.WithRootAndOptions(tree.GetCompilationUnitRoot()
                .WithUsings(SyntaxFactory.List<UsingDirectiveSyntax>())
                .WithMembers(SyntaxFactory.List(new[] { GeneratePartialClass(), node, })).NormalizeWhitespace()
                .WithAttributeLists(SyntaxFactory.SingletonList(GetShareAsAttributeSyntax())),
                tree.Options);
        }

        public virtual SyntaxTree GenerateInterfaceImplementation()
        {
            SyntaxTree tree = Symbol.DeclaringSyntaxReferences[0].SyntaxTree;
            return tree.WithRootAndOptions(tree.GetCompilationUnitRoot().WithUsings(SyntaxFactory.List<UsingDirectiveSyntax>()).WithMembers(GenerateNamespaceAndMemberNodes()).NormalizeWhitespace(), tree.Options);
        }

        public override void SetModel(SemanticModel model)
        {
            base.SetModel(model);
            Symbol = (INamedTypeSymbol)Model.GetDeclaredSymbol(Symbol.DeclaringSyntaxReferences[0].GetSyntax());
            string s = FullInterfaceName.Replace("global::", "");
            if(Symbol.IsGenericType)
            {
                s += "`" + Symbol.TypeParameters.Length;
            }
            InterfaceSymbol = GetTypeByMetadataNameReplacement(s, Model.Compilation.GlobalNamespace);
        }

        private INamedTypeSymbol GetTypeByMetadataNameReplacement(string name, INamespaceSymbol namespaceToSearch)
        {
            INamedTypeSymbol found = namespaceToSearch.GetTypeMembers().FirstOrDefault(x => GetMetadataNameOfType(x) == name);
            if(found is null)
            {
                foreach(INamespaceSymbol newNamespace in namespaceToSearch.GetNamespaceMembers())
                {
                    found = GetTypeByMetadataNameReplacement(name, newNamespace);
                    if(found != null)
                    {
                        return found;
                    }
                }
                return null;
            }
            else
            {
                return found;
            }
        }

        private string GetMetadataNameOfType(INamedTypeSymbol symbol)
        {
            string toRet = GetFullNameOfType(symbol).Replace("global::", "");
            if(symbol.IsGenericType)
            {
                toRet += "`" + symbol.TypeParameters.Length;
            }
            return toRet;
        }

        protected virtual AttributeListSyntax GetShareAsAttributeSyntax()
        {
            NameSyntax synt = GetSyntaxForType(Symbol);
            if(synt is GenericNameSyntax gen)
            {
                synt = gen.WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(Symbol.TypeParameters.Select(x => SyntaxFactory.OmittedTypeArgument()))));
            }
            NameSyntax name = GetSyntaxForType(InterfaceSymbol);
            if (name is GenericNameSyntax gen2)
            {
                name = gen2.WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(InterfaceSymbol.TypeParameters.Select(x => SyntaxFactory.OmittedTypeArgument()))));
            }

            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(GetSyntaxForType(Model.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.ShareAsAttribute")))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                            new SyntaxNodeOrToken[]{
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.TypeOfExpression(
                                        synt
                                        )),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.TypeOfExpression(
                                        name))})))))
                .WithTarget(
                    SyntaxFactory.AttributeTargetSpecifier(
                    SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)));
        }

        protected virtual SyntaxList<SyntaxNode> GenerateNamespaceAndMemberNodes()
        {
            return SyntaxFactory.List(new[] { GenerateProxyInterface() });
        }

        protected virtual SyntaxNode GenerateProxyInterface()
        {
            SyntaxNode node = SyntaxFactory.InterfaceDeclaration(InterfaceName)
                .WithTypeParameterList(GetTypeParameterList())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithConstraintClauses(GetGenericConstraintClauses())
                .WithMembers(GenerateMemberDeclarations(Symbol));
            if (string.IsNullOrEmpty(InterfaceNamespace))
            {
                return node;
            }
            else
            {
                return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(InterfaceNamespace)).WithMembers(SyntaxFactory.SingletonList(node));
            }
        }

        protected virtual SyntaxList<TypeParameterConstraintClauseSyntax> GetGenericConstraintClauses()
        {
            return SyntaxFactory.List(Symbol.TypeParameters.Select(x => GetGenericParameterConstraintSyntax(x)).Where(x => x != null));
        }

        protected virtual BaseListSyntax GenerateInterfaceBaseList()
        {
            List<INamedTypeSymbol> interfaces = GenerateInterfaceBaseListInternal(Symbol);
            if(interfaces.Count > 0)
            {
                return SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(interfaces.Select(x => SyntaxFactory.SimpleBaseType(GetSyntaxForType(x)))));
            }
            else
            {
                return null;
            }
        }

        private List<INamedTypeSymbol> GenerateInterfaceBaseListInternal(INamedTypeSymbol type)
        {
            List<INamedTypeSymbol> interfaces;
            if (type.BaseType is null || (GetFullNameOfType(type.BaseType) == FullInterfaceName && type.BaseType.TypeParameters.Count() == Symbol.TypeParameters.Count()))
            {
                interfaces = new List<INamedTypeSymbol>();
            }
            else if (type.BaseType.TypeKind != TypeKind.Class)
            {
                interfaces = new List<INamedTypeSymbol>();
                if (GetFullNameOfType(type.BaseType) != FullInterfaceName)
                {
                    interfaces.Add(type.BaseType);
                }
            }
            else
            {
                interfaces = GenerateInterfaceBaseListInternal(type.BaseType);
            }
            interfaces.AddRange(type.Interfaces);
            return interfaces.Distinct().ToList();
        }

        protected virtual TypeParameterListSyntax GetTypeParameterList()
        {
            return Symbol.TypeParameters.Count() > 0 ? SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(Symbol.TypeParameters.Select(x => SyntaxFactory.TypeParameter(x.Name).WithAttributeLists(GetAttributeList(x))))) : null;
        }

        protected virtual TypeArgumentListSyntax GetTypeArgumentList()
        {
            return Symbol.TypeParameters.Count() > 0 ? SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(Symbol.TypeParameters.Select(x => (TypeSyntax)GetSyntaxForType(x)))) : null;
        }

        protected virtual SyntaxNode GeneratePartialClass()
        {
            SyntaxNode node = SyntaxFactory.ClassDeclaration(Symbol.Name)
                    .WithTypeParameterList(GetTypeParameterList())
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                    .WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(GetInterfaceIdentifier()))));
            if (Symbol.ContainingNamespace is null || Symbol.ContainingNamespace.IsGlobalNamespace)
            {
                return node;
            }
            else
            {
                return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(GetFullNamespaceName(Symbol.ContainingNamespace))).WithMembers(SyntaxFactory.SingletonList(node));
            }
        }

        protected virtual NameSyntax GetInterfaceIdentifier()
        {
            if (Symbol.IsGenericType)
            {
                return SyntaxFactory.GenericName(GetFullNameOfType(InterfaceName, InterfaceNamespace)).WithTypeArgumentList(GetTypeArgumentList());
            }
            else
            {
                return SyntaxFactory.IdentifierName(GetFullNameOfType(InterfaceName, InterfaceNamespace));
            }
        }

        protected virtual SyntaxList<SyntaxNode> GenerateMemberDeclarations(INamedTypeSymbol type)
        {
            return SyntaxFactory.List(GenerateMemberDeclarationsInternal(type, new Dictionary<string, MemberDeclarationSyntax>()).Values);
        }

        private Dictionary<string,MemberDeclarationSyntax> GenerateMemberDeclarationsInternal(INamedTypeSymbol type, Dictionary<string,MemberDeclarationSyntax> members)
        {
            INamedTypeSymbol attributeSymbol = Model.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.LocalAttribute");
            foreach (ISymbol memberSymbol in type.GetMembers())
            {
                if (!members.ContainsKey(memberSymbol.Name) && !memberSymbol.GetAttributes().Any(x => x.AttributeClass.Equals(attributeSymbol)))
                {
                    if (memberSymbol is IMethodSymbol method && method.DeclaredAccessibility == Accessibility.Public && method.MethodKind == MethodKind.Ordinary && !method.ExplicitInterfaceImplementations.Any())
                    {
                        CheckMethod(method);
                        members.Add(method.Name, new MethodBuilder(method, Model).GenerateMethod());
                    }
                    else if (memberSymbol is IPropertySymbol property && property.DeclaredAccessibility == Accessibility.Public && !property.ExplicitInterfaceImplementations.Any())
                    {
                        members.Add(property.Name, new PropertyBuilder(property, Model).GenerateProperty());
                    }
                }
            }
            if(!(type.BaseType.SpecialType == SpecialType.System_Object || type.BaseType.TypeKind != TypeKind.Class))
            {
                GenerateMemberDeclarationsInternal(type.BaseType, members);
            }
            return members;
        }

        private TypeParameterListSyntax GetTypeParameters(IEnumerable<ITypeParameterSymbol> symbols)
        {
            return SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(symbols.Select(x => GetTypeParameter(x))));
        }

        private TypeParameterSyntax GetTypeParameter(ITypeParameterSymbol symbol)
        {
            return SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(symbol.Name));
        }

        private void CheckMethod(IMethodSymbol symbol)
        {
            INamedTypeSymbol unreliableSymbol = Model.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.UnreliableAttribute");
            bool isUnreliable = symbol.GetAttributes().Any(x => x.AttributeClass.Equals(unreliableSymbol));
            INamedTypeSymbol attributeSymbol = Model.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.CallingClientAttribute");
            INamedTypeSymbol impClientType = Model.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.IImpClient");
            if (isUnreliable && symbol.ReturnType.SpecialType != SpecialType.System_Void)
            {
                Context.ReportDiagnostic(Diagnostic.Create(ImpRules.UnreliableMethodReturnTypeError, symbol.Locations.FirstOrDefault(), symbol.Name, symbol.ReturnType.Name));
            }
            bool hasCallingClient = false;
            foreach (IParameterSymbol para in symbol.Parameters)
            {
                if (para.GetAttributes().Any(x => x.AttributeClass.Equals(attributeSymbol)))
                {
                    if (hasCallingClient)
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(ImpRules.CallingClientParameterCountError, symbol.Locations.FirstOrDefault(), symbol.Name, para.Name));
                    }
                    else if (!IsTypeSymbolAssignable(impClientType, para.Type))
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(ImpRules.CallingClientParameterTypeError, symbol.Locations.FirstOrDefault(), symbol.Name, para.Name));
                        hasCallingClient = true;
                    }
                }
                else if (isUnreliable)
                {
                    if (para.Type.SpecialType != SpecialType.System_String && para.Type.IsReferenceType)
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(ImpRules.UnreliableMethodReferenceParameterError, symbol.Locations.FirstOrDefault(), symbol.Name, para.Name));
                    }
                    else if (CanStructHoldReferences(para.Type, para.Type))
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(ImpRules.UnreliableMethodValueParameterError, symbol.Locations.FirstOrDefault(), symbol.Name, para.Name));
                    }
                }
            }
        }

        private bool CanStructHoldReferences(ITypeSymbol symbol, ITypeSymbol baseSymbol)
        {
            return symbol.GetMembers().Any(x => x is IFieldSymbol field && !field.IsStatic && ((field.Type.IsReferenceType && field.Type.SpecialType != SpecialType.System_String) || (!baseSymbol.Equals(field.Type) && CanStructHoldReferences(field.Type, baseSymbol))));
        }
    }

    public class OldSharedType
    {
        public ITypeSymbol TypeSymbol;
        public string InterfaceName;
        public string InterfaceNamespace;
        public string RemoteName;
        public string RemoteNamespace;
        public List<string> UsingDirectives;

        public string FullInterfaceName => "global::" + (string.IsNullOrEmpty(InterfaceNamespace) ? InterfaceName : InterfaceNamespace + "." + InterfaceName);
        public string FullRemoteName => "global::" + (string.IsNullOrEmpty(RemoteNamespace) ? RemoteName : RemoteNamespace + "." + RemoteName);

        public OldSharedType(ITypeSymbol typeSymbol, string interfaceName, string interfaceNamespace, string remoteName, string remoteNamespace, List<string> usingDirectives)
        {
            TypeSymbol = typeSymbol;
            InterfaceName = interfaceName;
            InterfaceNamespace = interfaceNamespace;
            RemoteName = remoteName;
            RemoteNamespace = remoteNamespace;
            UsingDirectives = usingDirectives;
        }
    }
}
