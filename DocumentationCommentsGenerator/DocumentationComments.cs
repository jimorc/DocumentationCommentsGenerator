using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationCommentsGenerator
{
    internal class DocumentationComments
    {
        internal DocumentationComments(SyntaxNode nodeToDocument)
        {
            var leadingTrivia = nodeToDocument.GetLeadingTrivia();
            InsertLeadingNewLines(leadingTrivia);
            _lastLeadingTrivia = nodeToDocument.GetLeadingTrivia().LastOrDefault();
            _documentationCommentDelimiter = _lastLeadingTrivia.ToFullString() + _commentDelimiter;

            if (NodeContainsDocumentationComments(nodeToDocument))
            {
                GenerateCommentsForNodeWithDocumentationComments(nodeToDocument);
            }
            else
            {
                var newNodes = CreateNewSummaryNodes(nodeToDocument);
                AddDocumentationNodesToNodeList(newNodes);
            }

            AddNewLineNodeToNodes(_documentationCommentDelimiter);
            InsertIndentationNode(_lastLeadingTrivia.ToString());
        }

        private void InsertIndentationNode(string indentationText)
        {
            var indentLiteralToken = Token.CreateXmlTextLiteral(indentationText, noSpace);
            var indentNode = Node.CreateXmlText(noSpace, indentLiteralToken);
            _nodes = _nodes.Add(indentNode);
        }

        private void InsertLeadingNewLines(SyntaxTriviaList leadingTrivia)
        {
            foreach (var trivia in leadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    AddNewLineNodeToNodes(noSpace);
                }
                else
                {
                    break;
                }
            }
        }

        private void AddNewLineNodeToNodes(string commentDelimiter)
        {
            var newlineToken = Token.CreateXmlTextNewLine();
            var newLineNode = Node.CreateXmlText(commentDelimiter, newlineToken);
            _nodes = _nodes.Add(newLineNode);
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
            var docNodes = new List<DocumentationNode>();
            var firstNewlineToken = Token.CreateXmlTextNewLine();
            var firstPartSummaryComment = Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var secondNewlineToken = Token.CreateXmlTextNewLine();
            var secondPartSummaryComment = Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter);
            var elementTextNode = Node.CreateXmlText(_documentationCommentDelimiter,
                new SyntaxToken[] { firstNewlineToken, firstPartSummaryComment, secondNewlineToken,
                secondPartSummaryComment});
            var elementNode = Node.CreateExampleElementNode(elementTextNode, summary);
            var docNode = new DocumentationNode(elementNode, _documentationCommentDelimiter);
            docNodes.Add(docNode);
            var baseNodes = GetClassDeclarationBaseClasses(nodeToDocument);
            foreach(var node in baseNodes)
            {
                docNodes.Add(node);
            }
            return docNodes;
        }

        private IEnumerable<DocumentationNode> GetClassDeclarationBaseClasses(SyntaxNode nodeToDocument)
        {
            var baseNodes = new List<DocumentationNode>();
            var baseClasses = nodeToDocument.ChildNodes()
                .OfType<BaseListSyntax>().FirstOrDefault();
            if (baseClasses != null)
            {
                var baseClasses2 = baseClasses.ChildNodes()
                    .OfType<SimpleBaseTypeSyntax>();
                string identifierName = string.Empty;
                if (baseClasses2 != null)
                {
                    foreach(var baseClass in baseClasses2)
                    {
                        var firstBaseNode = baseClass.ChildNodes().First();
                        switch (firstBaseNode.Kind())
                        {
                            case SyntaxKind.IdentifierName:
                            case SyntaxKind.GenericName:
                                var idName = firstBaseNode.ToString();
                                identifierName = GetFullyQualifiedClassName(firstBaseNode, nodeToDocument);
                                break;
                            case SyntaxKind.SimpleBaseType:
                                identifierName = baseClass.ChildNodes()
                                    .OfType<IdentifierNameSyntax>()
                                    .First()
                                    .ToString();
                                break;
                        }
                        var nullElement = Node.CreateXmlNullKeywordElement("seealso", identifierName);
                        var baseNode = new DocumentationNode(nullElement, _documentationCommentDelimiter);
                        baseNodes.Add(baseNode);
                    }
                }
            }
            return baseNodes;
        }

        private static string GetFullyQualifiedClassName(SyntaxNode classNameNode, SyntaxNode nodeToDocument)
        {
            var typeClassName = GetTypeClassName(classNameNode);
            var syntaxTree = nodeToDocument.SyntaxTree;
            var root = syntaxTree.GetRoot();
            var usingNodes = root.ChildNodes()
                .OfType<UsingDirectiveSyntax>();
            foreach(var usingNode in usingNodes)
            {
                var usingIdentifier = usingNode.ChildNodes()
                    .Where(u => u.IsKind(SyntaxKind.IdentifierName)
                        || u.IsKind(SyntaxKind.QualifiedName))
                    .FirstOrDefault();
                if(usingIdentifier != null)
                {
                    var possibleFullyQualifiedClassName = string.Format(
                        "{0}.{1}", usingIdentifier.ToString(), typeClassName);
                    if (Type.GetType(possibleFullyQualifiedClassName) != null)
                    {
                        return possibleFullyQualifiedClassName;
                    }
                }
            }
            return typeClassName;
        }

        private static string GetTypeClassName(SyntaxNode classNameNode)
        {
            var className = classNameNode.ToString();
            var baseClassNames = classNameNode.ChildNodes();
            var genericArgumentCount = baseClassNames.Count();
            if(genericArgumentCount > 0)
            {
                Match match = Regex.Match(className, (@"[A-Za-z0-9_\.]+"));
                className = match.Value + "`" + genericArgumentCount.ToString();
            }
            return className;
        }

        private void AddDocumentationNodesToNodeList(IEnumerable<DocumentationNode> documentationNodes)
        {
            var firstTextNode = Node.CreateXmlText(_documentationCommentDelimiter,
                Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter));
            _nodes = _nodes.Add(firstTextNode);
            var tokenNumber = 0;
            var tokenList = new List<SyntaxToken>();
            foreach(var docNode in documentationNodes)
            {
                tokenList.Clear();
                if(tokenNumber != 0)
                {
                    tokenList.Add(Token.CreateXmlTextNewLine());
                    tokenList.Add(Token.CreateXmlTextLiteral(singleSpace, _documentationCommentDelimiter));
                }
                ++tokenNumber;
                var textNode = Node.CreateXmlText(_documentationCommentDelimiter, tokenList.ToArray());
                _nodes = _nodes.Add(textNode);
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
