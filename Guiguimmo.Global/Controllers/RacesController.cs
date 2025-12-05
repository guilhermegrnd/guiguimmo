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
public class RacesController : ControllerBase
{
  private readonly IRepository<Race> _racesRepository;

  public RacesController(IRepository<Race> RacesRepository)
  {
    _racesRepository = RacesRepository;
  }

  [HttpGet]
  [Authorize(Roles = "member")]
  public async Task<ActionResult<IEnumerable<RaceDto>>> GetAsync()
  {
    var items = (await _racesRepository.GetAllAsync()).Select(item => item.AsDto());
    return Ok(items);
  }

  [HttpGet("{id}")]
  [Authorize(Roles = "member")]
  public async Task<ActionResult<RaceDto>> GetByIdAsync(Guid id)
  {
    var item = await _racesRepository.GetAsync(id);

    if (item == null)
    {
      return NotFound();
    }

    return item.AsDto();
  }
}