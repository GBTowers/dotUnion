using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace dotUnion.CodeFixes;

/// <summary>
///   A sample code fix provider that removes sealed keyword to marked type as needed
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ProhibitedSealedKeywordCodeFixProvider)), Shared]
public sealed class ProhibitedSealedKeywordCodeFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["UL1004"];

	public override FixAllProvider? GetFixAllProvider() => null;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (Diagnostic diagnostic in context.Diagnostics)
		{
			TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);


			SyntaxNode? diagnosticNode = root?.FindNode(diagnosticSpan);

			if (diagnosticNode is not TypeDeclarationSyntax declaration) return;

			context.RegisterCodeFix(
				action: CodeAction.Create(
					title: "Remove Sealed Keyword",
					createChangedDocument: token => RemoveSealedKeyword(
						document: context.Document,
						typeDeclaration: declaration,
						cancellationToken: token
					),
					equivalenceKey: "Remove Sealed Keyword"
				),
				diagnostic
			);
		}
	}

	private static async Task<Document> RemoveSealedKeyword(
		Document document,
		TypeDeclarationSyntax typeDeclaration,
		CancellationToken cancellationToken
	)
	{
		TypeDeclarationSyntax typeWithoutSealed = typeDeclaration.WithoutTrivia()
		.WithModifiers(typeDeclaration.Modifiers.RemoveAt(typeDeclaration.Modifiers.IndexOf(SyntaxKind.SealedKeyword)));

		SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		SyntaxNode? newRoot = oldRoot?.ReplaceNode(
			oldNode: typeDeclaration,
			newNode: typeWithoutSealed.WithTriviaFrom(typeDeclaration)
		);

		return newRoot is null ? document : document.WithSyntaxRoot(newRoot);
	}
}
