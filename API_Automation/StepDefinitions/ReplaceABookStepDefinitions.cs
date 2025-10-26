using API_Automation.Client;
using API_Automation.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace API_Automation.Steps
{
    [Binding]
    public class ReplaceBookSteps
    {
        private readonly RestApiClient _client = new(ConfigReader.GetBaseUrl("bookstore"));
        private string _userId = string.Empty;
        private string _token = string.Empty;
        private JObject _books;

        [Given("A user is created and authorized")]
        public async Task GivenAUserIsCreatedAndAuthorized()
        {
            var username = TestDataGenerator.RandomUsername();
            var password = TestDataGenerator.RandomPassword();

            var userResponse = await _client.CreateUser(username, password);
            Assert.That(userResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created),
                $"Expected 201 Created but got {userResponse.StatusCode}. Response: {userResponse.Content}");

            var userObj = JObject.Parse(userResponse.Content);
            _userId = userObj["userID"]!.ToString();

            _token = await _client.GenerateToken(username, password);
            Assert.That(_token, Is.Not.Null, "Token was not generated");
        }

        [When("I get all books")]
        public async Task WhenIGetAllBooks()
        {
            var response = await _client.GetAsync("/BookStore/v1/Books");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK),
                $"Expected 200 but got {response.StatusCode}");
            _books = JObject.Parse(response.Content);
        }

        [When("I add the first book to user's list")]
        public async Task WhenIAddTheFirstBookToUserSList()
        {
            var firstIsbn = _books["books"]![0]!["isbn"]!.ToString();
            var body = new
            {
                userId = _userId,
                collectionOfIsbns = new[] { new { isbn = firstIsbn } }
            };

            var response = await _client.PostAsync("/BookStore/v1/Books", body, _token);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created),
                $"Expected 201 Created but got {response.StatusCode}. Response: {response.Content}");
        }

        [Then("User has only one book and it matches the added one")]
        public async Task ThenUserHasOnlyOneBookAndItMatchesTheAddedOne()
        {
            var response = await _client.GetAsync($"/Account/v1/User/{_userId}", _token);
            var userData = JObject.Parse(response.Content);

            Assert.That(userData["books"]!.Count(), Is.EqualTo(1), "Unexpected number of books");

            string expectedIsbn = _books["books"]![0]!["isbn"]!.ToString();
            string actualIsbn = userData["books"]![0]!["isbn"]!.ToString();
            Assert.That(actualIsbn, Is.EqualTo(expectedIsbn), "Book does not match the one added");
        }

        [When("I replace the book with the second one")]
        public async Task WhenIReplaceTheBookWithTheSecondOne()
        {
            string firstIsbn = _books["books"]![0]!["isbn"]!.ToString();
            string secondIsbn = _books["books"]![1]!["isbn"]!.ToString();

            // Delete first book
            var deleteResponse = await _client.DeleteBookAsync(_userId, firstIsbn, _token);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent),
                $"Expected 204 NoContent but got {deleteResponse.StatusCode}. Response: {deleteResponse.Content}");

            // Add second book
            var body = new
            {
                userId = _userId,
                collectionOfIsbns = new[] { new { isbn = secondIsbn } }
            };

            var response = await _client.PostAsync("/BookStore/v1/Books", body, _token);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created),
                $"Expected 201 Created but got {response.StatusCode}. Response: {response.Content}");
        }

        [Then("The user's book list contains only the replaced book")]
        public async Task ThenTheUserSBookListContainsOnlyTheReplacedBook()
        {
            var response = await _client.GetAsync($"/Account/v1/User/{_userId}", _token);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            var userData = JObject.Parse(response.Content);
            string actualIsbn = userData["books"]![0]!["isbn"]!.ToString();
            string expectedIsbn = _books["books"]![1]!["isbn"]!.ToString();

            Assert.That(actualIsbn, Is.EqualTo(expectedIsbn), "Book was not replaced correctly");
        }

        [AfterScenario]
        public async Task Cleanup()
        {
            if (!string.IsNullOrEmpty(_userId))
            {
                var client = new RestApiClient(ConfigReader.GetBaseUrl("bookstore"));
                var response = await client.DeleteAsync($"/Account/v1/User/{_userId}", _token);

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent ||
                    response.StatusCode == System.Net.HttpStatusCode.OK)
                    TestContext.Out.WriteLine($"User {_userId} deleted successfully ({response.StatusCode}).");
                else
                    TestContext.Out.WriteLine($"Failed to delete user {_userId}. Status: {response.StatusCode}, Response: {response.Content}");
            }
        }
    }
}
