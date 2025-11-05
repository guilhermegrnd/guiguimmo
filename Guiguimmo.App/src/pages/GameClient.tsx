// GameClient.tsx

import React, { useState, useEffect, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from '../context/AuthContext'; // Import the auth hook
import type { Player } from '../types/player';
import type { ChatMessage } from '../types/chatMessage';

const SERVER_URL = 'https://localhost:5003';

export const GameClient: React.FC = () => {
  const { user } = useAuth();
  const token = user?.access_token || null;
  const [chatConnection, setChatConnection] = useState<signalR.HubConnection | null>(null);
  const [gameConnection, setGameConnection] = useState<signalR.HubConnection | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [players, setPlayers] = useState<Record<string, Player>>({});
  const [messageInput, setMessageInput] = useState<string>('');

  // --- 1. CONNECTION SETUP EFFECT ---
  // Connect only when a valid token is present
  useEffect(() => {
    if (!token) return;

    // --- 1.1. Build Connections with Token ---
    const buildConnection = (hubPath: string) => new signalR.HubConnectionBuilder()
        .withUrl(`${SERVER_URL}/${hubPath}`, {
            // 🔑 This is how the JWT is sent for authentication!
            accessTokenFactory: () => token,
        })
        .withAutomaticReconnect()
        .build();

    const newChatConnection = buildConnection('chathub');
    const newGameConnection = buildConnection('gamehub');

    // --- 1.2. Subscription Logic ---
    newChatConnection.on('ReceiveMessage', (user: string, message: string) => {
        setMessages(prev => [...prev, { user, message }]);
    });

    newGameConnection.on('ReceivePosition', (playerId: string, x: number, y: number, z: number) => {
        setPlayers(prev => ({ ...prev, [playerId]: { id: playerId, x, y, z } }));
    });
    
    // ... (Add other subscriptions like 'NewPlayerJoined', 'PlayerLeft' with correct types) ...
    newGameConnection.on('LoadExistingPlayers', (pArr: Player[]) => {
        const playerMap = pArr.reduce((acc, p) => ({ ...acc, [p.id]: p }), {});
        setPlayers(p => ({...p, ...playerMap}));
    });

    // --- 1.3. Start Connections ---
    Promise.all([newChatConnection.start(), newGameConnection.start()])
      .then(() => console.log('SignalR Connections Started with Auth!'))
      .catch(err => console.error('SignalR Connection Error: ', err));

    setChatConnection(newChatConnection);
    setGameConnection(newGameConnection);

    // --- 1.4. Cleanup ---
    return () => {
      newChatConnection.stop();
      newGameConnection.stop();
      setChatConnection(null);
      setGameConnection(null);
      setPlayers({});
      setMessages([]);
    };
  }, [token]); // Re-run whenever the token changes (i.e., on login/logout)

  // ... (sendChatMessage and sendPositionUpdate functions remain similar but use TS types) ...
  const sendChatMessage = useCallback(async () => {
    if (chatConnection && chatConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await chatConnection.invoke('SendMessage', 'ReactUser', messageInput);
        setMessageInput('');
      } catch (e) {
        console.error('Error sending chat message:', e);
      }
    }
  }, [chatConnection, messageInput]);

  return (
    <div>
      <p>Connection ID: {gameConnection?.connectionId || 'Connecting...'}</p>
      
      {/* Player List (Game State) */}
      <div style={{ border: '1px solid #ccc', padding: '10px' }}>
        <h3>Players in Game ({Object.keys(players).length})</h3>
        <ul>
          {Object.values(players).map(p => (
            <li key={p.id}>
              **{p.id.substring(0, 4)}...**: ({p.x.toFixed(1)}, {p.y.toFixed(1)}, {p.z.toFixed(1)})
            </li>
          ))}
        </ul>
      </div>
      
      {/* Chat Interface */}
      <div style={{ marginTop: '20px' }}>
        <h3>Global Chat</h3>
        <div style={{ height: '150px', overflowY: 'scroll', border: '1px solid #007bff', padding: '5px', marginBottom: '10px' }}>
          {messages.map((msg, index) => (
            <div key={index}>**{msg.user}**: {msg.message}</div>
          ))}
        </div>
        <input
          type="text"
          value={messageInput}
          onChange={(e) => setMessageInput(e.target.value)}
          placeholder="Type a chat message..."
          onKeyPress={(e) => { if (e.key === 'Enter') sendChatMessage(); }}
        />
        <button onClick={sendChatMessage}>Send</button>
      </div>
    </div>
  );
};
