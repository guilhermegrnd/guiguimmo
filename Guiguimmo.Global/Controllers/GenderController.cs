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
public class GendersController : ControllerBase
{
  private readonly IRepository<Gender> _genderRepository;

  public GendersController(IRepository<Gender> genderRepository)
  {
    _genderRepository = genderRepository;
  }

  [HttpGet]
  [Authorize(Roles = "member")]
  public async Task<ActionResult<IEnumerable<GenderDto>>> GetAsync()
  {
    var items = (await _genderRepository.GetAllAsync()).Select(item => item.AsDto());
    return Ok(items);
  }

  [HttpGet("{id}")]
  [Authorize(Roles = "member")]
  public async Task<ActionResult<GenderDto>> GetByIdAsync(Guid id)
  {
    var item = await _genderRepository.GetAsync(id);

    if (item == null)
    {
      return NotFound();
    }

    return item.AsDto();
  }
}