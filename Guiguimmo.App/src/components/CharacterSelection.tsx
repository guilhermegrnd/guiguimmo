import { use, useCallback, useEffect, useState, type FC } from 'react'
import type { Character } from '../types/character'
import { GameLobby } from './GameLobby'
import { useAuth } from '../context/AuthContext'
import type { Race } from '../types/race'
import type { Class } from '../types/class'
import type { Gender } from '../types/gender'
import { CHARACTERS_SERVER_URL, GLOBAL_SERVER_URL } from '../lib/constants'
import { fetchApi } from '../api/fetchApi'
import { Loading } from './Loading'

interface CharacterCardProps {
  character: Character;
  isSelected: boolean;
  onClick: (character: Character) => void;
}

const CharacterCard: FC<CharacterCardProps> = ({ character, isSelected, onClick }) => {
  return (
    <div
      onClick={() => onClick(character)}
      className={`
        p-4 sm:p-6 rounded-xl transition-all duration-300 cursor-pointer 
        flex flex-col items-center text-center shadow-2xl
        ${character.bg} border-2 ${character.color}
        ${isSelected 
          ? 'ring-4 ring-offset-4 ring-offset-gray-900 ring-yellow-400 scale-105' 
          : 'opacity-70 hover:opacity-100 hover:scale-105'
        }
      `}
    >
      <h3 className="text-xl sm:text-2xl font-bold text-yellow-300 mb-1">{character.name}</h3>
      <p className="text-sm text-gray-300">{character.classId}</p>
    </div>
  );
};

export const CharacterSelection: FC = () => {
  const { user, signOut } = useAuth()
  const [hasJoined, setHasJoined] = useState<boolean>(false)
  const [selectedCharacter, setSelectedCharacter] = useState<Character | undefined>(undefined)
  const [isJoining, setIsJoining] = useState<boolean>(false)
  const [joinMessage, setJoinMessage] = useState<string>('')
  const [isCreatingCharacter, setIsCreatingCharacter] = useState<boolean>(false)

  const [charactersData, setCharactersData] = useState<Character[]>([]);
  const [racesData, setRacesData] = useState<Race[]>([]);
  const [classesData, setClassesData] = useState<Class[]>([]);
  const [gendersData, setGendersData] = useState<Gender[]>([]);

  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    async function fetchMultipleData() {
      setIsLoading(true);
      try {
        const fetchPromises = [
          fetchApi<Character[]>(user?.access_token!,`${CHARACTERS_SERVER_URL}/v1/characters`),
          fetchApi<Race[]>(user?.access_token!,`${GLOBAL_SERVER_URL}/v1/races`),
          fetchApi<Class[]>(user?.access_token!,`${GLOBAL_SERVER_URL}/v1/classes`),
          fetchApi<Gender[]>(user?.access_token!,`${GLOBAL_SERVER_URL}/v1/genders`),
        ];

        const [characters, races, classes, genders] = await Promise.all(fetchPromises);
        setCharactersData(characters as Character[]);
        setRacesData(races as Race[]);
        setClassesData(classes as Class[]);
        setGendersData(genders as Gender[]);
      } catch (error) {
        console.error("Error fetching or processing data:", error);
        throw error;
      } finally {
        setIsLoading(false);
      }
    }
    fetchMultipleData()
  }, []);
        
  
  const handleJoinGame = useCallback(() => {
    if (!selectedCharacter) {
      setJoinMessage('Please select a character.');
      return;
    }

    setIsJoining(true);
    setJoinMessage('Attempting to connect to the game server...');

    console.log(`Selected Character: ${selectedCharacter.name} (ID: ${selectedCharacter.id})`);
    
    setTimeout(() => {
        setIsJoining(false);
        setHasJoined(true);
        setJoinMessage(`Connected! Welcome, ${selectedCharacter.name}.`);
    }, 1500);

  }, [selectedCharacter]);
  
  const handleLogout = () => {
    setHasJoined(false);
    setIsJoining(false);
    setJoinMessage('Welcome back! Select a character to join the hub.');
  };

  const handleSelectCharacter = (character: Character) => {
    setSelectedCharacter(character);
  }

  if (hasJoined && selectedCharacter) {
    return (
      <GameLobby 
        character={selectedCharacter} 
        race={(racesData.find(r => r.id === selectedCharacter.raceId)?.name || 'unknown').toLowerCase()} 
        gender={(gendersData.find(r => r.id === selectedCharacter.genderId)?.name || 'unknown').toLowerCase()} 
        handleLogout={handleLogout} />
    );
  }

  if (isLoading) {
    return <Loading />;
  }

  if (isCreatingCharacter) {

    const handleCreateCharacter = async (e: React.FormEvent) => {
      e.preventDefault();
      const form = e.target as HTMLFormElement;
      const name = (form.elements.namedItem('characterName') as HTMLInputElement).value;
      const genderId = (form.elements.namedItem('characterGender') as HTMLSelectElement).value;
      const raceId = (form.elements.namedItem('characterRace') as HTMLSelectElement).value;
      const classId = (form.elements.namedItem('characterClass') as HTMLSelectElement).value;
      try {
        const newCharacter = await fetchApi<Character>(user?.access_token!,`${CHARACTERS_SERVER_URL}/v1/characters`,'POST',{
          name,
          genderId,
          raceId,
          classId,
          color: 'cyan-500',
          bg: 'bg-gradient-to-r from-cyan-500 to-blue-500',
        })
        setIsCreatingCharacter(false);
        setCharactersData(prev => [...prev, newCharacter]);
      } catch (error) {
        console.error("Error creating character:", error);
      }
    };

    return (
      <div className="min-h-screen bg-gray-900 text-white font-inter p-4 sm:p-8 flex items-center justify-center">
        <div className="w-full max-w-5xl bg-gray-800/90 backdrop-blur-sm p-6 sm:p-10 rounded-3xl shadow-2xl border border-gray-700">
          
          <h1 className="text-4xl sm:text-5xl font-extrabold text-center text-transparent bg-clip-text bg-linear-to-r from-yellow-300 to-red-500 mb-2">
            Create New Character
          </h1>
          <form className="space-y-6 max-w-lg mx-auto" onSubmit={handleCreateCharacter}>
            <div>
              <label className="block text-sm font-medium mb-1" htmlFor="characterName">Character Name</label>
              <input
                type="text"
                id="characterName"
                className="w-full px-4 py-2 rounded-lg bg-gray-700 border border-gray-600 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400"
                placeholder="Enter character name"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1" htmlFor="characterGender">Gender</label>
              <select
                id="characterGender"
                className="w-full px-4 py-2 rounded-lg bg-gray-700 border border-gray-600 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400"
              >
                {gendersData && gendersData.map(item => <option value={item.id}>{item.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1" htmlFor="characterRace">Race</label>
              <select
                id="characterRace"
                className="w-full px-4 py-2 rounded-lg bg-gray-700 border border-gray-600 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400"
              >
                {racesData && racesData.map(item => <option value={item.id}>{item.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1" htmlFor="characterClass">Class</label>
              <select
                id="characterClass"
                className="w-full px-4 py-2 rounded-lg bg-gray-700 border border-gray-600 text-white focus:outline-none focus:ring-2 focus:ring-yellow-400"
              >
                {classesData && classesData.map(item => <option value={item.id}>{item.name}</option>)}
              </select>
            </div>
            <div className="text-center">
              <button
                type="submit"
                className="px-12 py-4 text-xl font-bold uppercase rounded-full bg-green-500 hover:bg-green-600 text-gray-900 shadow-lg tracking-wider transition-all duration-300 hover:shadow-green-500/50 transform hover:-translate-y-1"
              >
                Create Character
              </button>
            </div>
          </form>
          <div>
            <button
              onClick={() => setIsCreatingCharacter(false)}
              className="mt-6 px-6 py-3 bg-yellow-500 hover:bg-yellow-600 text-gray-900 font-bold rounded-full"
            >
              Cancel
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <>
      <header className="fixed top-0 left-0 w-full z-10">
        <nav className="bg-gray-800 text-white p-4 flex justify-between items-center">
          <h1 className="text-xl">Guiguimmo</h1>
          <button
            onClick={signOut}
            className="px-6 py-2 bg-red-600 text-white font-semibold rounded-lg shadow-md hover:bg-red-700 transition duration-150 transform hover:scale-[1.02]"
          >
            Sign Out
          </button>
        </nav>
      </header> 
      <div className="min-h-screen bg-gray-900 text-white font-inter p-4 sm:p-8 flex items-center justify-center">
        <div className="w-full max-w-5xl bg-gray-800/90 backdrop-blur-sm p-6 sm:p-10 rounded-3xl shadow-2xl border border-gray-700">
          
          <h1 className="text-4xl sm:text-5xl font-extrabold text-center text-transparent bg-clip-text bg-linear-to-r from-yellow-300 to-red-500 mb-2">
            Choose Your Character
          </h1>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
            {charactersData.map((char) => (
              <CharacterCard
                key={char.id}
                character={char}
                isSelected={selectedCharacter?.id === char.id}
                onClick={handleSelectCharacter}
              />
            ))}
          </div>

          {selectedCharacter && (
            <div className="mb-8 p-4 bg-gray-700/50 rounded-lg border border-gray-600 text-center max-w-xl mx-auto">
              <p className="text-lg font-semibold text-yellow-300">
                Selected Character: <span className="text-white">{selectedCharacter.name}</span>
              </p>
            </div>
          )}

          <div className="flex justify-between">
            <button
              onClick={() => setIsCreatingCharacter(true)}
              className={`
                px-12 py-4 text-xl font-bold uppercase rounded-full transition-all duration-300 
                shadow-lg tracking-wider bg-green-500 hover:bg-green-600 text-gray-900 hover:shadow-green-500/50 transform hover:-translate-y-1 cursor-pointer
              `}
            >
              Create New Character
            </button>
            <button
              onClick={handleJoinGame}
              disabled={selectedCharacter === undefined || isJoining}
              className={`
                px-12 py-4 text-xl font-bold uppercase rounded-full transition-all duration-300 
                shadow-lg tracking-wider
                ${selectedCharacter && !isJoining
                  ? 'bg-green-500 hover:bg-green-600 text-gray-900 hover:shadow-green-500/50 transform hover:-translate-y-1 cursor-pointer'
                  : 'bg-gray-600 text-gray-400 cursor-not-allowed'
                }
              `}
            >
              {isJoining ? (
                <div className="flex items-center justify-center">
                  <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-gray-900" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Joining...
                </div>
              ) : 'Join Game Hub'}
            </button>
            
            {joinMessage && (
              <p className="mt-4 text-sm font-medium text-yellow-400 transition-opacity duration-300">
                {joinMessage}
              </p>
            )}
          </div>

        </div>
      </div>
    </>
  );
};