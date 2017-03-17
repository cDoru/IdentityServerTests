using System;
using System.Security.Authentication;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.WebHost;

namespace MRTutorial.Membership
{
    public class Membership
    {
        public Membership(string connectionString)
        {
            Database = new DefaultMembershipRebootDatabase(connectionString);
            UserRepository = new DefaultUserAccountRepository(Database);
            UserService = new UserAccountService(UserRepository);
            AuthService = new SamAuthenticationService(UserService);

            UserService.Configuration.RequireAccountVerification = false;
        }

        public UserAccount Account { get; private set; }
        public DefaultMembershipRebootDatabase Database { get; private set; }
        public IUserAccountRepository UserRepository { get; private set; }
        public UserAccountService UserService { get; private set; }
        public AuthenticationService AuthService { get; private set; }


        public void CreateAccount(string username, string password, string email)
        {
            Account = UserService.CreateAccount(username, password, email);
        }

        public void DeleteAccount()
        {
            if (Account != null)
            {
                UserService.DeleteAccount(Account.ID);
                Account = null;
            }
        }

        public bool Authenticate(string usernameOrEmail, string password)
        {
            UserAccount account;
            AuthenticationFailureCode failureCode;

            bool authenticated = Account != null;
            if (!authenticated &&
                UserService.AuthenticateWithUsernameOrEmail(usernameOrEmail, password, out account, out failureCode))
            {
                Account = account;
                return true;
            }

            return authenticated;
        }

        public void SignIn(string usernameOrEmail, string password, bool rememberMe = false)
        {
            UserAccount account;
            AuthenticationFailureCode failureCode;

            bool authenticated = Authenticate(usernameOrEmail, password);
            if (authenticated)
            {
                AuthService.SignIn(Account, rememberMe);

                if (Account.RequiresTwoFactorAuthCodeToSignIn())
                {
                    throw new NotImplementedException();
                }
                if (Account.RequiresTwoFactorCertificateToSignIn())
                {
                    throw new NotImplementedException();
                }

                if (Account.RequiresPasswordReset)
                {
                    // this might mean many things -- 
                    // it might just mean that the user should change the password, 
                    // like the expired password below, so we'd just redirect to change password page
                    // or, it might mean the DB was compromised, so we want to force the user
                    // to reset their password but via a email token, so we'd want to 
                    // let the user know this and invoke ResetPassword and not log them in
                    // until the password has been changed
                    //userAccountService.ResetPassword(account.ID);

                    // so what you do here depends on your app and how you want to define the semantics
                    // of the RequiresPasswordReset property
                    throw new NotImplementedException();
                }

                if (UserService.IsPasswordExpired(Account))
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new AuthenticationException("Authentication failed.");
            }
        }
    }
}
