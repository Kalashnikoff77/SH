using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Models;

namespace Common.Repository
{
    public interface IRepository<TModel, TRequestDto, TResponseDto> 
        where TModel : ModelBase 
        where TRequestDto : RequestDtoBase
        where TResponseDto : ResponseDtoBase, new()
    {
        Task<ApiResponse<TResponseDto>> HttpPostAsync(TModel model);
    }
}
