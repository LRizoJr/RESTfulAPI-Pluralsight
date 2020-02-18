using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title.")]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public virtual string Description { get; set; }
    }
}
