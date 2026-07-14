namespace OnibusExpress.Tests.Domain;

public class DocumentTests
{
    [Fact]
    public void Create_WithValidCpfNoMask_ReturnsDocument()
    {
        var document = Document.Create("52998224725");
        Assert.Equal("52998224725", document.Value);
    }

    [Fact]
    public void Create_WithValidCpfMasked_ReturnsDocument()
    {
        var document = Document.Create("529.982.247-25");
        Assert.Equal("52998224725", document.Value);
    }

    [Fact]
    public void Create_WithSecondValidCpf_ReturnsDocument()
    {
        var document = Document.Create("111.444.777-35");
        Assert.Equal("11144477735", document.Value);
    }

    [Fact]
    public void Create_WithCheckDigitZero_ReturnsDocument()
    {
        var document = Document.Create("168.995.350-09");
        Assert.Equal("16899535009", document.Value);
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("33333333333")]
    [InlineData("44444444444")]
    [InlineData("55555555555")]
    [InlineData("66666666666")]
    [InlineData("77777777777")]
    [InlineData("88888888888")]
    [InlineData("99999999999")]
    [InlineData("10101010101")]
    public void Create_WithRepeatedSequence_ThrowsInvalidDocument(string value)
    {
        Assert.Throws<InvalidDocumentException>(() => Document.Create(value));
    }

    [Fact]
    public void Create_WithWrongCheckDigit_ThrowsInvalidDocument()
    {
        Assert.Throws<InvalidDocumentException>(() => Document.Create("52998224724"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("1234567890")]
    public void Create_WithInvalidLength_ThrowsInvalidDocument(string? value)
    {
        Assert.Throws<InvalidDocumentException>(() => Document.Create(value));
    }

    [Fact]
    public void Create_WithNoiseReducingBelow11Digits_ThrowsInvalidDocument()
    {
        Assert.Throws<InvalidDocumentException>(() => Document.Create("529.982.24a-25"));
    }

    [Fact]
    public void Create_WithNoiseButElevenValidDigits_LenientAccepts()
    {
        var document = Document.Create("52998224725x");
        Assert.Equal("52998224725", document.Value);
    }

    [Theory]
    [InlineData("52998224725", true)]
    [InlineData("529.982.247-25", true)]
    [InlineData("111.111.111-11", false)]
    [InlineData("52998224724", false)]
    [InlineData("123", false)]
    public void IsValid_ReturnsBoolWithoutThrowing(string value, bool expected)
    {
        Assert.Equal(expected, Document.IsValid(value));
    }

    [Fact]
    public void Equality_MaskedVsRaw_AreEqual()
    {
        var masked = Document.Create("529.982.247-25");
        var raw = Document.Create("52998224725");
        Assert.True(masked.Equals(raw));
        Assert.Equal(masked.GetHashCode(), raw.GetHashCode());
    }

    [Fact]
    public void Formatted_ReturnsCorrectMask()
    {
        var document = Document.Create("52998224725");
        Assert.Equal("529.982.247-25", document.Formatted);
    }
}
