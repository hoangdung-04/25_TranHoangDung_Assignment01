using System;
using System.Linq;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Repositories.Interfaces;

namespace _25_TranHoangDung_Assignment01_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ODataController
    {
        private readonly ITagRepository _tagRepository;

        public TagsController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        [EnableQuery]
        public IActionResult Get()
        {
            var tags = _tagRepository.GetTags();
            return Ok(tags.AsQueryable());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var tag = _tagRepository.GetTagById(id);
            if (tag == null)
            {
                return NotFound(new { message = $"Tag with ID {id} not found." });
            }
            return Ok(tag);
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Create([FromBody] Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = _tagRepository.AddTag(tag);
                return CreatedAtAction(nameof(GetById), new { id = created.TagID }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
