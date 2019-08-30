using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SocialUplift.Roles.Dto;
using SocialUplift.Users.Dto;

namespace SocialUplift.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();

        Task ChangeLanguage(ChangeUserLanguageDto input);
    }
}
