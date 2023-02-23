#region Imports
using CLIMATE_REST_API.Models;
using CLIMATE_REST_API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
#endregion

namespace CLIMATE_REST_API.Controllers
{
    #region Tea Pot Controller
    public class TeaPotController : Controller
    {
        #region region MongoDBServices Variable
        private readonly MongoDBServices _mongodbServices;
        #endregion

        #region Setting _mongodbServices To mongodbServices
        public TeaPotController(MongoDBServices mongodbServices)
        {
            _mongodbServices = mongodbServices;
        }
        #endregion

        #region Get A Tea Pot
        /// <summary>
        /// I am the tea pot and the tea pot is me
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ImATeaPot")]
        public async Task<IActionResult> ImATeaPot(TeaPotModel tea_pot_model)
        {
            var result = await _mongodbServices.GetTeaPotAsync(tea_pot_model);
            return Ok(result);
        }
        #endregion
    }
    #endregion
}
