using System.Collections.Generic;
using IdentityServer4.Models;

namespace KnowledgeBase.BackendServer.IdentityServer
{
    public static class Config
    {
        /*Config for Ids*/
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        /*Config for Apis*/
        public static IEnumerable<ApiResource> Apis =>
            new[]
            {
                new ApiResource("api.knowledgebase", "KnowledgeBase API")
            };
        
        /*Config for Scopes Api*/
        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("api.knowledgebase", "KnowledgeBase API")
            };
    }
}