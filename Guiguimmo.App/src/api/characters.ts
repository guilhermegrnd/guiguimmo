import { CHARACTERS_SERVER_URL } from '../lib/constants'
import type { Character, CreateCharacter } from '../types/character'

export const createCharacter = async (token: string, createCharacterData: CreateCharacter): Promise<void> => {
  try {
    const response = await fetch(`${CHARACTERS_SERVER_URL}/v1/characters`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(createCharacterData),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    console.log('Character uploaded successfully');
  } catch (error) {
    console.error('Error uploading Character:', error);
  }
};

export const fetchCharacters = async (token: string): Promise<Character[]> => {
  const response = await fetch(`${CHARACTERS_SERVER_URL}/v1/characters`, {
    headers: {
      'Authorization': `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }

  return response.json();
};