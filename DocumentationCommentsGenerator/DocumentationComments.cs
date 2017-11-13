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
                var newNodes = CreateNewSummaryNodes(nodeToDocument);
                AddDocumentationNodesToNodeList(newNodes);
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
            var xmlTriviaList = GetDocumentationTriviaList(nodeToDocument);
            foreach(var xmlTrivia in xmlTriviaList)
            {
                var elementTriviaList = xmlTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>();
                foreach (var elementTrivia in elementTriviaList)
                {
                    triviaNodes.Add(new DocumentationNode(elementTrivia, _documentationCommentDelimiter));
                }
            }

            var summaryNode = GetFirstSummaryNode();
            if (summaryNode == null)
            {
                var generatedSummaryNodes = CreateNewSummaryNodes(nodeToDocument);
                var i = 0;
                foreach(var node in generatedSummaryNodes)
                {
                    triviaNodes.Insert(i, node);
                }
            }
            AddDocumentationNodesToNodeList(triviaNodes);
        }

        private DocumentationNode GetFirstSummaryNode()
        {
            foreach (var triviaNode in triviaNodes)
            {
                if (triviaNode.DocumentationTagName.Equals(summary))
                {
                    return triviaNode;
                }
            }
            return null;
        }

        private static IEnumerable<DocumentationCommentTriviaSyntax> GetDocumentationTriviaList(SyntaxNode nodeToDocument)
        {
            return nodeToDocument.GetLeadingTrivia().Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>();
        }

        private IEnumerable<DocumentationNode> CreateNewSummaryNodes(SyntaxNode nodeToDocument)
        {
            var firstNewlineToken = Token.CreateXmlTextNewLine();
            var firstPartSummaryComment = Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var secondNewlineToken = Token.CreateXmlTextNewLine();
            var secondPartSummaryComment = Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var elementTextNode = Node.CreateXmlText(_documentationCommentDelimiter,
                new SyntaxToken[] { firstNewlineToken, firstPartSummaryComment, secondNewlineToken,
                secondPartSummaryComment});
            var elementNode = Node.CreateExampleElementNode(elementTextNode, summary);
            var docNode = new DocumentationNode(elementNode, _documentationCommentDelimiter);
            return new[] { docNode };
        }

        private void AddDocumentationNodesToNodeList(IEnumerable<DocumentationNode> documentationNodes)
        {
            foreach(var docNode in documentationNodes)
            {
                _nodes = _nodes.Add(docNode.ElementNode);
            }
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
        private static readonly string summary = "summary";
        private SyntaxList<XmlNodeSyntax> _nodes = SyntaxFactory.List<XmlNodeSyntax>();
        private List<DocumentationNode> triviaNodes = new List<DocumentationNode>();
        private SyntaxTrivia _lastLeadingTrivia;
        private string _documentationCommentDelimiter;
    }
}
