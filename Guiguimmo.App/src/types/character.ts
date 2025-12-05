export interface Character {
  id: string;
  name: string;
  class: string;
  color: string;
  health: number;
  level: number;
  bg: string;
  position: Position
}

export type Position = {
  x: number;
  y: number;
  z: number;
}

export type CreateCharacter = {
  name: string;
  raceId: string;
  classId: string;
  genderId: string;
  color: string;
  bg: string;
}