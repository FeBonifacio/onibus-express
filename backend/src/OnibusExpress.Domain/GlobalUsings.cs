// Domain has ZERO external dependencies. It does not know the source of randomness
// (that is why ReservationCode.NewRandom takes a Func<int,int>).
global using System.Text.RegularExpressions;

// Domain's own namespaces, so files reference each other without repeated usings.
global using OnibusExpress.Domain.Abstractions;
global using OnibusExpress.Domain.Common;
global using OnibusExpress.Domain.Enums;
global using OnibusExpress.Domain.Exceptions;
global using OnibusExpress.Domain.ValueObjects;
