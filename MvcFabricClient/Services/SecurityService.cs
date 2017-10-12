namespace MvcFabricClient.Services
{
    using System.Threading.Tasks;

    using System.IdentityModel.Claims;
    using System.Linq;
    using System;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Collections.Generic;
    using System.Threading;
    using System.Security.Authentication;
    using System.Web;
    using System.Security.Claims;
    using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;
    using IdentityModel.Client;

    public interface ISecurityService
    {
        bool IsEdwAdmin { get; }

        string CurrentUserName { get; }

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
        private readonly ClaimsPrincipal user;
        private readonly AuthorizationClient authorizationClient;

        public bool IsEdwAdmin
        {
            get
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;
                var group = this.authorizationClient.GetGroupByName("EDWAdmin", token);
                if (!group.Wait(100))
                {
                    source.Cancel();
                }
                return group.Result != null;
            }
        }

        public string CurrentUserName
        {
            get
            {
                if (!ClaimsPrincipal.Current.Identity.IsAuthenticated)
                {
                    throw new System.Security.SecurityException("Not authenticated");
                }

                var nameIdClaim = this.user.FindFirst("sub") ?? this.user.FindFirst(ClaimTypes.NameIdentifier) ?? this.user.FindFirst(ClaimTypes.Name);
                if (nameIdClaim == null)
                {
                    throw new System.Security.SecurityException($"Name/Identify claim not found");

                }

                return nameIdClaim.Value;
            }
        }

        public IdentitySecurityService(ClaimsPrincipal user, AuthorizationClient authorizationClient)
        {
            this.user = user;
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

    ////
    //// Below is copied from https://github.com/HealthCatalyst/Fabric.Platform which is a nuget package but only supports .net core 1.4
    ////
    public class AuthorizationClient : IDisposable
    {
        private HttpClient _client;
        private readonly Uri authClientUrl;
        private readonly ClaimsPrincipal user;

        private HttpClient client
        {
            get
            {
                if (_client == null)
                {
                    var token = this.user.FindFirst("sid").Value;

                    _client = new HttpClient { BaseAddress = this.authClientUrl };
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _client.DefaultRequestHeaders.Add(FabricHeaders.CorrelationTokenHeaderName, Guid.NewGuid().ToString());
                    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                }
                return _client;
            }
        }

        public AuthorizationClient(Uri authClientUrl, ClaimsPrincipal user)
        {
            this.authClientUrl = authClientUrl;
            this.user = user;
        }

        public async Task<dynamic> CreatePermission(dynamic permission)
        {
            var patientPermissionResponse = await client.PostAsync("/permissions", CreateJsonContent(permission));
            patientPermissionResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await patientPermissionResponse.Content.ReadAsStringAsync());
        }

        public async Task<dynamic> GetPermission(string grain, string securableItem, string name)
        {
            var permissionResponse = await client.GetAsync($"/permissions/{grain}/{securableItem}/{name}");
            permissionResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await permissionResponse.Content.ReadAsStringAsync());
        }

        public async Task<dynamic> CreatRole(dynamic role)
        {
            var roleResponse = await client.PostAsync("/roles", CreateJsonContent(role));
            roleResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await roleResponse.Content.ReadAsStringAsync());
        }

        public async Task<dynamic> GetRole(string grain, string securableItem, string name)
        {
            var roleResponse = await client.GetAsync($"/roles/{grain}/{securableItem}/{name}");
            roleResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await roleResponse.Content.ReadAsStringAsync());
        }

        public async Task<bool> AddPermissionToRole(dynamic permission, dynamic role)
        {
            var rolePermissionResponse = await client.PostAsync($"/roles/{role.id}/permissions", CreateJsonContent(new[] { permission }));
            rolePermissionResponse.EnsureSuccessStatusCode();
            return rolePermissionResponse.IsSuccessStatusCode;
        }

        public async Task<bool> AddRoleToGroup(dynamic role, string groupName)
        {
            var groupUrlStub = WebUtility.UrlEncode(groupName);
            var groupRoleResponse = await client.PostAsync($"/groups/{groupUrlStub}/roles", CreateJsonContent(role));
            groupRoleResponse.EnsureSuccessStatusCode();
            return groupRoleResponse.IsSuccessStatusCode;
        }

        public async Task<dynamic> GetGroupByName(string groupName, CancellationToken cancellationToken)
        {
            var groupUrlStub = WebUtility.UrlEncode(groupName);
            var groupResponse = await client.GetAsync($"/groups/{groupUrlStub}/roles", cancellationToken);
            groupResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await groupResponse.Content.ReadAsStringAsync());
        }

        public async Task<UserPermissions> GetPermissionsForUser(string grain, string securableItem)
        {
            var permissionResponse = await client.GetAsync($"/user/permissions?grain={grain}&securableItem={securableItem}");
            permissionResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<UserPermissions>(await permissionResponse.Content.ReadAsStringAsync());
        }


        private StringContent CreateJsonContent(object model)
        {
            return new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AuthorizationClient()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }
    }

    public class UserPermissions
    {
        public IEnumerable<string> Permissions { get; set; }
        public string RequestedGrain { get; set; }
        public string RequestedSecurableItem { get; set; }
    }

    public static class FabricHeaders
    {
        public const string SubjectNameHeader = "fabric-end-user-subject-id";
        public const string IdTokenHeader = "fabric-end-user";
        public const string CorrelationTokenHeaderName = "correlation-token";
        public const string AuthenticationHeaderPrefix = "Bearer";
    }
}