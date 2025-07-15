# dotUnion - Source Generator

## A Source Generator to generate Union types inside C\#

As of July 2025 - C# doesn't have proper union types, this library solves this using a source generator to create a closed polymorphic structure.

Also included are analyzers to help with syntax rules and common errors.

## Installation

The NuGet package is available in nuget.org, and you can use your IDE or run the following command to install it

```
nuget install dotUnion
```

## Usage

Once installed, you only need to mark your record type with the \[Union\] Attribute,
This will generate the polymorphic relationship and the corresponding methods.

```CSharp
using dotUnion.Attributes;

[Union]
public partial record Result<T,TE>
{
	partial record Ok(T Value);
	partial record Err(TE Error);
}
```

Once you have your union you can use the Match method variants to map every union variant into a single value.

```CSharp
Result<User, Error> result = GetUser();

string match = result.Match(
	ok => ok.Value.Name,
	err => err.Error.Message
);
```

Or Switch method variants to execute a different action for every union variant.

```CSharp

result.Switch(
	ok => RegisterUser(ok.Value),
	err => NotifyError(err.Error)
);
```


## Analyzers

This source generator comes with a set of analyzer rules to help you catch errors in the union type.

### Rules

| Rule ID | Category | Default Severity | Description                                      |
|---------|----------|------------------|--------------------------------------------------|
| UL1001  | Union    | Error            | Union target must be partial                     |
| UL1002  | Union    | Error            | Union parents must be partial                    |
| UL1003  | Union    | Error            | Union type cannot be sealed                      |
| UL1004  | Union    | Error            | Union target cannot have base type               |
| UL1005  | Union    | Error            | Union parent cannot have non-private constructor |
| UL1006  | Union    | Error            | Union type can only have one non-generated part  |
| UL2001  | Union    | Error            | Union member must be partial                     |
| UL2002  | Union    | Error            | Union member cannot be generic                   |
| UL2003  | Union    | Error            | Union member cannot have base type               |
| UL2004  | Union    | Info             | Union type member must be a record and partial   |
| UL2005  | Union    | Error            | Union type member must be public                 |

## Async Extensions

Async extension methods can be enabled on a per-project basis using the `AsyncUnionExtensions` property in the `.csproj` file.

```XML
<Project>
	<PropertyGroup>  
		<AsyncUnionExtensions>enable</AsyncUnionExtensions>  
	</PropertyGroup>
</Project>
```

Once enabled, you have to use the `Union()` extension to access the `Switch()` and `Match()` extension methods for the union tasks, this is due to the lack of covariance in C# types.

```CSharp

Task<Result<User, Error>> resultTask = GetUserAsync();

string match = await resultTask.Union().Match(
	ok => ok.Value.Name,
	err => err.Error.Message
);
```

# Implicit operators

The source generator automatically creates implicit operators for every part of the union if all parts of the union have a different primary constructor.

Union parts with more than one parameter in their constructor are given an implicit operator that takes in a tuple with the same signature as the constructor.

```CSharp
[Union]
partial record Notification
{
	partial record Payment(decimal Amount);
	partial record Refund(decimal Amount, string Reason);
}

Notification payment = 50m;
Notification refund = (50m, "Lost product");
```
