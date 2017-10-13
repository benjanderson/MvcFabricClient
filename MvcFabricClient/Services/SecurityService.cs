namespace MvcFabricClient.Services
{
    using System.Threading.Tasks;

    using System.IdentityModel.Claims;
    using System.Linq;
    using System.Security.Authentication;
    using System.Web;
    using System.Security.Claims;
    using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;
    using IdentityModel.Client;
    using System.Security;

    public interface ISecurityService
    {
        Task<bool> IsEdwAdmin();

        bool IsUserDataSteward(string dataMartName);

        bool IsAdminOrSteward(string dataMartName);

        bool IsAdminOrCreator(string creatorIdentityName);

        Task<bool> UserCanExecuteBatchAsync(int dataMartId);

        void AddUserToEdwAdminRole(string userName);

        void RemoveUserFromEdwAdminRole(string userName);

        //bool CreateUserIfNotExists(AtlasUser atlasUser);

        bool UserExists(string userName);
    }


    public class IdentitySecurityService : ISecurityService
    {
        private readonly AuthorizationClient authorizationClient;

        public async Task<bool> IsEdwAdmin()
        {
            var group = await this.authorizationClient.GetGroupUsers("peters-admin-group2");
            return group.users.Any(user => user.subjectId == CurrentUserName);
        }

        public static string CurrentUserName
        {
            get
            {
                var user = HttpContext.Current.User as ClaimsPrincipal;
                if (!ClaimsPrincipal.Current.Identity.IsAuthenticated)
                {
                    throw new SecurityException("Not authenticated");
                }

                var username = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity.Name;
                if (username == null)
                {
                    throw new SecurityException($"Name/Identify claim not found");

                }

                return username;
            }
        }

        public IdentitySecurityService(AuthorizationClient authorizationClient)
        {
            this.authorizationClient = authorizationClient;
        }

        public void AddUserToEdwAdminRole(string userName)
        {

        }

        public bool IsAdminOrCreator(string creatorIdentityName)
        {
            return true;
        }

        public bool IsAdminOrSteward(string dataMartName)
        {
            return true;
        }

        public bool IsUserDataSteward(string dataMartName)
        {
            return true;
        }

        public void RemoveUserFromEdwAdminRole(string userName)
        {
        }

        public Task<bool> UserCanExecuteBatchAsync(int dataMartId)
        {
            return Task.FromResult(true);
        }

        public bool UserExists(string userName)
        {
            return true;
        }
    }
}