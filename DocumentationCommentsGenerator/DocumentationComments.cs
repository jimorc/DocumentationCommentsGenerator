using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationCommentsGenerator
{
    internal abstract class DocumentationComments
    {
        internal DocumentationComments(SyntaxNode nodeToDocument)
        {
            var tree = nodeToDocument.SyntaxTree;
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            _model = compilation.GetSemanticModel(tree);

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

            AddNewLineNodeToNodes(DocumentationCommentDelimiter);
            InsertIndentationNode(_lastLeadingTrivia.ToString());
        }

        protected abstract IEnumerable<DocumentationNode> CreateNewSummaryNodes(SyntaxNode nodeToDocument);

        private void InsertIndentationNode(string indentationText)
        {
            var indentLiteralToken = Token.CreateXmlTextLiteral(indentationText, NoSpace);
            var indentNode = Node.CreateXmlText(NoSpace, indentLiteralToken);
            _nodes = _nodes.Add(indentNode);
        }

        private void InsertLeadingNewLines(SyntaxTriviaList leadingTrivia)
        {
            foreach (var trivia in leadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    AddNewLineNodeToNodes(NoSpace);
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
                    triviaNodes.Add(new DocumentationNode(elementTrivia, DocumentationCommentDelimiter));
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
                if (triviaNode.DocumentationTagName.Equals(Summary))
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

        private void AddDocumentationNodesToNodeList(IEnumerable<DocumentationNode> documentationNodes)
        {
            var firstTextNode = Node.CreateXmlText(DocumentationCommentDelimiter,
                Token.CreateXmlTextLiteral(SingleSpace, DocumentationCommentDelimiter));
            _nodes = _nodes.Add(firstTextNode);
            var tokenNumber = 0;
            var tokenList = new List<SyntaxToken>();
            foreach(var docNode in documentationNodes)
            {
                tokenList.Clear();
                if(tokenNumber != 0)
                {
                    tokenList.Add(Token.CreateXmlTextNewLine());
                    tokenList.Add(Token.CreateXmlTextLiteral(SingleSpace, DocumentationCommentDelimiter));
                }
                ++tokenNumber;
                var textNode = Node.CreateXmlText(DocumentationCommentDelimiter, tokenList.ToArray());
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
        protected static string Summary { get => "summary"; }
        private SyntaxList<XmlNodeSyntax> _nodes = SyntaxFactory.List<XmlNodeSyntax>();
        private List<DocumentationNode> triviaNodes = new List<DocumentationNode>();
        private SyntaxTrivia _lastLeadingTrivia;
        protected SemanticModel _model;
        private readonly string _documentationCommentDelimiter;

        protected static string NoSpace { get => string.Empty; }
        protected static string SingleSpace { get => " "; }
        protected string DocumentationCommentDelimiter { get => _documentationCommentDelimiter; }
    }
}
