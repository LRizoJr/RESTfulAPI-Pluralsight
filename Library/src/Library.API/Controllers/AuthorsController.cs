using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using Library.API.Models;
using Library.API.Helpers;
using AutoMapper;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;

namespace Library.API.Controllers
{    
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authorsFromRepo = _libraryRepository.GetAuthors();
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors);
        }

        [HttpGet("{id}", Name ="GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {            
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorFromRepo);            
            return Ok(author);
        }

        [HttpPost()]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if(author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(authorEntity);

            if(!_libraryRepository.Save())
            {
                // Let ExceptionHandler middleware return a 500 server error
                throw new Exception("Creating an author failed on save");
            }
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { id = authorEntity.Id }, authorToReturn);
        }

        // Need this to adhere to http standard of returning 404 for non-existing author and 409 (conflict) for existing author since a post with a guid parameter is invalid
        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreationByGuid(Guid id)        
        {
            if(_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();            
        }
    }
}