using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using QuickstartIdentityServer.Quickstart.Interface;

namespace QuickstartIdentityServer.Quickstart.Store
{
    public class CustomResourceStore : IResourceStore
    {
        protected IRepository _dbRepository;
        protected readonly ILogger<CustomResourceStore> Logger;

        public CustomResourceStore(IRepository repository, ILogger<CustomResourceStore> logger)
        {
            _dbRepository = repository;
            Logger = logger;
        }

        private IEnumerable<ApiResource> GetAllApiResources()
        {
            return _dbRepository.All<ApiResource>();
        }

        private IEnumerable<IdentityResource> GetAllIdentityResources()
        {
            return _dbRepository.All<IdentityResource>();
        }

        public Task<ApiResource> FindApiResourceAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            return Task.FromResult(_dbRepository.Single<ApiResource>(a => a.Name == name));
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var list = _dbRepository.Where<ApiResource>(a => a.Scopes.Any(s => scopeNames.Contains(s)));

            return Task.FromResult(list.AsEnumerable());
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var list = _dbRepository.Where<IdentityResource>(e => scopeNames.Contains(e.Name));

            return Task.FromResult(list.AsEnumerable());
        }

       
        private Func<IdentityResource, bool> BuildPredicate(Func<IdentityResource, bool> predicate)
        {
            return predicate;
        }

        /// <summary>
        /// Gets all resources.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Resources> GetAllResourcesAsync()
        {
            var identity = _dbRepository.All<IdentityResource>();

            var apis = _dbRepository.All<ApiResource>();


            var scopes = _dbRepository.All<ApiScope>();


            //var result = new Resources(
            //    ( identity.ToArray()).Select(x => x.ToModel()),
            //    (apis.ToArray()).Select(x => x.ToModel()),
            //    (scopes.ToArray()).Select(x => x.ToModel())
            //);
            var result = new Resources(
               (identity.ToArray()).Select(x => x),
               (apis.ToArray()).Select(x => x),
               (scopes.ToArray()).Select(x => x)
           );

            Logger.LogDebug("Found {scopes} as all scopes, and {apis} as API resources",
                result.IdentityResources.Select(x => x.Name).Union(result.ApiScopes.Select(x => x.Name)),
                result.ApiResources.Select(x => x.Name));

            return result;
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var scopes = scopeNames.ToArray();

            var query = _dbRepository.Where<IdentityResource>(x=> scopes.Contains(x.Name));
                
            //var resources = query
            //    .Include(x => x.UserClaims)
            //    .Include(x => x.Properties)
            //    .AsNoTracking();

            var results = query.ToArray();

            Logger.LogDebug("Found {scopes} identity scopes in database", results.Select(x => x.Name));

            //return results.Select(x => x.ToModel()).ToArray();
            return Task.FromResult(results.AsEnumerable());
        }

        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            var scopes = scopeNames.ToArray();

            var query =
                 _dbRepository.Where<ApiScope>(x=> scopes.Contains(x.Name));
                

          
            var results = query.ToArray();

            Logger.LogDebug("Found {scopes} scopes in database", results.Select(x => x.Name));

            //return results.Select(x => x.ToModel()).ToArray();
            return Task.FromResult(results.AsEnumerable());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var names = scopeNames.ToArray();

            var query = _dbRepository.Where<ApiResource>(x => names.Contains(x.Name));



            var results =  query.ToArray();
            //var models = results.Select(x => x.ToModel()).ToArray();

            Logger.LogDebug("Found {apis} API resources in database", results.Select(x => x.Name));

            //return models;
            return Task.FromResult(results.AsEnumerable());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            if (apiResourceNames == null) throw new ArgumentNullException(nameof(apiResourceNames));

            var query =
                 _dbRepository.Where<ApiResource>(x=> apiResourceNames.Contains(x.Name));
              

            //var result = (query.ToArray()).Select(x => x.ToModel()).ToArray();

            if (query.Any())
            {
                Logger.LogDebug("Found {apis} API resource in database", query.Select(x => x.Name));
            }
            else
            {
                Logger.LogDebug("Did not find {apis} API resource in database", apiResourceNames);
            }

            return Task.FromResult( query.AsEnumerable());
        }
    }
}
