using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationCommentsGenerator
{
    internal static class StartAndEndTags
    {
        internal static string GetStartTagName(SyntaxNode textNode)
        {
            var xmlName = textNode.ChildNodes()
                .OfType<XmlNameSyntax>()
                .FirstOrDefault();
            if (xmlName != null)
            {
                return xmlName.GetText().ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
