# dotUnion

C# Source Generator for Discriminated Union-like polymorphic relationships

[Source Code](https://github.com/GBTowers/dotUnion)

---

## Basic Usage

`install-package dotUnion`

```csharp
using dotUnion.Attributes;

namespace Test;

[Union]
public abstract partial record Notification
{
	partial record StatusNotification(string Message, int Code);
	partial record PaymentNotification(decimal Amount, string Account);
}
```
