using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    public class MyControllerBase : ControllerBase
    {
        protected IMapper _mapper;
        protected IConfiguration _configuration;
        protected ClaimsIdentity? _identity;

        protected int _accountId;
        protected Guid _accountGuid;

        protected string connectionString = null!;

        public MyControllerBase(IMapper mapper, IConfiguration configuration)
        {
            _mapper = mapper;
            _configuration = configuration;

            connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        }

        protected bool AuthenticateUser()
        {
            _identity = HttpContext.User.Identity as ClaimsIdentity;

            if (_identity != null && _identity.IsAuthenticated == true)
            {
                int.TryParse(_identity.Claims?.FirstOrDefault(x => x.Type == "Id")?.Value, out _accountId);
                Guid.TryParse(_identity.Claims?.FirstOrDefault(x => x.Type == "Guid")?.Value, out _accountGuid);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected List<string> GetRequiredColumns<T>()
        {
            var columns = typeof(T).GetMembers()
                .Where(x => x.IsDefined(typeof(RequiredAttribute), false))
                .Select(s => s.Name)
                .ToList();

            return columns;
        }
    }
}
