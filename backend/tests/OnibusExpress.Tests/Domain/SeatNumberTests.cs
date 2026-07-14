namespace OnibusExpress.Tests.Domain;

public class SeatNumberTests
{
    [Fact]
    public void Create_Positive_Ok()
    {
        Assert.Equal(1, SeatNumber.Create(1).Value);
        Assert.Equal(44, SeatNumber.Create(44).Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_LessThanOne_ThrowsInvalidSeat(int value)
    {
        Assert.Throws<InvalidSeatException>(() => SeatNumber.Create(value));
    }

    [Fact]
    public void Equality_ByValue()
    {
        Assert.True(SeatNumber.Create(7) == SeatNumber.Create(7));
        Assert.True(SeatNumber.Create(7) != SeatNumber.Create(8));
    }
}
