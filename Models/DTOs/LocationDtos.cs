using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models.DTOs;

public record CreateLocationDto(
    [Required] string Name,
    string? Address,
    string? City,
    string? Province,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude
);

public record LocationDto(
    int LocationId, string Name, string? Address, string? City,
    string? Province, string? PostalCode, decimal? Latitude, decimal? Longitude
);