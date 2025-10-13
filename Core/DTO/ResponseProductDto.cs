using System.ComponentModel.DataAnnotations;

namespace Core.DTO;

public record ResponseProductDto
{
    public int Id { get; init; }
    [StringLength(100), MinLength(1)] public string Name { get; init; }
    [Range(1, int.MaxValue)] public decimal Price { get; init; }
    public int CategoryId { get; init; }
}