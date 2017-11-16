using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationCommentsGenerator
{
    internal class ClassDeclarationDocumentationComments : DocumentationComments
    {
        internal ClassDeclarationDocumentationComments(SyntaxNode nodeToDocument)
            : base(nodeToDocument)
        { }

        protected override IEnumerable<DocumentationNode> CreateNewSummaryNodes(SyntaxNode nodeToDocument)
        {
            var docNodes = new List<DocumentationNode>();
            var firstNewlineToken = Token.CreateXmlTextNewLine();
            var firstPartSummaryComment = Token.CreateXmlTextLiteral(SingleSpace, DocumentationCommentDelimiter);
            var secondNewlineToken = Token.CreateXmlTextNewLine();
            var secondPartSummaryComment = Token.CreateXmlTextLiteral(SingleSpace, DocumentationCommentDelimiter);
            var elementTextNode = Node.CreateXmlText(DocumentationCommentDelimiter,
                new SyntaxToken[] { firstNewlineToken, firstPartSummaryComment, secondNewlineToken,
                    secondPartSummaryComment});
            var elementNode = Node.CreateExampleElementNode(elementTextNode, summary);
            var docNode = new DocumentationNode(elementNode, DocumentationCommentDelimiter);
            docNodes.Add(docNode);
            var baseNodes = GetClassDeclarationBaseClasses(nodeToDocument);
            foreach (var node in baseNodes)
            {
                docNodes.Add(node);
            }
            return docNodes;
        }

        protected IEnumerable<DocumentationNode> GetClassDeclarationBaseClasses(SyntaxNode nodeToDocument)
        {
            var baseNodes = new List<DocumentationNode>();
            var baseClasses = nodeToDocument.ChildNodes()
                .OfType<BaseListSyntax>().FirstOrDefault();
            if (baseClasses != null)
            {
                var baseClasses2 = baseClasses.ChildNodes()
                    .OfType<SimpleBaseTypeSyntax>();
                string identifierName = string.Empty;
                if (baseClasses2 != null)
                {
                    foreach (var baseClass in baseClasses2)
                    {
                        var firstBaseNode = baseClass.ChildNodes().First();
                        switch (firstBaseNode.Kind())
                        {
                            case SyntaxKind.IdentifierName:
                            case SyntaxKind.GenericName:
                                var idName = firstBaseNode.ToString();
                                identifierName = GetFullyQualifiedClassName(firstBaseNode, nodeToDocument);
                                break;
                            case SyntaxKind.SimpleBaseType:
                                identifierName = baseClass.ChildNodes()
                                    .OfType<IdentifierNameSyntax>()
                                    .First()
                                    .ToString();
                                break;
                        }
                        var nullElement = Node.CreateXmlNullKeywordElement(SeeAlso, identifierName);
                        var baseNode = new DocumentationNode(nullElement, DocumentationCommentDelimiter);
                        baseNodes.Add(baseNode);
                    }
                }
            }
            return baseNodes;
        }

        private static string GetFullyQualifiedClassName(SyntaxNode classNameNode, SyntaxNode nodeToDocument)
        {
            var typeClassName = GetTypeClassName(classNameNode);
            var syntaxTree = nodeToDocument.SyntaxTree;
            var root = syntaxTree.GetRoot();
            var usingNodes = root.ChildNodes()
                .OfType<UsingDirectiveSyntax>();
            foreach (var usingNode in usingNodes)
            {
                var usingIdentifier = usingNode.ChildNodes()
                    .Where(u => u.IsKind(SyntaxKind.IdentifierName)
                        || u.IsKind(SyntaxKind.QualifiedName))
                    .FirstOrDefault();
                if (usingIdentifier != null)
                {
                    var possibleFullyQualifiedClassName = string.Format(
                        "{0}.{1}", usingIdentifier.ToString(), typeClassName);
                    if (Type.GetType(possibleFullyQualifiedClassName) != null)
                    {
                        return possibleFullyQualifiedClassName;
                    }
                }
            }
            return typeClassName;
        }

        private static string GetTypeClassName(SyntaxNode classNameNode)
        {
            var className = classNameNode.ToString();
            var baseClassNames = classNameNode.ChildNodes();
            var genericArgumentCount = baseClassNames.Count();
            if (genericArgumentCount > 0)
            {
                Match match = Regex.Match(className, TypeClassNameMatchString);
                className = match.Value + "`" + genericArgumentCount.ToString();
            }
            return className;
        }

        private static string SeeAlso { get => "seealso";}
        private static string TypeClassNameMatchString { get => @"([A-Za-z0-9_\.]+)"; }
    }

}
