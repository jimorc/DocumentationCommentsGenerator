using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationCommentsGenerator
{
    internal class DocumentationComments
    {
        internal DocumentationComments(SyntaxNode nodeToDocument)
        {
            _lastLeadingTrivia = nodeToDocument.GetLeadingTrivia().LastOrDefault();
            _documentationCommentDelimiter = _lastLeadingTrivia.ToFullString() + _commentDelimiter;

            var firstTextToken = Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var firstTextNode = Node.CreateXmlText(_documentationCommentDelimiter, firstTextToken);
            _nodes = _nodes.Add(firstTextNode);

            if (NodeContainsDocumentationComments(nodeToDocument))
            {
                GenerateCommentsForNodeWithDocumentationComments(nodeToDocument);
            }
            else
            {
                GenerateCommentsForNode(nodeToDocument);
            }

            var lastNewlineToken = Token.CreateXmlTextNewLine();
            var lastTextNode = Node.CreateXmlText(_documentationCommentDelimiter, lastNewlineToken);
            _nodes = _nodes.Add(lastTextNode);

            var indentLiteralToken = Token.CreateXmlTextLiteral(_lastLeadingTrivia.ToFullString(), noSpace);
            var indentNode = Node.CreateXmlText(noSpace, indentLiteralToken);
            _nodes = _nodes.Add(indentNode);
        }

        private static bool NodeContainsDocumentationComments(SyntaxNode nodeToDocument)
        {
            var xmlTriviaList = GetDocumentationTriviaList(nodeToDocument);
            return xmlTriviaList != null && xmlTriviaList.Count() > 0;
        }

        private void GenerateCommentsForNodeWithDocumentationComments(SyntaxNode nodeToDocument)
        {
            XmlElementSyntax summaryElement = GetFirstSummaryElement(nodeToDocument);
            if(summaryElement != null)
            {
                var summaryDocNode = new DocumentationNode(summaryElement, _documentationCommentDelimiter);
                _nodes = _nodes.Add(summaryDocNode.ElementNode);
            }
            else
            {
                GenerateCommentsForNode(nodeToDocument);
            }
        }

        private static XmlElementSyntax GetFirstSummaryElement(SyntaxNode nodeToParse)
        {
            XmlElementSyntax summaryElement = null;
            var xmlTriviaList = GetDocumentationTriviaList(nodeToParse);
            foreach (var xmlTrivia in xmlTriviaList)
            {
                var elementTriviaList = xmlTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>();
                summaryElement = elementTriviaList
                    .Where(t => t.StartTag.Name.ToString().Equals("summary"))
                    .FirstOrDefault();
                if (summaryElement != null)
                {
                    return summaryElement;
                }
            }
            return null;
        }

        private static IEnumerable<DocumentationCommentTriviaSyntax> GetDocumentationTriviaList(SyntaxNode nodeToDocument)
        {
            return nodeToDocument.GetLeadingTrivia().Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>();
        }

        private void GenerateCommentsForNode(SyntaxNode nodeToDocument)
        {
            var firstNewlineToken = Token.CreateXmlTextNewLine();
            var firstPartSummaryComment = Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var secondNewlineToken = Token.CreateXmlTextNewLine();
            var secondPartSummaryComment = Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var elementTextNode = Node.CreateXmlText(_documentationCommentDelimiter,
                new SyntaxToken[] { firstNewlineToken, firstPartSummaryComment, secondNewlineToken,
                secondPartSummaryComment});
            var summaryExampleElementNode = Node.CreateExampleElementNode(elementTextNode);
            _nodes = _nodes.Add(summaryExampleElementNode);
        }

        internal SyntaxTrivia CreateDocumentationCommentsTrivia()
        {
            return SyntaxFactory.Trivia(
                SyntaxFactory.DocumentationCommentTrivia(
                    SyntaxKind.SingleLineDocumentationCommentTrivia,
                    _nodes));
        }

        private const string _commentDelimiter = "///";
        private static readonly string noSpace = "";
        private static readonly string singleSpace = " ";
        private SyntaxList<XmlNodeSyntax> _nodes = SyntaxFactory.List<XmlNodeSyntax>();
        private SyntaxTrivia _lastLeadingTrivia;
        private string _documentationCommentDelimiter;
    }
}
