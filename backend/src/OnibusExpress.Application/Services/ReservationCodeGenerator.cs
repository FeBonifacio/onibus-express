namespace OnibusExpress.Application.Services;

/// <summary>Generates codes using a cryptographic RNG (real randomness source).</summary>
public sealed class ReservationCodeGenerator : IReservationCodeGenerator
{
    public ReservationCode Generate() => ReservationCode.NewRandom(RandomNumberGenerator.GetInt32);
}
