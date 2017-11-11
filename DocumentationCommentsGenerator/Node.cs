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

        internal static XmlElementSyntax CreateExampleElementNode(XmlNodeSyntax textNode)
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
    }
}
