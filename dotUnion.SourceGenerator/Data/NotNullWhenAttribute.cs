namespace dotUnion.SourceGenerator.Data;

internal sealed class NotNullWhenAttribute : Attribute
{
	public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

	public bool ReturnValue { get; }
}

internal sealed class NotNullIfNotNullAttribute : Attribute
{
	/// <summary>Initializes the attribute with the associated parameter name.</summary>
	/// <param name="parameterName">
	///   The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.
	/// </param>
	public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

	/// <summary>Gets the associated parameter name.</summary>
	public string ParameterName { get; }
}
