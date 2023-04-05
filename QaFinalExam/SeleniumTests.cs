using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QaFinalExam
{
    public class Tests
    {
        private IWebDriver _driver;

        [SetUp]
        public void SetUp()
        {
            _driver = new ChromeDriver();
            _driver.Navigate().GoToUrl("https://contactbook.evgenidimitrov0.repl.co/");
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        //Листнете всички контакти и проверете, дали първият контакт е с име Elon.
        [Test]
        public void TestCheckIfFirstContactIsElon()
        {
            var allContacts = GetAllContacts();
            var firstContact = allContacts.FirstOrDefault();
            Assert.That(firstContact.FindElement(By.ClassName("fname")).FindElement(By.TagName("td")).Text == "Elon");
        }

        //Търсете контакт по ключова дума “dimitar” и проверете дали първият резултат съдържа Dimitar Berbatov.
        [Test]
        public void TestLookForDimitar()
        {
            var allContacts = GetAllContacts();
            var resultedContacts = new List<IWebElement>();

            foreach (var contact in allContacts)
            {
                var name = contact.FindElement(By.ClassName("fname")).FindElement(By.TagName("td"));

                if (name.Text.ToLower().Contains("dimitar"))
                {
                    resultedContacts.Add(contact);
                }
            }

            var firstAndLastNameOfFirstEntry = $"{resultedContacts.FirstOrDefault().FindElement(By.ClassName("fname")).FindElement(By.TagName("td")).Text} " +
                $"{resultedContacts.FirstOrDefault().FindElement(By.ClassName("lname")).FindElement(By.TagName("td")).Text}";

            Assert.That(firstAndLastNameOfFirstEntry.Contains("Dimitar Berbatov"));

        }

        //Търсете контакт по ключова дума „Invalid key word 123“ и проверете, че резултатът е празен.
        [Test]
        public void LookForInvalidWord()
        {
            var allContacts = GetAllContacts();
            var resultedContacts = new List<IWebElement>();

            foreach (var contact in allContacts)
            {
                var firstAndLastCurrentNames = $"{contact.FindElement(By.ClassName("fname")).FindElement(By.TagName("td")).Text} " +
                                $"{contact.FindElement(By.ClassName("lname")).FindElement(By.TagName("td")).Text}";

                if (firstAndLastCurrentNames.ToLower().Contains(new string("Invalid key word 123").ToLower()))
                {
                    resultedContacts.Add(contact);
                }
            }
            Assert.That(resultedContacts.Count(), Is.EqualTo(0));
        }

        //Направете опит за създаване на нов контакт с грешни данни и проверете, дали се показва грешка на екрана.
        [Test]
        public void CreateInvalidUser()
        {
            NavigateToCreationOfUser();
            var firstName = _driver.FindElement(By.Id("firstName"));
            var lastName = _driver.FindElement(By.Id("lastName"));
            var email = _driver.FindElement(By.Id("email"));
            var phone = _driver.FindElement(By.Id("phone"));
            var comments = _driver.FindElement(By.Id("comments"));
            var submitButton = _driver.FindElement(By.Id("create"));

            firstName.SendKeys("Test");
            lastName.SendKeys("Testov");
            email.SendKeys("TestEmailJailbreak");
            phone.SendKeys("breakingValidation");
            comments.SendKeys("'OR 1=1");

            submitButton.Click();

            var error = _driver.FindElement(By.ClassName("err"));

            Assert.That(error, Is.Not.Null);
        }

        //Създайте нов контакт с верни данни и проверете, дали той се е добавил успешно.
        [Test]
        public void CreateValidUser()
        {
            NavigateToCreationOfUser();
            var firstName = _driver.FindElement(By.Id("firstName"));
            var lastName = _driver.FindElement(By.Id("lastName"));
            var email = _driver.FindElement(By.Id("email"));
            var phone = _driver.FindElement(By.Id("phone"));
            var comments = _driver.FindElement(By.Id("comments"));
            var submitButton = _driver.FindElement(By.Id("create"));

            firstName.SendKeys("Jeff");
            lastName.SendKeys("Bezos");
            email.SendKeys("jeff@choccymilk.com");
            phone.SendKeys("6594206969");
            comments.SendKeys("I am rich!");

            submitButton.Click();

            Assert.That(_driver.Url == "https://contactbook.evgenidimitrov0.repl.co/contacts");
        }

        private void NavigateToContacts()
        {
            var viewContactsButton = _driver.FindElement(By.XPath("/html/body/main/div/a[1]"));
            viewContactsButton.Click();
        }

        private void NavigateToCreationOfUser()
        {
            var createContactButton = _driver.FindElement(By.XPath("/html/body/main/div/a[2]"));
            createContactButton.Click();
        }

        private List<IWebElement> GetAllContacts()
        {
            NavigateToContacts();
            var resultingList = new List<IWebElement>();
            var allContacts = _driver.FindElements(By.ClassName("contact-entry"));
            foreach (var contact in allContacts)
            {
                resultingList.Add(contact);
            }
            return resultingList;
        }

        [TearDown]
        public void TearDown()
        {
            _driver.Close();
        }
    }
}