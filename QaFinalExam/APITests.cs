using NUnit.Framework;
using RestSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace QaFinalExam
{
    public class APITests
    {
        public static string _baseUrl = @"https://contactbook.evgenidimitrov0.repl.co/api";

        [SetUp]
        public void SetUp()
        {
        }

        //Листнете всички контакти и проверете дали 3-тия контакт е с фамилия Hathaway.
        [Test]
        public void TestListAllContactsAndCheckThirdOneForHathaway()
        {
            var client = new RestClient(_baseUrl + "/contacts");
            var requst = new RestRequest("", Method.Get);

            var result = client.ExecuteAsync(requst).ConfigureAwait(false).GetAwaiter().GetResult();
            var castToContacts = JsonConvert.DeserializeObject<List<Contact>>(result.Content);

            Assert.That(castToContacts[2].LastName == "Hathaway");
        }

        //Търсете контакт по ключова дума „dimitar“ и проверете, че първият резултат съдържа име и фамилия Dimitar Berbatov.
        [Test]
        public void TestSearchForDimitarResult()
        {
            var client = new RestClient(_baseUrl + "/contacts/search/dimitar");
            var requst = new RestRequest("", Method.Get);

            var result = client.ExecuteAsync(requst).ConfigureAwait(false).GetAwaiter().GetResult();
            var castToContacts = JsonConvert.DeserializeObject<List<Contact>>(result.Content);
            var firstResult = castToContacts[0];
            Assert.That($"{firstResult.FirstName} {firstResult.LastName}" == "Dimitar Berbatov");
        }

        //Търсете по ключова дума „missing{random}“, където {random} е случайно генерирано число и проверете, че върнатия резултат е празен.
        [Test]
        public void TestRandomSearch()
        {
            var random = new Random();
            var client = new RestClient(_baseUrl + $"/contacts/{random.Next(300, 1000)}");
            var requst = new RestRequest("", Method.Get);

            var result = client.ExecuteAsync(requst).ConfigureAwait(false).GetAwaiter().GetResult();
            var errorMessage = JsonConvert.DeserializeObject<ErrorMessageObject>(result.Content);

            Assert.That(errorMessage is not null);
        }

        //Опитайте да създадете контакт с невалидни данни и проверете, че се връща грешка.
        [Test]
        public void TestCreateInvalidUser()
        {
            var client = new RestClient(_baseUrl + "/contacts");
            var request = new RestRequest("", Method.Post);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new { firstName = "foo", lastName = "bar", email = "123test", comments = "empty", phone = "1234" });

            var result = client.ExecuteAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
            var errorMessage = JsonConvert.DeserializeObject<ErrorMessageObject>(result.Content);

            Assert.That(errorMessage.ErrorMessage == "Invalid email!");
        }

        //Създайте нов контакт с верни данни и проверете, дали той се е добавил успешно.
        [Test]
        public void TestCreateValidUser()
        {
            var client = new RestClient(_baseUrl + "/contacts");
            var request = new RestRequest("", Method.Post);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new { firstName = "Monster2", lastName = "QA", email = "monster@qaemail.com", comments = "QA best uni course!", phone = "123456789" });

            var result = client.ExecuteAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.That(result.StatusCode == HttpStatusCode.Created);
        }

        //Създайте нов контакт с верни данни, изтрийте новосъздадения контакт и проверете, че няма такъв контакт при всички контакти.
        [Test]
        public void TestCreateValidUserAndDeleteAndMAkeSureHeDoesntExist()
        {
            //Create new user
            CreateUser("GoToDeleteFolder", "Deleted", "will@email.com", "Sorry I will go...", "123456789");

            //Get all users
            var client = new RestClient(_baseUrl + "/contacts");
            var request = new RestRequest("", Method.Get);
            var result = client.ExecuteAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
            var castToContacts = JsonConvert.DeserializeObject<List<Contact>>(result.Content);

            //Find last created user
            var idOfContactToBeDeleted = castToContacts.Last().Id;

            var deleteClient = new RestClient(_baseUrl + $"/contacts/{idOfContactToBeDeleted}");
            var deleteRequest = new RestRequest("/", Method.Delete);
            deleteClient.ExecuteAsync(deleteRequest).ConfigureAwait(false).GetAwaiter().GetResult();

            //Look for the newly created user
            var lookClient = new RestClient(_baseUrl + "/contacts/search/GoToDeleteFolder");
            var lookRequest = new RestRequest("", Method.Get);

            var lookResult = lookClient.ExecuteAsync(lookRequest).ConfigureAwait(false).GetAwaiter().GetResult();
            var convertedAllUsersAfterDeletion = JsonConvert.DeserializeObject<List<Contact>>(lookResult.Content);

            Assert.That(convertedAllUsersAfterDeletion.Count == 0);
        }

        private void CreateUser(string firstName, string lastName, string email, string comments, string phone)
        {
            var client = new RestClient(_baseUrl + "/contacts");
            var request = new RestRequest("", Method.Post);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new { firstName = firstName, lastName = lastName, email = email, comments = comments, phone = phone });

            client.ExecuteAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
    public class Contact
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }
    }

    public class ErrorMessageObject
    {
        [JsonProperty("errMsg")]
        public string ErrorMessage { get; set; }
    }
}
