using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#pragma warning disable RS1024

namespace DouglasDwyer.ImpGenerator
{
    [Generator]
    public class ProxyGenerator : ISourceGenerator {

        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new SharedClassReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is SharedClassReceiver receiver)
            {
                Compilation currentCompilation = context.Compilation;
                INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("DouglasDwyer.Imp.SharedAttribute");
                List<SharedTypeBuilder> sharedTypes = new List<SharedTypeBuilder>();
                SemanticModel model = null;
                
                foreach (ClassDeclarationSyntax syntax in receiver.CandidateSharedClasses)
                {
                    if (model is null || model.SyntaxTree != syntax.SyntaxTree)
                    {
                        model = context.Compilation.GetSemanticModel(syntax.SyntaxTree);
                    }

                    INamedTypeSymbol type = model.GetDeclaredSymbol(syntax);
                    AttributeData attributeData = type.GetAttributes().FirstOrDefault(x => x.AttributeClass.Equals(attributeSymbol));
                    if (attributeData != null)
                    {
                        SharedTypeBuilder shared;
                        if (attributeData.ConstructorArguments.Count() == 1)
                        {
                            string name = attributeData.ConstructorArguments[0].Value.ToString();
                            if(name.StartsWith(".") || name.EndsWith("."))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(ImpRules.InvalidTypeNameError, type.Locations.FirstOrDefault(), type.Name, name));
                            }
                            string nSpace = "";
                            if(name.Contains("."))
                            {
                                nSpace = name.Remove(name.LastIndexOf("."));
                                name = name.Substring(name.LastIndexOf(".") + 1);
                            }
                            shared = new SharedTypeBuilder(type, model, context, name, nSpace);
                        }
                        else
                        {
                            shared = new SharedTypeBuilder(type, model, context, "I" + type.Name, MemberBuilder.GetFullNamespaceName(type.ContainingNamespace));
                        }
                        sharedTypes.Add(shared);
                        SyntaxTree tree = shared.GenerateInterfaceDefinition();
                        currentCompilation = currentCompilation.AddSyntaxTrees(tree);
                        string s = tree.GetCompilationUnitRoot().ToFullString();
                        context.AddSource("[ImpGenerated]" + Guid.NewGuid().ToString("N"), SourceText.From(s, Encoding.Default));
                    }
                }

                foreach (SharedTypeBuilder shared in sharedTypes)
                {
                    shared.SetModel(currentCompilation.GetSemanticModel(shared.Symbol.DeclaringSyntaxReferences[0].SyntaxTree));
                    SyntaxTree tree = shared.GenerateInterfaceInheritance();
                    currentCompilation = currentCompilation.AddSyntaxTrees(tree);
                    string s = tree.GetCompilationUnitRoot().ToFullString();
                    context.AddSource("[ImpGenerated]" + Guid.NewGuid().ToString("N"), SourceText.From(s, Encoding.Default));
                }

                foreach (SharedTypeBuilder shared in sharedTypes)
                {
                    shared.SetModel(currentCompilation.GetSemanticModel(shared.Symbol.DeclaringSyntaxReferences[0].SyntaxTree));
                    string s = shared.GenerateInterfaceImplementation().GetCompilationUnitRoot().ToFullString();
                    context.AddSource("[ImpGenerated]" + shared.FullInterfaceName.Replace("global::",""), SourceText.From(s, Encoding.Default));
                }
            }
        }

        private class SharedClassReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateSharedClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if(syntaxNode is ClassDeclarationSyntax classDeclaration)
                {
                    CandidateSharedClasses.Add(classDeclaration);
                }
            }
        }
    }
}
