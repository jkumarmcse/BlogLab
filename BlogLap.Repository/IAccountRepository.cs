using BlogLap.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlogLap.Repository
{
    public interface IAccountRepository
    {

        public Task<IdentityResult> CreateAsync(ApplicationUserIdentity user, CancellationToken cancellationToken);

        public Task<ApplicationUserIdentity> GetUsernameAsync(string normalizedUsername, , CancellationToken cancellationToken);
    }
}
