using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DocumentationCommentsGenerator
{
    internal static class Token
    {
        internal static SyntaxToken CreateXmlTextLiteral(string textForTextLiteral, string delimiter)
        {
            return SyntaxFactory.XmlTextLiteral(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.DocumentationCommentExterior(delimiter)),
                textForTextLiteral,
                textForTextLiteral,
                SyntaxFactory.TriviaList());
        }

        internal static SyntaxToken CreateXmlTextNewLine()
        {
            return SyntaxFactory.XmlTextNewLine(
                SyntaxFactory.TriviaList(),
                Environment.NewLine,
                Environment.NewLine,
                SyntaxFactory.TriviaList());
        }
    }
}
