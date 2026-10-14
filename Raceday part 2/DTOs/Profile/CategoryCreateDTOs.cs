// DTO used when an Organiser creates or updates a Category
public record CategoryCreateDto(
    string Name,
    int? MinAge,
    int? MaxAge,
    decimal? CategoryDistance
);

// DTO used when returning Category data to the client
public record CategoryResponseDto(
    int CategoryId,
    int EventId,
    string Name,
    int? MinAge,
    int? MaxAge,
    decimal? CategoryDistance
);