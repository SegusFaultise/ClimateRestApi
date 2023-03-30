#region Imports
using CLIMATE_REST_API.Models;
using CLIMATE_REST_API.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
#endregion

namespace CLIMATE_REST_API.Controllers
{
    #region Controller & Route -Variables
    [Controller]
    [Route("~/api/[controller]")]
    #endregion

    #region User Controller
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
        /// <param name="api_token"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpPost]
        [Route("{api_token} PostUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserModel user_model, string api_token)
        {
            if (AuthenticateUser(api_token, "Admin").Result == false)
            {
                return Unauthorized("Unauthorized");
            }

            await _mongodbServices.CreatedUserAsync(user_model);

            return CreatedAtAction(nameof(GetAllUsers), new {id = user_model.Id }, user_model); ;
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
        [HttpDelete]
        [Route(@"{id} {api_token} DeleteUser")]
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

        #region Http Deletes Users
        /// <summary>
        /// Deletes users if they havent logedin for 30 days can only be performed by an admin
        /// </summary>
        /// <param name="api_token"></param>
        /// <param name="date_time_start"></param>
        /// <param name="date_time_end"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpDelete]
        [Route(@"{api_token} {date_time_start} 
                 {date_time_end} DeleteManyUsers")]
        public async Task<IActionResult> DeleteManyUsers(string api_token, DateTime date_time_start, DateTime date_time_end)
        {
            if (AuthenticateUser(api_token, "Admin").Result == false)
            {
                return Unauthorized("Unauthorized");
            }

            await _mongodbServices.DeletesManyUsersAsync(api_token, date_time_start, date_time_end);

            return Ok("Users deleted");
        }
        #endregion

        #region Http Patch Users Role
        /// <summary>
        /// Updates a users role based on a date range in which they were created
        /// </summary>
        /// <param name="patch_model"></param>
        /// <param name="date_time_start"></param>
        /// <param name="date_time_end"></param>
        /// <param name="api_token"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpPatch]
        [Route(@"{api_token} {date_time_start}
                 {date_time_end} PatchUsers")]
        public async Task<IActionResult> UpdateUsersAsync([FromBody] JsonPatchDocument<List<UserModel>> patch_model, 
                                                                                                        DateTime date_time_start, 
                                                                                                        DateTime date_time_end, 
                                                                                                        string api_token)
        {
            var operation = patch_model.Operations.FirstOrDefault();

            try
            {
                if (patch_model == null)
                {
                    return BadRequest();
                }

                if (AuthenticateUser(api_token, "Admin").Result == false)
                {
                    return Unauthorized("Unauthorized");
                }

                if (date_time_start > date_time_end || date_time_end < date_time_start)
                {
                    return BadRequest("Do you have a time travel machine?");
                }

                else
                {
                    await _mongodbServices.PatchUsersRoleAsync(operation.path = "role", operation.value, date_time_start, date_time_end);

                    return Ok("Users roles have been updated");
                }
            }
            catch (
            Exception e)
            {
                return Problem(e.Message, statusCode: 500);
            }
        }
        #endregion
    }
    #endregion
}
