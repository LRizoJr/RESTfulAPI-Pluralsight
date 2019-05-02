using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authorscollection")]
    public class AuthorsCollectionController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorsCollectionController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost()]
        public IActionResult CreateAuthorsCollection([FromBody] IEnumerable<AuthorForCreationDto> authors)
        {
            if(authors == null || !authors.Any())
            {
                return BadRequest();
            }

            var authorEntities = Mapper.Map<IEnumerable<Author>>(authors);
            foreach(var author in authorEntities)
            {                
                _libraryRepository.AddAuthor(author);
            }
            
            if(!_libraryRepository.Save())
            {
                throw new Exception("Creating an author collection failed on save");
            }

            return Ok();
        }

        [HttpGet("({ids})")]
        public IActionResult GetAuthorsCollection(IEnumerable<Guid> ids)
        {

        }
    }
}
