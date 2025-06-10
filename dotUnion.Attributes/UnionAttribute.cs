namespace dotUnion.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class UnionAttribute : Attribute
{
	public bool GenerateAsyncExtensions { get; set; }
}
