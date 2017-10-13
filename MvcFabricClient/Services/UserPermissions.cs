namespace MvcFabricClient.Services
{
    using System.Collections.Generic;
    public class UserPermissions
    {
        public IEnumerable<string> Permissions { get; set; }
        public string RequestedGrain { get; set; }
        public string RequestedSecurableItem { get; set; }
    }
}