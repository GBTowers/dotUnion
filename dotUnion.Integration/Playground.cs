using System.Drawing;
using dotUnion.Attributes;

namespace dotUnion.Integration;

[Union]
public partial record Notification
{
	partial record Bye(Color Color);
}


public partial class Hello
{
	[Union]
	public partial record Option<T>
	{
		partial record Some(T Value);
		partial record None;
	}
}

	
public class Playground
{
	[Fact]
	public void Play()
	{
		Notification notification = new Notification.Bye(new Color());
		Task<string> result = Task.FromResult("Hello");

		Task<string> x = notification.MatchAsync(async b => await result);
	}
}
