using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Guiguiflix.Characters.Service.Extensions;
using Guiguiflix.Global.Service.Dtos;
using Guiguimmo.Common.Interfaces;
using Guiguimmo.Global.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guiguimmo.Global.Service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class ClassesController : ControllerBase
{
  private readonly IRepository<Class> _classesRepository;

  public ClassesController(IRepository<Class> classesRepository)
  {
    _classesRepository = classesRepository;
  }

  [HttpGet]
  [Authorize(Roles = "member")]
  public async Task<ActionResult<IEnumerable<ClassDto>>> GetAsync()
  {
    var items = (await _classesRepository.GetAllAsync()).Select(item => item.AsDto());
    return Ok(items);
  }

  [HttpGet("{id}")]
  [Authorize(Roles = "member")]
  public async Task<ActionResult<ClassDto>> GetByIdAsync(Guid id)
  {
    var item = await _classesRepository.GetAsync(id);

    if (item == null)
    {
      return NotFound();
    }

    return item.AsDto();
  }
}