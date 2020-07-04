using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using QuickstartIdentityServer.Quickstart.Interface;
using Microsoft.Extensions.Logging;
using System;
using IdentityServer4.Extensions;

namespace QuickstartIdentityServer.Quickstart.Store
{
    /// <summary>
    /// Handle consent decisions, authorization codes, refresh and reference tokens
    /// </summary>
    public class CustomPersistedGrantStore : IPersistedGrantStore
    {
        protected readonly IRepository _dbRepository;
        protected readonly ILogger<CustomPersistedGrantStore> logger;

        public CustomPersistedGrantStore(IRepository repository, ILogger<CustomPersistedGrantStore> logger)
        {
            _dbRepository = repository;
            this.logger = logger;
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var result = _dbRepository.Where<PersistedGrant>(i => i.SubjectId == subjectId);
            return Task.FromResult(result.AsEnumerable());
        }

        public  Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var persistedGrants = Filter(filter).ToArray();
            //var model = persistedGrants.Select(x => x.ToModel());

            logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Length, filter);
            return Task.FromResult(persistedGrants.AsEnumerable());
            //return model;
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            var result = _dbRepository.Single<PersistedGrant>(i => i.Key == key);
            return Task.FromResult(result);
        }

        public Task RemoveAllAsync(string subjectId, string clientId)
        {
            _dbRepository.Delete<PersistedGrant>(i => i.SubjectId == subjectId && i.ClientId == clientId);
            return Task.FromResult(0);
        }

        public Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            _dbRepository.Delete<PersistedGrant>(i => i.SubjectId == subjectId && i.ClientId == clientId && i.Type == type);
            return Task.FromResult(0);
        }

        public async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var persistedGrants = Filter(filter).ToArray();

            logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", persistedGrants.Length, filter);

            //Context.PersistedGrants.RemoveRange(persistedGrants);

            try
            {

                _dbRepository.Add<PersistedGrant>(persistedGrants);

            }
            catch (Exception ex)
            {
                logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {@filter}: {error}", persistedGrants.Length, filter, ex.Message);
            }
        }

        public Task RemoveAsync(string key)
        {
            _dbRepository.Delete<PersistedGrant>(i => i.Key == key);
            return Task.FromResult(0);
        }

        public Task StoreAsync(PersistedGrant grant)
        {
            _dbRepository.Add<PersistedGrant>(grant);
            return Task.FromResult(0);
        }

        private IQueryable<PersistedGrant> Filter(PersistedGrantFilter filter)
        {
            var query = _dbRepository.All<PersistedGrant>();

            if (!String.IsNullOrWhiteSpace(filter.ClientId))
            {
                query = query.Where(x => x.ClientId == filter.ClientId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!String.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            return query;
        }
    }
}
