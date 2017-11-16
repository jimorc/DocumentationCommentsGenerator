using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[assembly: InternalsVisibleTo("DocumentationCommentsGeneratorTests,PublicKey=" +
    "00240000048000009400000006020000002400005253413100040000010001001198cda887ded4" +
"f73bca244098ab06ac38920b46a7b95bda542beae80dcb7f4f75aeb944a57ec8078fc74a8778d9" +
"a629d221eb1dd454311aa9554f2717e2155c5cb75a2c43c1ce70915cdb9e0e02ca5ab7c290d78d" +
"3bb9e88a023115be50e9d4663cfb065264d3187c1afe260aa033cf4e5cd6fc5392589823f39d03" +
"b7b7d4d3")]

namespace DocumentationCommentsGenerator
{
    internal class DocumentCommentsRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var accessModifier = node.Modifiers
                .Where(m => m.IsKind(SyntaxKind.PublicKeyword)
                    || m.IsKind(SyntaxKind.ProtectedKeyword)
                    || m.IsKind(SyntaxKind.InternalKeyword))
                .FirstOrDefault();
            if (!accessModifier.IsKind(SyntaxKind.None))
            {
                var docComments = new ClassDeclarationDocumentationComments(node);
                var leadingTrivia = docComments.CreateDocumentationCommentsTrivia();
                node = node.WithLeadingTrivia(leadingTrivia);
            }
            return base.VisitClassDeclaration(node);
        }
    }
}
