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
            var elementNode = Node.CreateExampleElementNode(elementTextNode, Summary);
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
                string identName = string.Empty;
                if (baseClasses2 != null)
                {
                    foreach (var baseClass in baseClasses2)
                    {
                        var identifierName = string.Empty;
                        var firstBaseNode = baseClass.ChildNodes().First();
                        var bClass = _model.GetSymbolInfo(firstBaseNode);
                        var bClassSymbol = bClass.Symbol;
                        if(bClassSymbol == null)
                        {
                            continue;
                        }
                        var idName = bClass.Symbol.Name.ToString();
                        var bClassNamespace = bClass.Symbol.ContainingNamespace.ToString();
                        if (bClassNamespace.Equals("<global namespace>"))
                        {
                            identifierName = idName;
                        }
                        else
                        {
                            identifierName = string.Format("{0}.{1}", bClassNamespace, idName);
                        }

                        var nullElement = Node.CreateXmlNullKeywordElement(SeeAlso, identifierName);
                        var baseNode = new DocumentationNode(nullElement, DocumentationCommentDelimiter);
                        baseNodes.Add(baseNode);
                    }
                }
            }
            return baseNodes;
        }

        private static string SeeAlso { get => "seealso";}
        private static string TypeClassNameMatchString { get => @"([A-Za-z0-9_\.]+)"; }
    }

}
