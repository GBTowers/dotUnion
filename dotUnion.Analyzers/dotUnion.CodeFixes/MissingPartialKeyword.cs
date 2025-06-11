using System;
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
///   A sample code fix provider that adds partial keyword to marked type as needed
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingPartialKeywordCodeFixProvider)), Shared]
public sealed class MissingPartialKeywordCodeFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["UL1001", "UL1002", "UL1003"];

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
					title: "Add partial keyword",
					createChangedDocument: token => AddPartialKeyword(
						document: context.Document,
						typeDeclaration: declaration,
						cancellationToken: token
					),
					equivalenceKey: "Add Partial Keyword for Union"
				),
				diagnostic
			);
		}
	}

	private static async Task<Document> AddPartialKeyword(
		Document document,
		TypeDeclarationSyntax typeDeclaration,
		CancellationToken cancellationToken
	)
	{
		TypeDeclarationSyntax typeWithPartial = typeDeclaration.WithoutTrivia()
			.WithModifiers(
				typeDeclaration.Modifiers.Insert(
					index: Math.Max(
						val1: typeDeclaration.Modifiers.IndexOf(SyntaxKind.AbstractKeyword) + 1,
						val2: HasAccessModifierKeyword(typeDeclaration.Modifiers) ? 1 : 0
					),
					token: SyntaxFactory.Token(SyntaxKind.PartialKeyword)
				)
			);

		SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		SyntaxNode? newRoot = oldRoot?.ReplaceNode(
			oldNode: typeDeclaration,
			newNode: typeWithPartial.WithTriviaFrom(typeDeclaration)
		);

		return newRoot is null ? document : document.WithSyntaxRoot(newRoot);
	}

	private static bool HasAccessModifierKeyword(SyntaxTokenList modifiers)
		=> modifiers.Any(SyntaxKind.PublicKeyword)
		|| modifiers.Any(SyntaxKind.PrivateKeyword)
		|| modifiers.Any(SyntaxKind.InternalKeyword);
}