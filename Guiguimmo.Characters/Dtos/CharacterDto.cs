using System;
using System.ComponentModel.DataAnnotations;
using Guiguimmo.Characters.Models;

namespace Guiguiflix.Characters.Dtos;

public record CharacterDto(Guid Id, string Name, Guid RaceId, Guid ClassId, Guid GenderId, int Health, int Level, string Color, string Bg, Position Position);

public record CreateCharacterDto([Required] string Name, Guid RaceId, Guid ClassId, Guid GenderId, string Color, string Bg);

public record UpdateCharacterDto(int Level, string Color, string Bg, Position Position);