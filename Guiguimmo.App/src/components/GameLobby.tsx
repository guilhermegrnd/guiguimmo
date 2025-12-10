import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../context/AuthContext'
import type { Character } from '../types/character'
import { useSignalRGame } from '../hooks/UseSignalRGame'
import HumanMaleSprite from '../assets/human_male.png';
import HumanFemaleSprite from '../assets/human_female.png';
import OrcMaleSprite from '../assets/orc_male.png';
import OrcFemaleSprite from '../assets/orc_female.png';
import ElfMaleSprite from '../assets/elf_male.png';
import ElfFemaleSprite from '../assets/elf_female.png';
import DwarfMaleSprite from '../assets/dwarf_male.png';
import DwarfFemaleSprite from '../assets/dwarf_female.png';
import MiniMap from './MiniMap'

interface GameLobbyProps {
  character: Character;
  race: string;
  gender: string;
  handleLogout: () => void;
}

const getTileClass = (tileValue: string) => {
  return `tile tile-${tileValue}`;
};

const getCharacterSprite = (race: string, gender: string) => {
  if (race === 'human') {
    return gender === 'male' ? HumanMaleSprite : HumanFemaleSprite;
  } else if (race === 'orc') {
    return gender === 'male' ? OrcMaleSprite : OrcFemaleSprite;
  } else if (race === 'elf') {
    return gender === 'male' ? ElfMaleSprite : ElfFemaleSprite;
  } else if (race === 'dwarf') {
    return gender === 'male' ? DwarfMaleSprite : DwarfFemaleSprite;
  }
};

export const GameLobby: React.FC<GameLobbyProps> = ({ character, race, gender, handleLogout }) => {
  const { user } = useAuth()
  const token = user?.access_token || null
  const { gameState, connectionStatus, connect, sendMovement, sendMessage, startAutoWalk, messages } = useSignalRGame(token!);
  const [messageInput, setMessageInput] = useState<string>('')
  const [messageDisplay, setMessageDisplay] = useState<string>('')
  const messagesEndRef = useRef<HTMLDivElement | null>(null);
  connect(character.id);

  useEffect(() => {
    const handleKeyDown = (event: any) => {
      switch (event.key) {
        case 'ArrowUp':
          sendMovement('up');
          break;
        case 'ArrowDown':
          sendMovement('down');
          break;
        case 'ArrowLeft':
          sendMovement('left');
          break;
        case 'ArrowRight':
          sendMovement('right');
          break;
        default:
          break;
      }
    };

    globalThis.addEventListener('keydown', handleKeyDown);
    
    return () => {
        globalThis.removeEventListener('keydown', handleKeyDown);
    };
  }, [sendMovement]);

  useEffect(() => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'auto' });
    }
  }, [messages]);

  if (connectionStatus === 'Disconnected') {
    return (
      <div>
        <p>Status: Disconnected</p>
        <button onClick={() => connect(character.id)}>
          Connect and Join Game
        </button>
      </div>
    );
  }

  if (connectionStatus !== 'Connected') {
    return <div>Status: {connectionStatus}...</div>;
  }
  
  const displayMessage = (msg: string) => {
    setMessageDisplay(msg);
    const timer = setTimeout(() => {
      setMessageDisplay('')
      clearTimeout(timer)
    }, 3000)
  }

  const handleSendMessage = () => {
    if (messageInput.trim() === '') return;
    sendMessage(messageInput);
    displayMessage(messageInput);
    setMessageInput('');
  }

  return (
    <div className="grow grid grid-cols-12 gap-4 h-screen">
      <section className="col-span-12 lg:col-span-10 p-4 flex gap-4 flex-col max-h-screen">
        <div className="h-75/100 p-4">
          <div className="map-container">
            {gameState.mapTiles?.map((row, y) => (
              <>
                {row.map((tileValue, x) => (
                  <div 
                    key={`${x}-${y}`} 
                    className={getTileClass(tileValue)}
                    title={`Tile: ${tileValue} at (${x}, ${y})`}
                    onClick={() => startAutoWalk(x, y)}
                  />
                ))}
              </>
            ))}
            {gameState.characters.map(p => (
              <div 
                key={p.id}
                className="entity-sprite"
                style={{
                  left: `${p.position.x * 4}rem`,
                  top: `${p.position.y * 4}rem`,
                  backgroundImage: `url(${getCharacterSprite(race, gender)})`,
                  backgroundSize: 'cover'
                }}
              >
                {messageDisplay && <div className='message-bubble'>{messageDisplay}</div>}
                <div className='chracter-name-bubble'>{p.name}</div>
              </div>
            ))}
            {/* You would also map over monsters/items here... */}
          </div>
        </div>
        <div className="h-25/100 p-4">
          <div className='h-full border border-gray-300 rounded flex flex-col'>
            <div className='h-2/3 overflow-y-auto p-1'>
              {messages.map((msg, index) => (
                <div key={index} className='text-white'><b>{msg.user}</b>: {msg.message}</div>
              ))}
              <div ref={messagesEndRef} />
            </div>
            <div className='h-1/3 p-1 flex justify-between gap-2 items-center'>
              <input
                className='w-9/10 p-1 border border-gray-300 rounded text-white'
                type="text"
                value={messageInput}
                onChange={(e) => setMessageInput(e.target.value)}
                placeholder="Type a chat message..."
                onKeyDown={(e) => { if (e.key === 'Enter') handleSendMessage() }}
              />
              <button onClick={handleSendMessage} className='w-1/10 border rounded border-gray-300 text-white'>Send</button>
            </div>
          </div>
        </div>
      </section>
        
      <aside className="col-span-12 lg:col-span-2 p-4 border-l border-gray-300">
        {character.id && (
          <MiniMap gameState={gameState} characterId={character.id} />
        )}
        <p className="text-xl text-gray-200 mb-2">
          Welcome, <span className="text-yellow-300 font-bold">{character.name}</span>!
        </p>
        <div className="h-96 p-4">
          <button
            onClick={handleLogout}
            className="px-8 py-3 text-lg font-semibold rounded-lg bg-red-600 hover:bg-red-700 text-white transition duration-200 shadow-md transform hover:scale-[1.02]"
          >
            Logout
          </button>
        </div>
      </aside>
    </div>
  )
};