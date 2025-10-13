using Core.DTO;
using Core.Models;
using Mapster;
using Category = Core.Models.Category;

namespace Application.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Category, ResponseCategoryDto>.NewConfig()
            // .Map(dest => dest.ProductsDto, src => src.ProductsDto)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        TypeAdapterConfig<CreateCategoryRequest, Category>.NewConfig()
            .Map(dest => dest.Name, src => src.Name);

        TypeAdapterConfig<Product, ResponseProductDto>.NewConfig()
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Price, src => src.Price);
    }
}