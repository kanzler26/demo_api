using Core.DTO;

namespace Core.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Product> Products { get; set; }
    //public int ProductsCount { get; set; }
}