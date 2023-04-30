using Api.Classes;
using Api.Models;
using Database;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Set = Database.Set;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetController : ControllerBase
    {
        // GET: api/Set
        // Getting all the user's sets by his token
        [HttpGet("{token}")]
        public IActionResult GetAllSets(string token)
        {
            TokenClass tokenCL = new TokenClass();
            if (!tokenCL.ValidateToken(token))
            {
                return Unauthorized(new Response(false, null, "The user is not logged in"));
            }

            TokenDatabase TDB = new TokenDatabase();
            string email = TDB.GetEmailByToken(token);
            int userId = UsersDatabase.GetUserIdByEmail(email);

            List<Set> resultingSet = SetDatabase.GetAllSets(userId);
            if (resultingSet.Count == 0)
            {
                return BadRequest(new Response(false, null, "You don't have any sets"));
            }

            return Ok(new Response(true, resultingSet, null));
        }

        // GET: api/Set/5
        [HttpGet("{token}/{setId}")]
        public IActionResult GetSet(string token, int setId)
        {
            TokenClass tokenCL = new TokenClass();
            if (!tokenCL.ValidateToken(token))
            {
                return Unauthorized(new Response(false, null, "The user is not logged in"));
            }

            TokenDatabase TDB = new TokenDatabase();
            string email = TDB.GetEmailByToken(token);
            int userId = UsersDatabase.GetUserIdByEmail(email);

            Set resultingSet = SetDatabase.GetSet(userId, setId);
            if (resultingSet.Name == null)
            {
                return BadRequest(new Response(false, null, "You don't have set with this id"));
            }

            var resp = new
            {
                setInfo = resultingSet,
                words = WordsDatabase.GetSetWords(setId)
            };

            return Ok(new Response(true, resp, null));
        }

        // TODO : добавить set/addWord и set/deleteWord удаление можно реализовать через id, т.к. оно передаётся со всеми словами на клиент

        // POST: api/Set
        [Route("create")]
        [HttpPost]
        public IActionResult CreateSet(SetDto model)
        {
            TokenClass tokenCL = new TokenClass();
            if (!tokenCL.ValidateToken(model.AccessToken))
            {
                return Unauthorized(new Response(false, null, "The user is not logged in"));
            }

            TokenDatabase TDB = new TokenDatabase();
            string email = TDB.GetEmailByToken(model.AccessToken);
            int userId = UsersDatabase.GetUserIdByEmail(email);

            SetDatabase SDB = new SetDatabase();
            if (SDB.AddSet(userId, model.Name, model.Description))
            {
                return Ok(new Response(true, new { message = "Set created" }, null));
            }

            // If a set with the same name has already been created for such a user
            return BadRequest(new Response(false, null,
                "Set with the same name has already been created for such a user"));
        }

        // DELETE: api/Set
        [Route("delete")]
        [HttpDelete]
        public IActionResult DeleteSet(SetDto model)
        {
            TokenClass tokenCL = new TokenClass();
            if (!tokenCL.ValidateToken(model.AccessToken))
            {
                return Unauthorized(new Response(false, null, "The user is not logged in"));
            }

            TokenDatabase TDB = new TokenDatabase();
            string email = TDB.GetEmailByToken(model.AccessToken);
            int userId = UsersDatabase.GetUserIdByEmail(email);

            SetDatabase SDB = new SetDatabase();
            SDB.DeleteSet(userId, model.Name);

            return Ok(new Response(true, new { message = "Set deleted" }, null));
        }
    }
}