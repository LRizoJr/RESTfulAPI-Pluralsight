using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
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

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            var idsAsString = string.Join(",", authorEntities.Select(x => x.Id));

            return CreatedAtRoute("GetAuthorsCollection", new { ids = idsAsString}, authorsToReturn); 
        }

        [HttpGet("({ids})", Name ="GetAuthorsCollection")]
        public IActionResult GetAuthorsCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                return BadRequest();
            }

            var authorEntities = _libraryRepository.GetAuthors(ids);
            if(authorEntities.Count() != ids.Count())
            {
                return NotFound();
            }

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }
    }
}
