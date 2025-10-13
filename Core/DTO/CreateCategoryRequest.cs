using System.ComponentModel.DataAnnotations;

namespace Core.DTO;

public class CreateCategoryRequest
{
    [StringLength(100), MinLength(1)] public string Name { get; set; }
}