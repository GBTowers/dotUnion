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
	public async Task Play()
	{
		Notification notification = new Notification.Bye(new Color());
		Task<Notification> notificationTask = Task.FromResult(notification);

		_ = await notificationTask.Union().Match(bye => bye.Color);


	}
}
