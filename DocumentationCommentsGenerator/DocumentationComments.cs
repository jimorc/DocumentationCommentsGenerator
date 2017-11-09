using System;
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

            var firstTextToken = CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var firstTextNode = CreateXmlText(_documentationCommentDelimiter, firstTextToken);
            _nodes = _nodes.Add(firstTextNode);

            GenerateCommentsForNode(nodeToDocument);

            var lastNewlineToken = CreateXmlTextNewLine();
            var lastTextNode = CreateXmlText(_documentationCommentDelimiter, lastNewlineToken);
            _nodes = _nodes.Add(lastTextNode);

            var indentLiteralToken = CreateXmlTextLiteral(_lastLeadingTrivia.ToFullString(), noSpace);
            var indentNode = CreateXmlText(noSpace, indentLiteralToken);
            _nodes = _nodes.Add(indentNode);
        }

        private void GenerateCommentsForNode(SyntaxNode nodeToDocument)
        {
            var firstNewlineToken = CreateXmlTextNewLine();
            var firstPartSummaryComment = CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var secondNewlineToken = CreateXmlTextNewLine();
            var secondPartSummaryComment = CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var elementTextNode = CreateXmlText(_documentationCommentDelimiter,
                new SyntaxToken[] { firstNewlineToken, firstPartSummaryComment, secondNewlineToken,
                secondPartSummaryComment});
            var summaryExampleElementNode = CreateExampleElementNode(elementTextNode);
            _nodes = _nodes.Add(summaryExampleElementNode);
        }

        internal SyntaxTrivia CreateDocumentationCommentsTrivia()
        {
            return SyntaxFactory.Trivia(
                SyntaxFactory.DocumentationCommentTrivia(
                    SyntaxKind.SingleLineDocumentationCommentTrivia,
                    _nodes));
        }

        private static XmlElementSyntax CreateExampleElementNode(XmlNodeSyntax textNode)
        {
            string summary = "summary";
            return SyntaxFactory.XmlExampleElement(
                SyntaxFactory.SingletonList<XmlNodeSyntax>(
                    textNode))
                    .WithStartTag(
                SyntaxFactory.XmlElementStartTag(
                    SyntaxFactory.XmlName(
                        SyntaxFactory.Identifier(summary))))
                        .WithEndTag(
                SyntaxFactory.XmlElementEndTag(
                    SyntaxFactory.XmlName(
                        SyntaxFactory.Identifier(summary))));
        }

        private static SyntaxToken CreateXmlTextLiteral(string textForTextLiteral, string delimiter)
        {
            return SyntaxFactory.XmlTextLiteral(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.DocumentationCommentExterior(delimiter)),
                textForTextLiteral,
                textForTextLiteral,
                SyntaxFactory.TriviaList());
        }

        private static SyntaxToken CreateXmlTextNewLine()
        {
            return SyntaxFactory.XmlTextNewLine(
                SyntaxFactory.TriviaList(),
                Environment.NewLine,
                Environment.NewLine,
                SyntaxFactory.TriviaList());
        }

        private static XmlNodeSyntax CreateXmlText(string commentDelimiter, SyntaxToken token)
        {
            return CreateXmlText(commentDelimiter, new SyntaxToken[] { token });
        }

        private static XmlNodeSyntax CreateXmlText(string commentDelimiter, SyntaxToken[] tokens)
        {
            return SyntaxFactory.XmlText()
                .WithTextTokens(SyntaxFactory.TokenList(tokens));
        }

        private const string _commentDelimiter = "///";
        private const string noSpace = "";
        private const string singleSpace = " ";
        private SyntaxList<XmlNodeSyntax> _nodes = SyntaxFactory.List<XmlNodeSyntax>();
        private SyntaxTrivia _lastLeadingTrivia;
        private string _documentationCommentDelimiter;
    }
}
