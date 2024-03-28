using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            if (AgeCheck(dateOfBirth))
            {
                return false;
            }
            
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            var user = CreateUser(firstName, lastName, email, dateOfBirth, clientId);
                
            if (CreditLimitCheck(user, client))
            {
                return false;
            }
            UserDataAccess.AddUser(user);
            return true;
        }

        private static bool CreditLimitCheck(User user, Client client)
        {
            switch (client.Type)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                    using (var userCreditService = new UserCreditService())
                    {
                        var creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                        creditLimit = creditLimit * 2;
                        user.CreditLimit = creditLimit;
                    }
                    break;
                default:
                    user.HasCreditLimit = true;
                    using (var userCreditService = new UserCreditService())
                    {
                        var creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                        user.CreditLimit = creditLimit;
                    }
                    break;
            }
            return user.HasCreditLimit && user.CreditLimit < 500;
            
        }
        private static User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            return user;
        }
        private static bool AgeCheck(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            var age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }
            return age < 21;
        }
    }
}
