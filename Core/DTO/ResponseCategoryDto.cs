using System.ComponentModel.DataAnnotations;

namespace Core.DTO;

public class ResponseCategoryDto
{
    public int Id { get; set; }
    [StringLength(100), MinLength(1)] public string Name { get; set; }
 }