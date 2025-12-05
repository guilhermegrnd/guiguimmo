import type { Character } from './character'

export type GameState = {
  timeStamp: string | number;
  characters: Character[];
  mapTiles?: string[][];
};