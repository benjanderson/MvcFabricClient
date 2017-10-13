namespace MvcFabricClient.Services
{
    using System.Threading.Tasks;
    using System;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Security.Claims;
    using System.Collections.Generic;

    ////
    //// Below is copied from https://github.com/HealthCatalyst/Fabric.Platform which is a nuget package but only supports .net core 1.4
    ////
    public class AuthorizationClient : IDisposable
    {
        private HttpClient _client;
        private readonly string authClientUrl;
        private readonly ClaimsPrincipal user;

        private HttpClient client
        {
            get
            {
                if (_client == null)
                {
                    var token = this.user.FindFirst("access_token").Value;

                    _client = new HttpClient ();
                    _client.DefaultRequestHeaders.Clear();
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(FabricHeaders.AuthenticationHeaderPrefix, token);
                    _client.DefaultRequestHeaders.Add(FabricHeaders.CorrelationTokenHeaderName, Guid.NewGuid().ToString());
                    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return _client;
            }
        }

        public AuthorizationClient(string authClientUrl, ClaimsPrincipal user)
        {
            this.authClientUrl = authClientUrl;
            this.user = user;
        }

        public async Task<dynamic> CreatePermission(dynamic permission)
        {
            var patientPermissionResponse = await client.PostAsync($"{this.authClientUrl}/permissions", CreateJsonContent(permission));
            patientPermissionResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await patientPermissionResponse.Content.ReadAsStringAsync());
        }

        public async Task<dynamic> GetPermission(string grain, string securableItem, string name)
        {
            var permissionResponse = await client.GetAsync($"{this.authClientUrl}/permissions/{grain}/{securableItem}/{name}");
            permissionResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await permissionResponse.Content.ReadAsStringAsync());
        }

        public async Task<dynamic> CreatRole(dynamic role)
        {
            var roleResponse = await client.PostAsync($"{this.authClientUrl}/roles", CreateJsonContent(role));
            roleResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await roleResponse.Content.ReadAsStringAsync());
        }

        public async Task<dynamic> GetRole(string grain, string securableItem, string name)
        {
            var roleResponse = await client.GetAsync($"{this.authClientUrl}/roles/{grain}/{securableItem}/{name}");
            roleResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await roleResponse.Content.ReadAsStringAsync());
        }

        public async Task<bool> AddPermissionToRole(dynamic permission, dynamic role)
        {
            var rolePermissionResponse = await client.PostAsync($"{this.authClientUrl}/roles/{role.id}/permissions", CreateJsonContent(new[] { permission }));
            rolePermissionResponse.EnsureSuccessStatusCode();
            return rolePermissionResponse.IsSuccessStatusCode;
        }

        public async Task<bool> AddRoleToGroup(dynamic role, string groupName)
        {
            var groupUrlStub = WebUtility.UrlEncode(groupName);
            var groupRoleResponse = await client.PostAsync($"{this.authClientUrl}/groups/{groupUrlStub}/roles", CreateJsonContent(role));
            groupRoleResponse.EnsureSuccessStatusCode();
            return groupRoleResponse.IsSuccessStatusCode;
        }

        public async Task<dynamic> GetGroupByName(string groupName, CancellationToken cancellationToken)
        {
            var groupUrlStub = WebUtility.UrlEncode(groupName);
            var groupResponse = await client.GetAsync($"{this.authClientUrl}/groups/{groupUrlStub}/roles", cancellationToken);
            groupResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject(await groupResponse.Content.ReadAsStringAsync());
        }

        public async Task<UserPermissions> GetPermissionsForUser(string grain, string securableItem)
        {
            var permissionResponse = await client.GetAsync($"{this.authClientUrl}/user/permissions?grain={grain}&securableItem={securableItem}");
            permissionResponse.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<UserPermissions>(await permissionResponse.Content.ReadAsStringAsync());
        }

        internal async Task<Group> GetGroupUsers(string groupName)
        {
            var groupUrlStub = WebUtility.UrlEncode(groupName);
            var groupResponse = await client.GetAsync($"{this.authClientUrl}/groups/{groupUrlStub}/users");
            groupResponse.EnsureSuccessStatusCode();
            var json = await groupResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Group>(json);
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

    public class User
    {
        public string subjectId { get; set; }
        public string identityProvider { get; set; }
        public List<string> groups { get; set; }
    }

    public class Group
    {
        public string id { get; set; }
        public string identifier { get; set; }
        public string groupName { get; set; }
        public string groupSource { get; set; }
        public List<User> users { get; set; }
    }
}