
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
        /// <summary>
        /// Gets all the user from the database
        /// </summary>
        /// <returns></returns>
        [EnableCors]
        [HttpGet]
        public async Task<List<UserModel>> GetAllUsers()
        {
            return await _mongodbServices.GetUserAsync();
        }
        #endregion

        #region Authenticate Users
        /// <summary>
        /// Authenticates the user to allow certian actions
        /// </summary>
        /// <param name="api_token"></param>
        /// <param name="role"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Creates a single user and can only be performed by an admin
        /// </summary>
        /// <param name="user_model"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel user_model, string api_token)
        {
            if (AuthenticateUser(api_token, "Admin").Result == false)
            {
                return Unauthorized("Unauthorized");
            }

            await _mongodbServices.CreatedUserAsync(user_model);
            return CreatedAtAction(nameof(GetAllUsers), new { id = user_model.Id }, user_model); ;
        }
        #endregion

        #region Http Delete User By Id
        /// <summary>
        /// Deletes the user by id and can only be performed by an admin
        /// </summary>
        /// <param name="api_token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserById(string api_token, string id)
        {
            if (AuthenticateUser(api_token, "Admin").Result == false)
            {
                return Unauthorized("Unauthorized");
            }
            await _mongodbServices.DeleteUserByIdAsync(api_token, id);
            return Ok("User deleted");
        }
        #endregion

        #region Http Patch User Email & Role

        [EnableCors]
        [HttpPatch("{start_date} {end_date}")]
        public async Task<IActionResult> PatchUsers(string api_token, string email, string role, DateTime start_date, DateTime end_date)
        {
            //if (AuthenticateUser(api_token, "Admin").Result == false)
            //{
            //    return Unauthorized("Unauthorized");
            //}

            await _mongodbServices.UpdateUserEmailAsync(api_token, email, start_date, end_date);
            await _mongodbServices.UpdateUserRoleAsync(api_token, role);
            return Ok();
        }
        #endregion

        //    #region Http Patch Users Role
        //    [EnableCors]
        //    [HttpPatch]
        //    [Route("PatchUsers")]
        //    public async Task<ActionResult> UpdateUsersAsync(UserPatchModel patch_model)
        //    {
        //        try
        //        {
        //            List<UserModel> user = new List<UserModel>();

        //            var exception = user[3];

        //            bool succeeded = false;
        //            switch (patch_model.PropertyName)
        //            {
        //                case "Role":
        //                    succeeded = await _mongodbServices.UpdateManyRoleAsync(patch_model.Filter, patch_model.PropertyValue);
        //                    break;
        //                case "Email":
        //                    succeeded = await _mongodbServices.UpdateManyEmailAsync(patch_model.Filter, patch_model.PropertyValue);
        //                    break;
        //                default:
        //                    break;
        //            }

        //            if (succeeded)
        //            {
        //                return Ok("Updated Successfully");
        //            }
        //            else
        //            {
        //                return BadRequest("No properties matched, or no properties updated");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            return Problem(e.Message, statusCode: 500);
        //        }
        //    }
        //    #endregion
    }
    #endregion
}
