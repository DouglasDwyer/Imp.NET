using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DouglasDwyer.KnetworkingGenerator
{
    public class ProxyDataReceiver : ISyntaxReceiver
    {
        public List<CompilationUnitSyntax> CandidateMakeSharedAttributes = new List<CompilationUnitSyntax>();
        public List<ClassDeclarationSyntax> CandidateRemoteTypes = new List<ClassDeclarationSyntax>();
        public List<ClassDeclarationSyntax> CandidateClasses = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode is ClassDeclarationSyntax syntax && syntax.AttributeLists.SelectMany(x => x.Attributes).Count() > 0)
            {
                CandidateClasses.Add(syntax);
            }
            else if(syntaxNode is ClassDeclarationSyntax interfaceSyntax && interfaceSyntax.AttributeLists.SelectMany(x => x.Attributes).Count() > 0)
            {
                CandidateRemoteTypes.Add(interfaceSyntax);
            }
            else if(syntaxNode is CompilationUnitSyntax attributeListSyntax && attributeListSyntax.AttributeLists.Count > 0)
            {
                CandidateMakeSharedAttributes.Add(attributeListSyntax);
            }
        }
    }
}
