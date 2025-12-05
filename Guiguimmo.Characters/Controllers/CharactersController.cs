using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Guiguiflix.Characters.Dtos;
using Guiguiflix.Characters.Extensions;
using Guiguimmo.Characters.Models;
using Guiguimmo.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guiguimmo.Characters.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class CharactersController : ControllerBase
{
  private readonly IRepository<Character> _charactersRepository;

  public CharactersController(IRepository<Character> charactersRepository)
  {
    _charactersRepository = charactersRepository;
  }

  // [HttpGet]
  // [Authorize(Roles = "admin")]
  // public async Task<ActionResult<IEnumerable<CharacterDto>>> GetAsync()
  // {
  //   var items = (await _charactersRepository.GetAllAsync()).Select(item => item.AsDto());
  //   return Ok(items);
  // }

  [HttpGet]
  [Authorize(Roles = "member, admin")]
  public async Task<ActionResult<IEnumerable<CharacterDto>>> GetAsyncByUser()
  {
    var claims = User.Claims;
    var UserId = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
    if (UserId == Guid.Empty)
    {
      return Unauthorized();
    }

    var items = (await _charactersRepository.GetAllAsync())
      .Where(item => item.UserId == UserId)
      .Select(item => item.AsDto());
    return Ok(items);
  }

  [HttpGet("{id}")]
  [Authorize(Roles = "member, admin")]
  public async Task<ActionResult<CharacterDto>> GetByIdAsync(Guid id)
  {
    var item = await _charactersRepository.GetAsync(id);

    if (item == null)
    {
      return NotFound();
    }

    return item.AsDto();
  }

  [HttpPost]
  [Authorize(Roles = "member, admin")]
  public async Task<ActionResult<CharacterDto>> PostAsync(CreateCharacterDto createCharacterDto)
  {
    var claims = User.Claims;
    var UserId = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
    if (UserId == Guid.Empty)
    {
      return Unauthorized();
    }

    var item = new Character
    {
      Name = createCharacterDto.Name,
      UserId = UserId,
      RaceId = createCharacterDto.RaceId,
      ClassId = createCharacterDto.ClassId,
      GenderId = createCharacterDto.GenderId,
      Color = createCharacterDto.Color,
      Bg = createCharacterDto.Bg,
      Health = 100,
      Level = 1,
      Position = new Position { X = 0, Y = 0, Z = 0 }
    };

    await _charactersRepository.CreateAsync(item);

    // return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    return item.AsDto();
  }

  [HttpPut("{id}")]
  [Authorize(Roles = "admin")]
  public async Task<ActionResult> PutAsync(Guid id, UpdateCharacterDto updateCharacterDto)
  {
    var existingItem = await _charactersRepository.GetAsync(id);
    if (existingItem == null)
    {
      return NotFound();
    }

    existingItem.Level = updateCharacterDto.Level;
    existingItem.Color = updateCharacterDto.Color;
    existingItem.Bg = updateCharacterDto.Bg;
    existingItem.Position = updateCharacterDto.Position;

    await _charactersRepository.UpdateAsync(existingItem);

    return NoContent();
  }

  [HttpDelete("{id}")]
  [Authorize(Roles = "admin")]
  public async Task<ActionResult> DeleteAsync(Guid id)
  {
    var existingItem = await _charactersRepository.GetAsync(id);
    if (existingItem == null)
    {
      return NotFound();
    }

    await _charactersRepository.RemoveAsync(existingItem.Id);

    return NoContent();
  }
}