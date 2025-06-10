using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace dotUnion.SourceGenerator.Diagnosis;

internal sealed record DiagnosticInfo
{
	public DiagnosticInfo(DiagnosticDescriptor descriptor, Location? location)
	{
		Descriptor = descriptor;
		Location = location is not null ? LocationInfo.CreateFrom(location) : null;
	}

	public DiagnosticDescriptor Descriptor { get; }
	public LocationInfo? Location { get; }
}

internal record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
	public Location ToLocation()
		=> Location.Create(
			filePath: FilePath,
			textSpan: TextSpan,
			lineSpan: LineSpan
		);

	public static LocationInfo? CreateFrom(SyntaxNode node) => CreateFrom(node.GetLocation());

	public static LocationInfo? CreateFrom(Location location)
		=> location.SourceTree is not null
			? new LocationInfo(
				FilePath: location.SourceTree.FilePath,
				TextSpan: location.SourceSpan,
				LineSpan: location.GetLineSpan().Span
			)
			: null;
}
