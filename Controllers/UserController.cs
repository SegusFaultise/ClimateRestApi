
using CLIMATE_REST_API.Models;
using CLIMATE_REST_API.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CLIMATE_REST_API.Controllers
{
    #region Controller & Route -Variables
    [Controller]
    [Route("~/api/[controller]")]
    #endregion

    #region Weather Controller
    public class UserController : Controller
    {
        #region region MongoDBServices Variable
        private readonly MongoDBServices _mongodbServices;
        #endregion

        #region Setting _mongodbServices To mongodbServices
        public UserController(MongoDBServices mongodbServices)
        {
            _mongodbServices = mongodbServices;
        }
        #endregion

        #region Http Get Users
        [EnableCors]
        [HttpGet]
        public async Task<List<UserModel>> GetAllUsers()
        {
            return await _mongodbServices.GetUserAsync();
        }
        #endregion

        #region Http Authenticate Users
        [EnableCors]
        [HttpGet]
        private async Task<bool> AuthenticateUser(string api_token, string role)
        {
            if (_mongodbServices.AuthenticateUserAsync(api_token, role) == null)
            {
                return false;
            }

            await _mongodbServices.GetUserAsync();
            return true;
        }
        #endregion

        #region Http Post Users
        [EnableCors]
        [HttpGet]
        public async Task<IActionResult> CreateUser(UserModel user_model)
        {
            return await _mongodbServices.CreatedUserAsync(user_model);
        }
        #endregion

        #region Http Delete User By Id
        [EnableCors]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SeleteUserById(string id)
        {
            await _mongodbServices.DeleteUserByIdAsync(id);
            return NoContent();
        }
        #endregion
    }
    #endregion
}
