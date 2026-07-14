namespace OnibusExpress.Tests.Domain;

public class ReservationCodeTests
{
    [Fact]
    public void NewRandom_ProducesAbc12345Format()
    {
        var random = new Random(1);
        for (var i = 0; i < 1000; i++)
        {
            var code = ReservationCode.NewRandom(random.Next);
            Assert.Matches("^[A-Z]{3}-[0-9]{5}$", code.Value);
        }
    }

    [Fact]
    public void NewRandom_ProducesVariety()
    {
        var random = new Random(1);
        var codes = new HashSet<string>();
        for (var i = 0; i < 500; i++)
        {
            var code = ReservationCode.NewRandom(random.Next);
            codes.Add(code.Value);
        }

        Assert.True(codes.Count > 490);
    }

    [Fact]
    public void NewRandom_UsesInjectedDelegate()
    {
        var code = ReservationCode.NewRandom(_ => 0);
        Assert.Equal("AAA-00000", code.Value);
    }

    [Fact]
    public void Create_ValidParse_TrimsAndUppercases()
    {
        var code = ReservationCode.Create(" abc-12345 ");
        Assert.Equal("ABC-12345", code.Value);
    }

    [Theory]
    [InlineData("AB-12345")]
    [InlineData("ABCD-12345")]
    [InlineData("ABC-1234")]
    [InlineData("ABC-123456")]
    [InlineData("abc12345")]
    [InlineData("ABC_12345")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_InvalidFormat_Throws(string? value)
    {
        Assert.Throws<InvalidReservationCodeException>(() => ReservationCode.Create(value));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        Assert.True(ReservationCode.Create("ABC-12345") == ReservationCode.Create("abc-12345"));
    }
}
