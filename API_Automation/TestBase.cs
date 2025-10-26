using API_Automation.Client;
using API_Automation.Utils;
using System.Threading.Tasks;

namespace API_Automation
{
    public class TestBase
    {
        protected readonly RestApiClient _apiClient = new(ConfigReader.GetBaseUrl("bookstore"));
        protected string _token;
        protected string _userId;

        public async Task DeleteUser()
        {
            if (!string.IsNullOrEmpty(_userId))
            {
                var client = new RestApiClient(ConfigReader.GetBaseUrl("bookstore"));
                await client.DeleteAsync($"/Account/v1/User/{_userId}", _token);
            }
        }
    }
}
