using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MyMoviesCatalogApp.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser//<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() { }
        public ApplicationRole(string roleName) : base(roleName) { }
        public string Description { get; set; }
    }

    //public class ApplicationUserRole : IdentityUserRole<int> { }
    //public class ApplicationUserClaim : IdentityUserClaim<int> { }
    //public class ApplicationUserLogin : IdentityUserLogin<int> { }

    //public class ApplicationRole : IdentityRole<int, ApplicationUserRole>
    //{
    //    public ApplicationRole() { }
    //    public ApplicationRole(string name) { Name = name; }
    //}

    //public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    //{
    //    public ApplicationUserStore(ApplicationDbContext context)
    //        : base(context)
    //    {
    //    }
    //}

    //public class ApplicationRoleStore : RoleStore<ApplicationRole, int, ApplicationUserRole>
    //{
    //    public ApplicationRoleStore(ApplicationDbContext context)
    //        : base(context)
    //    {
    //    }
    //}

    //public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole,
    //int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    //{
    //    public ApplicationDbContext()
    //        : base("DefaultConnection")
    //    {
    //    }

    //    public static ApplicationDbContext Create()
    //    {
    //        return new ApplicationDbContext();
    //    }
    //}

    //public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    //{
    //    public ApplicationDbContext()
    //        : base("DefaultConnection", throwIfV1Schema: false)
    //    {
    //    }

    //    public static ApplicationDbContext Create()
    //    {
    //        return new ApplicationDbContext();
    //    }


    //}
}