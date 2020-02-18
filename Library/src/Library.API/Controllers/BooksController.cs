using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Models;
using Library.API.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Library.API.Helpers;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;
        
        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            return Ok(booksForAuthor);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if(bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);
            return Ok(bookForAuthor);
        }

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if(!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            if(book == null)
            {
                return BadRequest();
            }

            // Custom validation -- do this before checking for ModelState.IsValid
            if(book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto),
                    "The provided book description should be different from the book title.");
            }

            if(!ModelState.IsValid)
            {
                // 422 (Unprocessable Entity)
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var bookEntity = Mapper.Map<Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if(!_libraryRepository.Save())
            {
                throw new Exception("Creating book for author failed on save");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookEntity.Id }, bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookEntity == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookEntity);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if(book == null)
            {
                return BadRequest();
            }

            // Custom validation -- do this before checking for ModelState.IsValid
            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "The provided book description should be different from the book title.");
            }

            if (!ModelState.IsValid)
            {
                // 422 (Unprocessable Entity)
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookEntity == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if(!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
            }

            Mapper.Map(book, bookEntity);
            _libraryRepository.UpdateBookForAuthor(bookEntity);
            if(!_libraryRepository.Save())
            {
                throw new Exception($"Updating book id {id} for author {authorId} failed on save.");
            }
            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if(patchDoc == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookEntity == null)
            {
                // Upsert
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto);

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book id {id} for author {authorId} failed on save.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = id }, bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookEntity);
            patchDoc.ApplyTo(bookToPatch);

            // TODO - Add validation

            Mapper.Map(bookToPatch, bookEntity);
            _libraryRepository.UpdateBookForAuthor(bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book id {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }
    }
}
