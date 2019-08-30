using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using SocialUplift.Authorization;
using SocialUplift.Authorization.Accounts;
using SocialUplift.Authorization.Roles;
using SocialUplift.Authorization.Users;
using SocialUplift.Roles.Dto;
using SocialUplift.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Abp.Domain.Uow;
using Abp.Net.Mail;

namespace SocialUplift.Users
{
    [AbpAuthorize(PermissionNames.Pages_Users)]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly IEmailSender _emailSender;

        public UserAppService(
            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            IEmailSender emailSender,
            LogInManager logInManager)
            : base(repository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _emailSender = emailSender;
        }
        
        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to change password.");
            }
            long userId = _abpSession.UserId.Value;
            var user = await _userManager.GetUserByIdAsync(userId);
            var loginAsync = await _logInManager.LoginAsync(user.UserName, input.CurrentPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Existing Password' did not match the one on record. Please try again or contact an administrator for assistance in resetting your password.");
            }
            if (!new Regex(AccountAppService.PasswordRegex).IsMatch(input.NewPassword))
            {
                throw new UserFriendlyException("Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.");
            }
            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            CurrentUnitOfWork.SaveChanges();
            return true;
        }

        [AbpAllowAnonymous]
        public async Task<bool> ResetPasswordEmail(ResetPasswordEmailDto input)
        {

            if (input.NewPassword != input.ConfirmPassword)
            {
                throw new UserFriendlyException("New password and Confirm password do not match.");
            }

            //Disabling MayHaveTenant filter, so we can reach to all users
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                //Now, we can search for a user name in all tenants
                User userFromInput = await _userManager.GetUserByIdAsync(input.UserId);

                if (userFromInput.PasswordResetCode != input.Token)
                {
                    throw new UserFriendlyException("Token Not Provided!");
                }

                if (userFromInput == null)
                {
                    throw new UserFriendlyException("Your User did not match the one on record. Please try again or contact an administrator for assistance in resetting your password.");
                }

                if (!new Regex(AccountAppService.PasswordRegex).IsMatch(input.NewPassword))
                {
                    throw new UserFriendlyException("Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.");
                }

                userFromInput.IsLockoutEnabled = false;

                userFromInput.Password = _passwordHasher.HashPassword(userFromInput, input.NewPassword);

                CurrentUnitOfWork.SaveChanges();

                return true;
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> ForgotPassword(ForgotPasswordDto input)
        {

            //Disabling MayHaveTenant filter, so we can reach to all users
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                //Now, we can search for a user name in all tenants
                User userFromInput = await _userManager.Users.FirstOrDefaultAsync(u => u.EmailAddress == input.Email);

                if (userFromInput == null)
                {
                    throw new UserFriendlyException($"{input.Email} did not match any one in our records. Please try again or contact an administrator for assistance in resetting your password.");
                }

                userFromInput.SetNewPasswordResetCode();

                var callbackUrl = $"{GetHostName()}/?r=reset-password&i={userFromInput.Id}&t={userFromInput.PasswordResetCode}&n={userFromInput.EmailAddress}";
                //Send a notification email

                _emailSender.Send(
                    to: userFromInput.EmailAddress,
                    subject: "Social Uplift - Reset Password",
                    body: $"Please reset your password by clicking <a href=\"{callbackUrl}\">here</a>.",
                    isBodyHtml: true
                );

                return true;

            }
            
        }

        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to reset password.");
            }
            long currentUserId = _abpSession.UserId.Value;
            var currentUser = await _userManager.GetUserByIdAsync(currentUserId);
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
            }
            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                CurrentUnitOfWork.SaveChanges();
            }

            return true;
        }

        public async Task<UserDto> GetEditUser()
        {
            return await base.Get(new EntityDto<long> { Id = AbpSession.UserId.Value }); 
        }

        private string GetHostName()
        {
            try
            {
                string startupPath = System.IO.Directory.GetCurrentDirectory();

                var configuration = Configuration.AppConfigurations.Get(startupPath);
                
                var retVal = configuration["App:ClientRootAddress"];

                if (retVal != null)
                {
                    Logger.Info("NOT_FOUND_GetHostName");
                }

                return retVal;
            }
            catch (System.Exception ex)
            {
                Logger.Error("ErrorGetting_GetHostName", ex);
                return "";
            }
        }
    }
}

