using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationCommentsGenerator
{
    internal class DocumentationNode
    {
        internal DocumentationNode(XmlNodeSyntax documentationElement, string commentDelimiter)
        {
            _documentationCommentDelimiter = commentDelimiter;
            XmlNodeSyntax tNode = null;
            string startTag = string.Empty;
            foreach (var textNode in documentationElement.ChildNodes())
            {
                switch (textNode.Kind())
                {
                    case SyntaxKind.XmlElementStartTag:
                        startTag = StartAndEndTags.GetStartTagName(textNode);
                        break;
                    case SyntaxKind.XmlElementEndTag:
                        break;
                    case SyntaxKind.XmlText:
                        tNode = GetTextNodeFromCommentTextNode(textNode);
                        _elementNode = Node.CreateExampleElementNode(tNode, startTag);
                        break;
                    case SyntaxKind.XmlName:
                        var name = ((XmlNameSyntax)textNode).LocalName.ValueText;
                        _elementNode = Node.CreateXmlNullKeywordElement(name);
                        break;
                    case SyntaxKind.XmlCrefAttribute:
                        var identifier = ((XmlCrefAttributeSyntax)textNode).Cref.ToString();
                        _elementNode = Node.AddCrefIdentifierToNullKeywordElement(_elementNode, identifier);
                        break;
                    default:
                        break;
                }
            }
//            _elementNode = Node.CreateExampleElementNode(tNode, startTag);
        }

        private XmlNodeSyntax GetTextNodeFromCommentTextNode(SyntaxNode textNode)
        {
            var tokens = new List<SyntaxToken>();
            foreach (var token in textNode.ChildTokens())
            {
                switch (token.Kind())
                {
                    case SyntaxKind.XmlTextLiteralNewLineToken:
                        tokens.Add(Token.CreateXmlTextNewLine());
                        break;
                    case SyntaxKind.XmlTextLiteralToken:
                        var text = token.ValueText.ToString();
                        var delimiter = noSpace;
                        if (tokens.Count() != 0 && tokens.Last().IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                        {
                            delimiter = _documentationCommentDelimiter;
                        }
                        tokens.Add(Token.CreateXmlTextLiteral(text, delimiter));
                        break;
                    default:
                        break;
                }
            }
            return Node.CreateXmlText(_documentationCommentDelimiter, tokens.ToArray());
        }

        internal string DocumentationTagName
        {
            get
            {
                if (_elementNode.IsKind(SyntaxKind.XmlElement))
                {
                    return ((XmlElementSyntax)_elementNode).StartTag.Name.ToString();
                }
                else
                {
                    return noSpace;
                }
            }
        }

        internal XmlNodeSyntax ElementNode
        {
            get { return _elementNode; }
        }

        private static readonly string noSpace = "";
        private string _documentationCommentDelimiter;
        private XmlNodeSyntax _elementNode = null;
    }
}
