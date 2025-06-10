using DbtHelper.Jinja2;

namespace DbtHelper.Jinja.Tests;

public class TemplateTests
{
    [Theory]
    [InlineData("Assignment")]
    [InlineData("Addition")]
    [InlineData("Subtraction")]
    [InlineData("CollectionAssignment")]
    public void Test1(string fileName)
    {
        var content = Helper.Read(fileName);
        var rendered = Helper.ReadRendered(fileName);
        
        var template = Template.FromString(content);
        var renderedContent = template.Render();
        Assert.Equal(rendered, renderedContent);
    }
}