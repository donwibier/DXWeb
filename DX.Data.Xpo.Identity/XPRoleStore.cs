using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Timers;

namespace DX.Data.Xpo.Identity
{
	public class XPRoleStore<TKey, TRole,
							TUserRole, TRoleClaim, TXPORole, TXPOClaim> :
		RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
			where TKey : IEquatable<TKey>
			where TRole : IdentityRole<TKey>
			where TUserRole : IdentityUserRole<TKey>, new()
			where TRoleClaim : IdentityRoleClaim<TKey>, new()
			where TXPORole : XPBaseObject, IXPRole<TKey>
			where TXPOClaim : XPBaseObject, IXPRoleClaim<TKey>
	{
        readonly IQueryableRoleStore<TKey, TRole> _RoleStore;
        readonly IQueryableRoleClaimStore<TKey, TRoleClaim> _ClaimStore;

        public XPRoleStore(IQueryableRoleStore<TKey, TRole> roleStore,
            IQueryableRoleClaimStore<TKey, TRoleClaim> roleClaimStore,             
			IdentityErrorDescriber? describer = null) : base(describer ?? new IdentityErrorDescriber())
        {
            _ClaimStore = roleClaimStore;
            _RoleStore = roleStore;
        }
        protected void ThrowIfNull(object? obj)
        {
#if (!NETCOREAPP)
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
#else
            ArgumentNullException.ThrowIfNull(obj);
#endif
        }

        public override IQueryable<TRole> Roles => throw new NotImplementedException();

        public async override Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
			await _RoleStore.AddClaimsAsync(role, claim, cancellationToken);
        }

        public async override Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(role);
            var r = await _RoleStore.CreateAsync(role);
            //TODO: Check appropriate error
            return r.Success ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.DefaultError());
        }

        public async override Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(role);
            //TODO: Check appropriate error
            var r = await _RoleStore.UpdateAsync(role);
            return r.Success ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.DefaultError());
        }

        public async override Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ThrowIfNull(role);
            //TODO: Check appropriate error
            var r = await _RoleStore.DeleteAsync(role);
            return r.Success ? IdentityResult.Success : IdentityResult.Failed(ErrorDescriber.DefaultError());

        }

        public async override Task<TRole?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _RoleStore.FindByIdAsync(id, cancellationToken);			
        }

        public async override Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        {
            return await  _RoleStore.FindByNameAsync(normalizedName, cancellationToken);            
        }

        public async override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
        {
			var results = await _ClaimStore.GetRoleClaimsAsync(role.Id, cancellationToken);
			return results;			
        }

        public async override Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
        {
			await _RoleStore.RemoveClaimAsync(role, claim, cancellationToken);
        }

    }

}
