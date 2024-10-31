using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Identity;

namespace Pusula.Training.HealthCare.Blazor.Models;

public class CustomLoginModel : LoginModel
{
    public CustomLoginModel(IAuthenticationSchemeProvider schemeProvider, IOptions<AbpAccountOptions> accountOptions,
        IOptions<IdentityOptions> identityOptions, IdentityDynamicClaimsPrincipalContributorCache contributorCache)
        : base(schemeProvider, accountOptions, identityOptions, contributorCache)
    {
    }
}