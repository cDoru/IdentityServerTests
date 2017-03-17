﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BrockAllen.MembershipReboot;
using Microsoft.Owin;
using MRTutorial.IdentityServer;
using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.EntityFramework;
using Thinktecture.IdentityServer.MembershipReboot;

[assembly: OwinStartup(typeof(Startup))]

namespace MRTutorial.IdentityServer
{
    public class Startup
    {
        private const string MembershipConnectionString =
            @"Data Source=(localdb)\v11.0; Initial Catalog=TestMembershipReboot; Integrated Security=True; MultipleActiveResultSets=True;";
        private const string IdentityConnectionString =
            @"Data Source=(localdb)\v11.0; Initial Catalog=TestMembershipReboot; Integrated Security=True; MultipleActiveResultSets=True;";
        //https://localhost:44301/identity/.well-known/openid-configuration

        public void Configuration(IAppBuilder app)
        {
            Membership.Membership membership = new Membership.Membership(MembershipConnectionString);
            MembershipRebootUserService<UserAccount> userService = new MembershipRebootUserService<UserAccount>(membership.UserService);
            
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = IdentityConnectionString,
                Schema = "mreboot",
                //Schema = "someSchemaIfDesired"
            };

            ConfigureClients(Clients.Get(), efConfig);

            var factory = new IdentityServerServiceFactory();
            factory.UserService = new Registration<IUserService>(userService); 
            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);
            var cleanup = new TokenCleanup(efConfig, 10);
            cleanup.Start();

            var options = new IdentityServerOptions
            {
                SiteName = "Embedded IdentityServer",
                SigningCertificate = LoadCertificate(),
                Factory = factory
            };

            app.Map("/identity", idsrvApp => idsrvApp.UseIdentityServer(options));
        }

        public static void ConfigureClients(IEnumerable<Client> clients, EntityFrameworkServiceOptions options)
        {
            using (var db = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Clients.Any())
                {
                    foreach (var c in clients)
                    {
                        var e = c.ToEntity();
                        db.Clients.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}
