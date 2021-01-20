using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DouglasDwyer.Imp;
using System.Text;

namespace DouglasDwyer.ImpGenerator
{
    [Generator]
    public class ProxyGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new ProxyDataReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is ProxyDataReceiver receiver)
            {
                CreateProxyTypes(context, receiver.CandidateMakeSharedAttributes);
                List<ProxyType> proxyInterfaces = GetProxyTypes(context, receiver.CandidateRemoteTypes);
                List<SharedType> sharedClasses = GetSharedClasses(context, receiver.CandidateClasses);

                foreach(SharedType type in sharedClasses)
                {
                    context.AddSource("[ImpGenerated]" + type.TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", ""),
                        SourceText.From(GenerateProxyTypes(type, proxyInterfaces, sharedClasses, type.UsingDirectives), Encoding.Default));
                }
            }
        }

        private void CreateProxyTypes(GeneratorExecutionContext context, List<CompilationUnitSyntax> attributeList)
        {
            ITypeSymbol proxyAttribute = context.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.MakeSharedAttribute");
            AttributeData attribute = context.Compilation.Assembly.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(proxyAttribute));
            if (attribute != null)
            {
                ITypeSymbol typeToShare = (ITypeSymbol)attribute.ConstructorArguments[0].Value;
                context.AddSource("[ImpGenerated]" + typeToShare.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "").Replace("<","Of").Replace(">",""),
                    SourceText.From(GenerateRemoteTypeForInterface(typeToShare, "Remote" + typeToShare.Name.Replace("<", "Of").Replace(">", ""), "DouglasDwyer.Imp.Proxy." + (typeToShare.ContainingNamespace is null ? "" : typeToShare.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::",""))), Encoding.Default));
            }
        }

        private List<ProxyType> GetProxyTypes(GeneratorExecutionContext context, List<ClassDeclarationSyntax> candidateInterfaces)
        {
            ITypeSymbol proxyAttribute = context.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.ProxyForAttribute");
            List<ProxyType> toReturn = new List<ProxyType>();
            foreach (MetadataReference reference in context.Compilation.References)
            {
                if (context.Compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
                {
                    foreach (INamedTypeSymbol symbol in Flatten(assembly.GlobalNamespace.GetNamespaceMembers(), x => x.GetNamespaceMembers()).SelectMany(x => x.GetTypeMembers()))
                    {
                        AttributeData proxyMark = symbol.GetAttributes().Where(x => x.AttributeClass.Equals(proxyAttribute)).FirstOrDefault();
                        if (proxyMark != null)
                        {
                            toReturn.Add(new ProxyType((ITypeSymbol)proxyMark.ConstructorArguments[0].Value, symbol));
                        }
                    }
                }
            }
            foreach (ClassDeclarationSyntax classDeclaration in candidateInterfaces)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                ITypeSymbol classSymbol = (ITypeSymbol)model.GetDeclaredSymbol(classDeclaration);
                AttributeData proxyMark = classSymbol.GetAttributes().Where(x => x.AttributeClass.Equals(proxyAttribute)).FirstOrDefault();
                if (proxyMark != null)
                {
                    toReturn.Add(new ProxyType((ITypeSymbol)proxyMark.ConstructorArguments[0].Value, classSymbol));
                }
            }

            return toReturn;
        }

        private Dictionary<string, ISymbol> GetAllShownInterfaceMembers(ITypeSymbol symbol)
        {
            Dictionary<string, ISymbol> toReturn = new Dictionary<string, ISymbol>();
            foreach(ISymbol member in symbol.GetMembers())
            {
                toReturn[member.Name] = member;
            }
            foreach(ITypeSymbol subInterface in symbol.Interfaces)
            {
                foreach(KeyValuePair<string,ISymbol> value in GetAllShownInterfaceMembers(subInterface))
                {
                    if (value.Key.Contains("."))
                    {
                        if (!toReturn.ContainsKey(value.Key))
                        {
                            toReturn[value.Key] = value.Value;
                        }
                    }
                    else
                    {
                        if (toReturn.ContainsKey(value.Key))
                        {
                            toReturn[subInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + value.Key] = value.Value;
                        }
                        else
                        {
                            toReturn[value.Key] = value.Value;
                        }
                    }
                }
            }
            return toReturn;
        }

        private List<SharedType> GetSharedClasses(GeneratorExecutionContext context, List<ClassDeclarationSyntax> candidateClasses)
        {
            INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.SharedAttribute");

            List<SharedType> toReturn = new List<SharedType>();
            foreach (ClassDeclarationSyntax classDeclaration in candidateClasses)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                ITypeSymbol classSymbol = (ITypeSymbol)model.GetDeclaredSymbol(classDeclaration);
                if (classSymbol.GetAttributes().Where(x => x.AttributeClass.Equals(attributeSymbol)).Count() > 0)
                {
                    List<string> syntax = (from x in classDeclaration.SyntaxTree.GetRoot().DescendantNodes() where x is UsingDirectiveSyntax select ((UsingDirectiveSyntax)x).Name.ToString()).ToList();
                    if(classSymbol.ContainingNamespace != null)
                    {
                        syntax.Add(classSymbol.ContainingNamespace.Name);
                    }
                    string namespaceName = classSymbol.ContainingNamespace is null ? "" : classSymbol.ContainingNamespace.Name;
                    toReturn.Add(new SharedType(classSymbol, "I" + classSymbol.Name, namespaceName, "Remote" + classSymbol.Name, "DouglasDwyer.Imp.Proxy" + (namespaceName == "" ? "" : "." + namespaceName), syntax));
                }
            }
            return toReturn;
        }

        private IEnumerable<T> Flatten<T>(IEnumerable<T> x, Func<T,IEnumerable<T>> map)
        {
            return x.SelectMany(y => Flatten(map(y), map).Concat(new[] { y }));
        }

        private string GenerateRemoteTypeForInterface(ITypeSymbol interfaceSymbol, string remoteName, string remoteNamespace)
        {
            string template = string.IsNullOrEmpty(remoteNamespace) ? remoteGlobalTemplate : remoteNamespaceTemplate;
            string classMembers = "";
            foreach (KeyValuePair<string,ISymbol> memberSymbol in GetAllShownInterfaceMembers(interfaceSymbol))
            {
                if (memberSymbol.Value is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
                {
                    string parameters = string.Concat(method.Parameters.Select(x => x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " " + x.Name + ","));
                    string typelessParameters = string.Concat(method.Parameters.Select(x => x.Name + ","));
                    if (parameters.Length > 0)
                    {
                        parameters = parameters.Remove(parameters.Length - 1);
                        typelessParameters = typelessParameters.Remove(typelessParameters.Length - 1);
                    }

                    classMembers +=
                        (memberSymbol.Key.Contains(".") ? "" : method.DeclaredAccessibility.ToString().ToLowerInvariant()) + " " +
                        method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " " +
                        memberSymbol.Key + "(" + parameters + ") => HostClient.CallRemoteMethod<" + (method.ReturnType.SpecialType == SpecialType.System_Void ? "object" : method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)) + ">(Location, new object[] { " + typelessParameters + "});";
                }
                else if (memberSymbol.Value is IPropertySymbol property)
                {
                    if (property.IsIndexer)
                    {
                        string parameters = string.Concat(property.Parameters.Select(x => x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " " + x.Name + ","));
                        string typelessParameters = string.Concat(property.Parameters.Select(x => x.Name + ","));
                        if (parameters.Length > 0)
                        {
                            parameters = parameters.Remove(parameters.Length - 1);
                            typelessParameters = typelessParameters.Remove(typelessParameters.Length - 1);
                        }

                        classMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " this[" +
                            parameters + "] {" + (property.GetMethod is null ? "" : "get => HostClient.GetRemoteIndexer<" + property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">(Location, new object[] { " + typelessParameters + " });") + (property.SetMethod is null ? "" : "set => HostClient.SetRemoteIndexer<" + property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">(Location, value, new object[] { " + typelessParameters + " });") + " }";
                    }
                    else
                    {
                        classMembers +=
                            (memberSymbol.Key.Contains(".") ? "" : property.DeclaredAccessibility.ToString().ToLowerInvariant()) + " " +
                            property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " " +
                            memberSymbol.Key + " { " + (property.GetMethod is null ? "" : "get => HostClient.GetRemoteProperty<" + property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">(Location);") + (property.SetMethod is null ? "" : "set => HostClient.SetRemoteProperty(Location, value); ") + " }";
                    }
                }
            }
            return template
                .Replace("{REMOTE_NAMESPACE}", remoteNamespace)
                .Replace("{REMOTE_CLASS_NAME}", remoteName)
                .Replace("{FULL_INTERFACE_NAME}", interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .Replace("{CLASS_MEMBERS}", classMembers);
        }

        const string proxyNamespaceTemplate = @"
                [assembly: global::DouglasDwyer.Imp.ShareAs(typeof({FULL_CLASS_NAME}), typeof({FULL_INTERFACE_NAME}))]
                namespace {NAMESPACE} {
                    public partial class {CLASS_NAME} : {INTERFACE_NAME} {

                    }

                    public interface {INTERFACE_NAME} {BASE_TYPE_NAME} {
                        {INTERFACE_MEMBERS}
                    }
                }
            ";
        const string remoteNamespaceTemplate = @"
                /*namespace {REMOTE_NAMESPACE} {
                    [global::DouglasDwyer.Imp.ProxyFor(typeof({FULL_INTERFACE_NAME}))]
                    public class {REMOTE_CLASS_NAME} : global::DouglasDwyer.Imp.RemoteSharedObject, {FULL_INTERFACE_NAME} {
                        public {REMOTE_CLASS_NAME}(global::DouglasDwyer.Imp.SharedObjectPath location, global::DouglasDwyer.Imp.ImpClient host) : base(location, host) {}                        

                        {CLASS_MEMBERS}
                    }
                }*/
            ";
        const string proxyGlobalTemplate = @"
                [assembly: global::DouglasDwyer.Imp.ShareAs(typeof({CLASS_NAME}), typeof({FULL_INTERFACE_NAME}))]
                public partial class {CLASS_NAME} : {INTERFACE_NAME} {

                }

                public interface {INTERFACE_NAME} {BASE_TYPE_NAME} {
                    {INTERFACE_MEMBERS}
                }
            ";
        const string remoteGlobalTemplate = @"
                /*[global::DouglasDwyer.Imp.ProxyFor(typeof({FULL_INTERFACE_NAME}))]
                public class {REMOTE_CLASS_NAME} : global::DouglasDwyer.Imp.RemoteSharedObject, {FULL_INTERFACE_NAME} {

                    public {REMOTE_CLASS_NAME}(global::DouglasDwyer.Imp.SharedObjectPath location, global::DouglasDwyer.Imp.ImpClient host) : base(location, host) {}                        

                    {CLASS_MEMBERS}
                }*/
            ";

        private string GenerateProxyTypes(SharedType sharedClass, List<ProxyType> proxyInterfaces, List<SharedType> sharedClasses, List<string> usings)
        {
            
            string toReturn = sharedClass.InterfaceNamespace == "" ? proxyGlobalTemplate : proxyNamespaceTemplate.Replace("{NAMESPACE}", sharedClass.InterfaceNamespace);
            string interfaceMembers = "";
            string classMembers = "";
            foreach (ISymbol memberSymbol in sharedClass.TypeSymbol.GetMembers())
            {
                if(memberSymbol is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
                {
                    string parameters = string.Concat(method.Parameters.Select(x => GetDisplayString(x.Type, sharedClasses, proxyInterfaces, usings) + " " + x.Name + ","));
                    string typelessParameters = string.Concat(method.Parameters.Select(x => x.Name + ","));
                    if (parameters.Length > 0)
                    {
                        parameters = parameters.Remove(parameters.Length - 1);
                        typelessParameters = typelessParameters.Remove(typelessParameters.Length - 1);
                    }

                    interfaceMembers +=
                        method.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                        GetDisplayString(method.ReturnType, sharedClasses, proxyInterfaces, usings) + " " +
                        method.Name + "(" + parameters + ");";

                    classMembers +=
                        method.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                        GetDisplayString(method.ReturnType, sharedClasses, proxyInterfaces, usings) + " " +
                        method.Name + "(" + parameters + ") => HostClient.CallRemoteMethod<" + (method.ReturnType.SpecialType == SpecialType.System_Void ? "object" : GetDisplayString(method.ReturnType, sharedClasses, proxyInterfaces, usings)) + ">(Location, new object[] { " + typelessParameters + "});";
                }
                else if(memberSymbol is IPropertySymbol property)
                {
                    if (property.IsIndexer)
                    {
                        string parameters = string.Concat(property.Parameters.Select(x => GetDisplayString(x.Type, sharedClasses, proxyInterfaces, usings) + " " + x.Name + ","));
                        string typelessParameters = string.Concat(property.Parameters.Select(x => x.Name + ","));
                        if (parameters.Length > 0)
                        {
                            parameters = parameters.Remove(parameters.Length - 1);
                            typelessParameters = typelessParameters.Remove(typelessParameters.Length - 1);
                        }

                        interfaceMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " this[" +
                            parameters + "] {" + (property.GetMethod is null ? "" : "get; ") + (property.SetMethod is null ? "" : "set; ") + " }";

                        classMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " this[" +
                            parameters + "] {" + (property.GetMethod is null ? "" : "get => HostClient.GetRemoteIndexer<" + GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + ">(Location, new object[] { " + typelessParameters + " });") + (property.SetMethod is null ? "" : "set => HostClient.SetRemoteIndexer<" + GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + ">(Location, value, new object[] { " + typelessParameters + " });") + " }";
                    }
                    else
                    {
                        interfaceMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " " +
                            property.Name + " { " + (property.GetMethod is null ? "" : "get; ") + (property.SetMethod is null ? "" : "set; ") + " }";

                        classMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " " +
                            property.Name + " { " + (property.GetMethod is null ? "" : "get => HostClient.GetRemoteProperty<" + GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + ">(Location);") + (property.SetMethod is null ? "" : "set => HostClient.SetRemoteProperty(Location, value); ") + " }";
                    }
                }
            }
            (string, List<ISymbol>) interfaceDeclarationsAndMembers = GetBaseSharedInterfacesAndMembersForType(sharedClass.TypeSymbol, proxyInterfaces, sharedClasses, usings);

            foreach (ISymbol memberSymbol in interfaceDeclarationsAndMembers.Item2)
            {
                if (memberSymbol is IMethodSymbol method && method.MethodKind == MethodKind.Ordinary)
                {
                    string parameters = string.Concat(method.Parameters.Select(x => GetDisplayString(x.Type, sharedClasses, proxyInterfaces, usings) + " " + x.Name + ","));
                    string typelessParameters = string.Concat(method.Parameters.Select(x => x.Name + ","));
                    if (parameters.Length > 0)
                    {
                        parameters = parameters.Remove(parameters.Length - 1);
                        typelessParameters = typelessParameters.Remove(typelessParameters.Length - 1);
                    }

                    classMembers +=
                        method.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                        GetDisplayString(method.ReturnType, sharedClasses, proxyInterfaces, usings) + " " +
                        method.Name + "(" + parameters + ") => HostClient.CallRemoteMethod<" + (method.ReturnType.SpecialType == SpecialType.System_Void ? "object" : GetDisplayString(method.ReturnType, sharedClasses, proxyInterfaces, usings)) + ">(Location, new object[] { " + typelessParameters + "});";
                }
                else if (memberSymbol is IPropertySymbol property)
                {
                    if (property.IsIndexer)
                    {
                        string parameters = string.Concat(property.Parameters.Select(x => GetDisplayString(x.Type, sharedClasses, proxyInterfaces, usings) + " " + x.Name + ","));
                        string typelessParameters = string.Concat(property.Parameters.Select(x => x.Name + ","));
                        if (parameters.Length > 0)
                        {
                            parameters = parameters.Remove(parameters.Length - 1);
                            typelessParameters = typelessParameters.Remove(typelessParameters.Length - 1);
                        }

                        interfaceMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " this[" +
                            parameters + "] {" + (property.GetMethod is null ? "" : "get; ") + (property.SetMethod is null ? "" : "set; ") + " }";

                        classMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " this[" +
                            parameters + "] {" + (property.GetMethod is null ? "" : "get => HostClient.GetRemoteIndexer<" + GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + ">(Location, new object[] { " + typelessParameters + " });") + (property.SetMethod is null ? "" : "set => HostClient.SetRemoteIndexer<" + GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + ">(Location, value, new object[] { " + typelessParameters + " });") + " }";
                    }
                    else
                    {
                        interfaceMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " " +
                            property.Name + " { " + (property.GetMethod is null ? "" : "get; ") + (property.SetMethod is null ? "" : "set; ") + " }";

                        classMembers +=
                            property.DeclaredAccessibility.ToString().ToLowerInvariant() + " " +
                            GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + " " +
                            property.Name + " { " + (property.GetMethod is null ? "" : "get => HostClient.GetRemoteProperty<" + GetDisplayString(property.Type, sharedClasses, proxyInterfaces, usings) + ">(Location);") + (property.SetMethod is null ? "" : "set => HostClient.SetRemoteProperty(Location, value); ") + " }";
                    }
                }
            }

            return (toReturn + (sharedClass.RemoteNamespace == "" ? remoteGlobalTemplate : remoteNamespaceTemplate))
                .Replace("{INTERFACE_NAME}", sharedClass.InterfaceName)
                .Replace("{FULL_INTERFACE_NAME}", sharedClass.FullInterfaceName)
                .Replace("{BASE_TYPE_NAME}", interfaceDeclarationsAndMembers.Item1)
                .Replace("{INTERFACE_MEMBERS}", interfaceMembers)
                .Replace("{CLASS_MEMBERS}", classMembers)
                .Replace("{CLASS_NAME}", sharedClass.TypeSymbol.Name)
                .Replace("{FULL_CLASS_NAME}", sharedClass.TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .Replace("{REMOTE_CLASS_NAME}", sharedClass.RemoteName)
                .Replace("{REMOTE_NAMESPACE}", sharedClass.RemoteNamespace);
        }

        private string GetDisplayString(ITypeSymbol type, List<SharedType> sharedClasses, List<ProxyType> proxyTypes, List<string> usings)
        {
            if(type.TypeKind == TypeKind.Error)
            {
                string typeDisplayName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                foreach(SharedType shared in sharedClasses)
                {
                    if(shared.InterfaceName == typeDisplayName && (string.IsNullOrEmpty(shared.InterfaceNamespace) || usings.Any(x => x == shared.InterfaceNamespace)))
                    {
                        return shared.FullInterfaceName;
                    }
                    else if (shared.RemoteName == typeDisplayName && (string.IsNullOrEmpty(shared.RemoteNamespace) || usings.Any(x => x == shared.RemoteNamespace)))
                    {
                        return shared.FullRemoteName;
                    }
                }
                return typeDisplayName;
            }
            else
            {
                return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }

        private (string, List<ISymbol>) GetBaseSharedInterfacesAndMembersForType(ITypeSymbol symbol, List<ProxyType> proxyTypes, List<SharedType> sharedTypes, List<string> usings)
        {
            string toReturn = ":";
            List<ISymbol> memberSymbols = new List<ISymbol>();
            if(symbol.BaseType.TypeKind == TypeKind.Class)
            {
                SharedType baseSharedType = sharedTypes.FirstOrDefault(x => x.TypeSymbol.Equals(symbol.BaseType));
                if(baseSharedType != null)
                {
                    toReturn += baseSharedType.FullInterfaceName + ",";
                    memberSymbols.AddRange(baseSharedType.TypeSymbol.GetMembers());
                }
            }
            foreach(ITypeSymbol candidateInterface in symbol.AllInterfaces)
            {
                if(proxyTypes.Any(x => x.ProxyInterface.Equals(candidateInterface)))
                {
                    toReturn += GetDisplayString(candidateInterface, sharedTypes, proxyTypes, usings) + ",";
                    memberSymbols.AddRange(candidateInterface.GetMembers());
                }
            }
            if(toReturn == ":")
            {
                return ("", memberSymbols);
            }
            else
            {
                return (toReturn.Remove(toReturn.Length - 1), memberSymbols);
            }
        }
    }
}
