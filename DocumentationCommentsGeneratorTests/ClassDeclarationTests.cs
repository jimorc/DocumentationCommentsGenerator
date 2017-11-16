using DocumentationCommentsGenerator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace GenerateDocumentationCommentsTests
{
    public class DocumentationCommentsGenerationClassDeclarationTests
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

        [Fact]
        public void ShouldNotAddSummaryDocCommentsClassDeclarationWithSummaryComments()
        {
            var classDecl =
    @"        /// <summary>
        /// A summary description
        /// </summary>
        public class Class1
        {
        }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(classDecl, result.ToFullString());
        }

        [Fact]
        public void ShouldAddSummaryDocCommentsToIndented4SpacesProtectedClassDeclaration()
        {
            var classDecl =
@"    protected class Class1
    {
    }";
            var expected =
@"    /// <summary>
    /// 
    /// </summary>
    protected class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }
        [Fact]
        public void ShouldAddSummaryDocCommentsToInternalClassDeclaration()
        {
            var classDecl =
@"    internal class Class1
    {
    }";
            var expected =
@"    /// <summary>
    /// 
    /// </summary>
    internal class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldNotAddSummaryDocCommentsToPrivateClassDeclaration()
        {
            var classDecl =
@"    private class Class1
    {
    }";
            var expected =
@"    private class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldNotAddSummaryDocCommentsToNoAccessClassDeclaration()
        {
            var classDecl =
@"    class Class1
    {
    }";
            var expected =
    @"    class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldNotAddSummaryDocCommentsClassDeclarationWithMultilineSummaryComments()
        {
            var classDecl =
@"        /** <summary>
         * A summary description
         * </summary> 
         **/
        public class Class1
        {
        }";
            var expected =
@"        /// <summary>
        /// A summary description
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

        [Fact]
        public void ShouldChangeSummaryDocCommentsToBeginAtClassDeclarationColumn()
        {
            var classDecl =
@"    /// <summary>
    /// 
    /// </summary>
  internal class Class1
  {
  }";
            var expected =
@"  /// <summary>
  /// 
  /// </summary>
  internal class Class1
  {
  }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldRetainOneLineSummaryDocumentationComments()
        {
            var classDecl =
@"    /// <summary>A summary comment.</summary>
    internal class Class1
    {
    }";
            var expected =
@"    /// <summary>A summary comment.</summary>
    internal class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldRetainExistingCommentsWhenRetainingSummaryComments()
        {
            var classDecl =
@"    /// <remarks>A remarks comment.</remarks>
    /// <summary>A summary comment.</summary>
    /// <remarks>More remarks.</remarks>
    internal class Class1
    {
    }";
            var expected =
@"    /// <remarks>A remarks comment.</remarks>
    /// <summary>A summary comment.</summary>
    /// <remarks>More remarks.</remarks>
    internal class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldRetainExistingCommentsWhenAddingSummaryComments()
        {
            var classDecl =
@"    /// <remarks>A remarks comment.</remarks>
    /// <remarks>More remarks.</remarks>
    internal class Class1
    {
    }";
            var expected =
@"    /// <summary>
    /// 
    /// </summary>
    /// <remarks>A remarks comment.</remarks>
    /// <remarks>More remarks.</remarks>
    internal class Class1
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldAddSummaryAndSeeAlsoCommentsForChildClass()
        {
            var classDecl =
@"    internal class Class1 : Class0
    {
    }";
            var expected =
@"    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref=""Class0""/>
    internal class Class1 : Class0
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldAddSummaryAnd3SeeAlsoCommentsForChildClassesAnd2Interfaces()
        {
            var classDecl =
@"    internal class Class1 : Class0, IClass, IClass2
    {
    }";
            var expected =
@"    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref=""Class0""/>
    /// <seealso cref=""IClass""/>
    /// <seealso cref=""IClass2""/>
    internal class Class1 : Class0, IClass, IClass2
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclSyntax = (ClassDeclarationSyntax)root.Members[0];

            var result = rewriter.VisitClassDeclaration(classDeclSyntax);

            Assert.Equal(expected, result.ToFullString());
        }

        [Fact]
        public void ShouldAddSummaryAndSeeAlsoCommentsForClassWithTemplatedBaseClass()
        {
            var classDecl =
@"using System.Collections.Generic;

    internal class Class1 : IEnumerable<Class0>
    {
    }";
            var expected =
@"using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref=""System.Collections.Generic.IEnumerable`1""/>
    internal class Class1 : IEnumerable<Class0>
    {
    }";
            var tree = CSharpSyntaxTree.ParseText(classDecl);
            var rewriter = new DocumentCommentsRewriter();
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var result = rewriter.Visit(root);

            Assert.Equal(expected, result.ToFullString());
        }
    }
}
