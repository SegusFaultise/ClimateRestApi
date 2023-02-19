
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

        #region Authenticate Users
        private async Task<bool> AuthenticateUser(string api_token, string role)
        {
            var auth = await _mongodbServices.AuthenticateUserAsync(api_token, role);

            if (auth == null)
            {
                return false;
            }

            await _mongodbServices.UpdateUserLoginTimeAsync(api_token, DateTime.Now);
            return true;
        }
        #endregion

        #region Http Post Users
        [EnableCors]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel user_model)
        {
            await _mongodbServices.CreatedUserAsync(user_model);
            return CreatedAtAction(nameof(GetAllUsers), new { id = user_model.Id }, user_model); ;
        }
        #endregion

        #region Http Delete User By Id
        [EnableCors]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SeleteUserById(string api_token, string id)
        {
            if (AuthenticateUser(api_token, "Admin").Result == false)
            {
                return Unauthorized("Unauthorized");
            }
            await _mongodbServices.DeleteUserByIdAsync(api_token, id);
            return Ok("User deleted successfully");
        }
        #endregion
    }
    #endregion
}
