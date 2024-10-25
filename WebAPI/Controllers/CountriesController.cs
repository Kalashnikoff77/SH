using AutoMapper;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Dapper;
using DataContext.Entities.Views;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : MyControllerBase
    {
        public CountriesController(IMapper mapper, IConfiguration configuration) : base(mapper, configuration) { }

        [Route("Get"), HttpPost]
        public async Task<GetCountriesResponseDto> GetAsync(GetCountriesRequestDto request)
        {
            var response = new GetCountriesResponseDto();

            IEnumerable<CountriesViewEntity> result;

            if (request.CountryId == null)
                result = await _unitOfWork.SqlConnection.QueryAsync<CountriesViewEntity>($"SELECT * FROM CountriesView ORDER BY [Order] ASC, Name ASC");
            else
                result = await _unitOfWork.SqlConnection.QueryAsync<CountriesViewEntity>($"SELECT TOP 1 * FROM CountriesView WHERE Id = @Id", new { Id = request.CountryId.Value });
            response.Countries = _mapper.Map<List<CountriesViewDto>>(result);

            return response;
        }


        /// <summary>
        /// Получает список регионов, в которых есть активные мероприятия
        /// </summary>
        [Route("GetRegionsForEvents"), HttpPost]
        public async Task<GetRegionsForEventsResponseDto> GetRegionsForEventsAsync(GetRegionsForEventsRequestDto request)
        {
            var response = new GetRegionsForEventsResponseDto();

            var sql = "SELECT * FROM RegionsForEventsView ORDER BY [Order]";
            var result = await _unitOfWork.SqlConnection.QueryAsync<RegionsForEventsViewEntity>(sql);
            response.RegionsForEvents = _mapper.Map<List<RegionsForEventsViewDto>>(result);

            return response;
        }

    }
}
