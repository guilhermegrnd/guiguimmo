import { useEffect, useState } from 'react'
import { useAuth } from '../context/AuthContext'
import type { Character } from '../types/character'
import { useSignalRGame } from '../hooks/UseSignalRGame'

interface GameLobbyProps {
  character: Character;
  handleLogout: () => void;
}

const getTileClass = (tileValue: string) => {
  return `tile tile-${tileValue}`;
};

export const GameLobby: React.FC<GameLobbyProps> = ({ character, handleLogout }) => {
  const { user } = useAuth()
  const token = user?.access_token || null
  const { gameState, connectionStatus, connect, sendMovement, sendMessage, startAutoWalk, messages } = useSignalRGame(token!);
  const [messageInput, setMessageInput] = useState<string>('')
  const [messageDisplay, setMessageDisplay] = useState<string>('')
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
    sendMessage(messageInput);
    displayMessage(messageInput);
    setMessageInput('');
  }
  console.log('Game State messages:', messages);
  return (
    <div className="grow grid grid-cols-12 gap-4 pt-16 h-screen">
      <section className="col-span-12 lg:col-span-10 p-4 flex gap-4 flex-col h-full bg-green-400">
        <div className="h-4/5 bg-gray-100 p-4">
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
                  top: `${p.position.y * 4}rem`
                }}
              >
                {messageDisplay && <div className='message-bubble'>{messageDisplay}</div>}
                {p.name}
              </div>
            ))}
            {/* You would also map over monsters/items here... */}
          </div>
        </div>
        <div className="h-1/5 bg-red-100 border border-red-500 p-4">
          <div className='h-full bg-cyan-300'>
            <div className='h-2/3 overflow-y-auto p-1'>
              {messages.map((msg, index) => (
                <div key={index} className='text-gray-700'><b>{msg.user}</b>: {msg.message}</div>
              ))}
            </div>
            <div className='h-1/3 p-1'>
              <input
                className='w-9/10 p-1 border border-gray-300 rounded text-gray-700'
                type="text"
                value={messageInput}
                onChange={(e) => setMessageInput(e.target.value)}
                placeholder="Type a chat message..."
                onKeyDown={(e) => { if (e.key === 'Enter') handleSendMessage() }}
              />
              <button onClick={handleSendMessage} className='w-1/10 text-gray-700'>Send</button>
            </div>
          </div>
        </div>
      </section>
        
      <aside className="col-span-12 lg:col-span-2 p-4 bg-blue-400 border-l border-gray-200">
          <h2 className="text-2xl font-bold mb-4">Right Sidebar</h2>
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