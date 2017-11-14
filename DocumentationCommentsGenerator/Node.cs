using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationCommentsGenerator
{
    internal static class Node
    {
        internal static XmlNodeSyntax CreateXmlText(string commentDelimiter, SyntaxToken token)
        {
            return CreateXmlText(commentDelimiter, new SyntaxToken[] { token });
        }

        internal static XmlNodeSyntax CreateXmlText(string commentDelimiter, SyntaxToken[] tokens)
        {
            return SyntaxFactory.XmlText()
                .WithTextTokens(SyntaxFactory.TokenList(tokens));
        }

        internal static XmlNodeSyntax CreateExampleElementNode(XmlNodeSyntax textNode, string tagName)
        {
            return SyntaxFactory.XmlExampleElement(
                SyntaxFactory.SingletonList<XmlNodeSyntax>(
                    textNode))
                    .WithStartTag(
                SyntaxFactory.XmlElementStartTag(
                    SyntaxFactory.XmlName(
                        SyntaxFactory.Identifier(tagName))))
                        .WithEndTag(
                SyntaxFactory.XmlElementEndTag(
                    SyntaxFactory.XmlName(
                        SyntaxFactory.Identifier(tagName))));
        }

        internal static XmlNodeSyntax CreateXmlNullKeywordElement(string nodeName)
        {
            return SyntaxFactory.XmlNullKeywordElement()
                .WithName(
                    SyntaxFactory.XmlName(
                        SyntaxFactory.Identifier(nodeName)));
        }

        internal static XmlNodeSyntax CreateXmlNullKeywordElement(string nodeName, string identifier)
        {
            var nullNode = CreateXmlNullKeywordElement(nodeName);
            return AddCrefIdentifierToNullKeywordElement(nullNode, identifier);
        }

        internal static XmlNodeSyntax AddCrefIdentifierToNullKeywordElement(XmlNodeSyntax node, string identifier)
        {
            if (node.IsKind(SyntaxKind.XmlEmptyElement))
            {
               node = ((XmlEmptyElementSyntax)(node)).WithAttributes(
                        SyntaxFactory.SingletonList<XmlAttributeSyntax>(
                            SyntaxFactory.XmlCrefAttribute(
                                SyntaxFactory.NameMemberCref(
                                    SyntaxFactory.IdentifierName(identifier)))));
            }
            return node;
        }
    }
}
