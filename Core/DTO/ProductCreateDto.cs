using System.ComponentModel.DataAnnotations;

namespace Core.DTO;

public record CreateProductRequest
{
    [StringLength(100), MinLength(1)] public string Name { get; set; }
    [Range(1, int.MaxValue)] public decimal Price { get; set; }
    [Range(1, int.MaxValue), Required] public int CategoryId { get; set; }
}