using System;
using System.Net;
using System.Threading.Tasks;
using Api.Exceptions;
using Api.Model;
using Api.Services.EntryServices;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntryController : Controller
    {
        private readonly IEntryService _service;
        public EntryController(IEntryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates a new entry based on input
        /// </summary>
        /// <param name="entryDto"> The inputs for the entry: </param>
        /// <param name="apiKey">Users api key</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /entry
        ///     {
        ///        "username": "Jabba the Hutt",
        ///        "description": "Gott kaffe, bråkig personal",
        ///        "rating": 3,
        ///        "poi": {
        ///             "name": "Jannes Fik",
        ///             "longitude": 15.532,
        ///             "latitude": 64.324,
        ///             "city": "Stockholm",
        ///             "country": "Sverige"
        ///         }    
        ///     }
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(Response<EntryDto>), 200)]
        public async Task<IActionResult> SetEntry([FromBody] AddEntry entryDto, [FromHeader] string apiKey)
        {
            try
            {
                return Ok(new Response<EntryDto>(await _service.SetEntry(entryDto, apiKey)));
            }
            catch (Exception)
            {
                //TODO: dont catch exception instead catch a specified exception
                return StatusCode((int)HttpStatusCode.BadRequest, new Response<Exception>(Status.Fail, "Something went wrong"));
            }

        }

        /// <summary>
        /// Update an existing entry. Omitt properties which should not be changed
        /// </summary>
        /// <param name="updateDto"></param>
        /// <param name="apiKey"></param>
        /// <param name="id">id of the entry to update</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /entry
        ///     {
        ///        "username": "Jabba the Hutt",
        ///        "description": "Gott kaffe, bråkig personal",
        ///        "rating": 3,
        ///        "poi": {
        ///             "name": "Jannes Fik",
        ///             "longitude": 15.532,
        ///             "latitude": 64.324,
        ///             "city": "Stockholm",
        ///             "country": "Sverige"
        ///         }    
        ///     }
        /// </remarks>
        [HttpPut(":id")]
        [ProducesResponseType(typeof(Response<EntryDto>), 200)]
        public async Task<IActionResult> UpdateEntry([FromBody] UpdateEntry updateDto, [FromHeader] string apiKey, long id)
        {
            try
            {
                return Ok(new Response<EntryDto>(await _service.UpdateEntry(updateDto, apiKey, id)));
            }
            catch (UnauthorizedAccessException ex)
            {
#if RELEASE
                return StatusCode((int)HttpStatusCode.Unauthorized,new Response<UnauthorizedAccessException>(Status.Fail, ex.Message));
#endif
                return StatusCode((int)HttpStatusCode.Unauthorized, new Response<UnauthorizedAccessException>(Status.Fail, ex.Message, ex));
            }
            catch (NotFoundException ex)
            {
#if RELEASE
                return StatusCode((int)HttpStatusCode.NotFound,new Response<NotFoundException>(Status.Fail, ex.Message));
#endif
                return StatusCode((int)HttpStatusCode.NotFound, new Response<NotFoundException>(Status.Fail, ex.Message, ex));
            }
        }

        /// <summary>
        /// Deletes an entry from the database
        /// </summary>
        /// <param name="id">Id of the entry to delete</param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        [HttpDelete(":id")]
        [ProducesResponseType(typeof(Response<EntryDto>), 200)]
        public async Task<IActionResult> RemoveEntry(long id, [FromHeader] string apiKey)
        {
            try
            {
                return Ok(new Response<EntryDto>(await _service.RemoveEntry(id, apiKey)));
            }
            catch (NotFoundException ex)
            {
#if RELEASE
                return StatusCode((int)HttpStatusCode.NotFound,new Response<NotFoundException>(Status.Fail, ex.Message));
#endif
                return StatusCode((int)HttpStatusCode.NotFound, new Response<NotFoundException>(Status.Fail, ex.Message, ex));
            }
        }

        [HttpPost("like/:entryId")]
        public async Task<IActionResult> AddLike(long entryId, [FromHeader] string ApiKey)
        {
            try
            {
                return Ok(new Response<LikeDislikeEntryDto>(await _service.AddLike(entryId, ApiKey)));
            }
            catch (NotFoundException ex)
            {
#if RELEASE
                return StatusCode((int)HttpStatusCode.NotFound,new Response<NotFoundException>(Status.Fail, ex.Message));
#endif
                return StatusCode((int)HttpStatusCode.NotFound, new Response<NotFoundException>(Status.Fail, ex.Message, ex));
            }
        }
    }
}
