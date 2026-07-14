// Crypto lives here (used by ReservationCodeGenerator), not in the Domain.
global using System.Security.Cryptography;
// Application (internal namespaces, so use cases/mappers stay clean)
global using OnibusExpress.Application.Abstractions;
global using OnibusExpress.Application.Common;
global using OnibusExpress.Application.DTOs;
global using OnibusExpress.Application.Exceptions;
global using OnibusExpress.Application.Mapping;
global using OnibusExpress.Application.Services;
// Domain
global using OnibusExpress.Domain.Abstractions;
global using OnibusExpress.Domain.Common;
global using OnibusExpress.Domain.Entities;
global using OnibusExpress.Domain.ValueObjects;
