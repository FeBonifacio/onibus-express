namespace OnibusExpress.Tests.Domain;

public class EmailTests
{
    [Fact]
    public void Create_Valid_TrimsValue()
    {
        var email = Email.Create("  ana@teste.com  ");

        Assert.Equal("ana@teste.com", email.Value);
    }

    [Theory]
    [InlineData("no-at")]
    [InlineData("a@b")]
    [InlineData("a@b.")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("a b@c.com")]
    public void Create_Invalid_Throws(string? value)
    {
        Assert.Throws<InvalidEmailException>(() => Email.Create(value));
    }

    [Fact]
    public void Equality_CaseInsensitive()
    {
        Assert.True(Email.Create("A@B.com").Equals(Email.Create("a@b.com")));
    }
}
