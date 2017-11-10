using System;
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

            var firstTextToken = CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var firstTextNode = CreateXmlText(_documentationCommentDelimiter, firstTextToken);
            _nodes = _nodes.Add(firstTextNode);

            if (NodeContainsDocumentationComments(nodeToDocument))
            {
                GenerateCommentsForNodeWithDocumentationComments(nodeToDocument);
            }
            else
            {
                GenerateCommentsForNode(nodeToDocument);
            }

            var lastNewlineToken = CreateXmlTextNewLine();
            var lastTextNode = CreateXmlText(_documentationCommentDelimiter, lastNewlineToken);
            _nodes = _nodes.Add(lastTextNode);

            var indentLiteralToken = CreateXmlTextLiteral(_lastLeadingTrivia.ToFullString(), noSpace);
            var indentNode = CreateXmlText(noSpace, indentLiteralToken);
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
                CreateExampleElementNodeFromCommentElementSyntax(summaryElement);
            }
            else
            {
                GenerateCommentsForNode(nodeToDocument);
            }
        }

        private void CreateExampleElementNodeFromCommentElementSyntax(XmlElementSyntax element)
        {
            XmlNodeSyntax tNode = null;
            foreach (var textNode in element.ChildNodes())
            {
                switch (textNode.Kind())
                {
                    case SyntaxKind.XmlElementStartTag:
                        break;
                    case SyntaxKind.XmlElementEndTag:
                        break;
                    case SyntaxKind.XmlText:
                        tNode = GetTextNodeFromCommentTextNode(textNode);
                        break;
                    default:
                        break;
                }
            }
            if (tNode != null)
            {
                _nodes = _nodes.Add(CreateExampleElementNode(tNode));
            }
        }

        private XmlNodeSyntax GetTextNodeFromCommentTextNode(SyntaxNode textNode)
        {
            var tokens = new List<SyntaxToken>();
            foreach (var token in textNode.ChildTokens())
            {
                switch (token.Kind())
                {
                    case SyntaxKind.XmlTextLiteralNewLineToken:
                        tokens.Add(CreateXmlTextNewLine());
                        break;
                    case SyntaxKind.XmlTextLiteralToken:
                        var text = token.ValueText.ToString();
                        var delimiter = noSpace;
                        if(tokens.Count() !=0 && tokens.Last().IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                        {
                            delimiter = _documentationCommentDelimiter;
                        }
                        tokens.Add(CreateXmlTextLiteral(text, delimiter));
                        break;
                    default:
                        break;
                }
            }
            return CreateXmlText(_documentationCommentDelimiter, tokens.ToArray());

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
