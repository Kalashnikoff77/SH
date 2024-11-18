using Common.Dto.Requests;
using Common.Dto.Responses;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Common.Repository
{
    public class Repository<TRequestDto, TResponseDto> : IRepository<TRequestDto, TResponseDto>
        where TRequestDto : RequestDtoBase
        where TResponseDto : ResponseDtoBase, new()
    {

        readonly HttpClient _httpClient;
        readonly IConfiguration _config;

        public Repository(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }


        public async Task<ApiResponse<TResponseDto>> HttpPostAsync(TRequestDto request)
        {
            var apiResponse = await PostAsJsonAsync<TRequestDto, TResponseDto>(request);
            return apiResponse;
        }


        private async Task<ApiResponse<TResponse>> PostAsJsonAsync<TRequest, TResponse>(TRequest request) 
            where TRequest : RequestDtoBase
            where TResponse : ResponseDtoBase, new()
        {
            var apiResponse = new ApiResponse<TResponse>();

            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(request.Token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);

            var host = _config.GetRequiredSection("WebAPI:Host").Value;

            var response = await _httpClient.PostAsJsonAsync($"{host}{request.Uri}", request);

            apiResponse.StatusCode = response.StatusCode;

            if (apiResponse.StatusCode != HttpStatusCode.Unauthorized)
                apiResponse.Response = (await response.Content.ReadFromJsonAsync<TResponse>())!;

            return apiResponse;
        }
    }
}
