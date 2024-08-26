using AutoMapper;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Dapper;
using DataContext.Entities.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

            using (var conn = new SqlConnection(connectionString))
            {
                IEnumerable<CountriesViewEntity> result;

                if (request.CountryId == null)
                    result = await conn.QueryAsync<CountriesViewEntity>($"SELECT * FROM CountriesView ORDER BY [Order] ASC, Name ASC");
                else
                    result = await conn.QueryAsync<CountriesViewEntity>($"SELECT TOP 1 * FROM CountriesView WHERE Id = @Id", new { Id = request.CountryId.Value });

                response.Countries = _mapper.Map<List<CountriesViewDto>>(result);
            }
            return response;
        }


        /// <summary>
        /// Получает список регионов, в которых есть активные мероприятия
        /// </summary>
        [Route("GetRegionsForEvents"), HttpPost]
        public async Task<GetRegionsForEventsResponseDto> GetRegionsForEventsAsync(GetRegionsForEventsRequestDto request)
        {
            var response = new GetRegionsForEventsResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM RegionsForEventsView ORDER BY [Order]";
                var result = await conn.QueryAsync<RegionsForEventsViewEntity>(sql);
                response.RegionsForEvents = _mapper.Map<List<RegionsForEventsViewDto>>(result);
            }
            return response;
        }

    }
}
