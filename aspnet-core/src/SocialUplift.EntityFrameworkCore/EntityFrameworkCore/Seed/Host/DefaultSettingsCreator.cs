using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.Configuration;
using Abp.Localization;
using Abp.Net.Mail;
using SocialUplift.Configuration;

namespace SocialUplift.EntityFrameworkCore.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly SocialUpliftDbContext _context;

        public DefaultSettingsCreator(SocialUpliftDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            // Emailing
            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "app@socialuplift.com");
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "socialuplift.com mailer");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Host, "smtp.mailtrap.io");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Port, "2525");
            AddSettingIfNotExists(EmailSettingNames.Smtp.UserName, "b9ff45157a55b7");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Password, "da6d37639bc457");
            AddSettingIfNotExists(EmailSettingNames.Smtp.UseDefaultCredentials, "false");
            AddSettingIfNotExists(EmailSettingNames.Smtp.EnableSsl, "false");

            // Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en");

            // Others Settings
            AddSettingIfNotExists(AppSettingNames.ShowAddAccountButton, "false");
            AddSettingIfNotExists(AppSettingNames.ShowCampaignsButton, "false");
            AddSettingIfNotExists(AppSettingNames.ShowNumbersTabs, "false");
            AddSettingIfNotExists(AppSettingNames.MaxNumberOfAccions, "500");

        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
        {
            if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
            {
                return;
            }

            _context.Settings.Add(new Setting(tenantId, null, name, value));
            _context.SaveChanges();
        }
    }
}
