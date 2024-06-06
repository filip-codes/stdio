namespace Studio.Support.Tests;

using Extensions;

public class String
{
    [Test]
    public void Camel()
    {
        Assert.That("hello_world".Camel(), Is.EqualTo("helloWorld"));
    }
    
    [Test]
    public void Snake()
    {
        Assert.That("Hello World".Snake(), Is.EqualTo("hello_world"));
    }
    
    [Test]
    public void Kebap()
    {
        Assert.That("Hello World".Kebab(), Is.EqualTo("hello-world"));
    }
    
    [Test]
    public void Append()
    {
        Assert.That("Hello ".Append("World"), Is.EqualTo("Hello World"));
    }
    
    [Test]
    public void Prepend()
    {
        Assert.That("World".Prepend("Hello "), Is.EqualTo("Hello World"));
    }
}
