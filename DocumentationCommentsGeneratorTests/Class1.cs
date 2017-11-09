using DocumentationCommentsGenerator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace GenerateDocumentationCommentsTests
{
    public class GenerateDocumentationClassDeclarationTests
    {
        [Fact]
        public void ShouldAddSummaryDocCommentsToIndented4SpacesPublicClassDeclaration()
        {
            var classDecl =
                @"    public class Class1
    {
    }";
            var expected =
                @"    /// <summary>
    /// 
    /// </summary>
    public class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }
    }
}
