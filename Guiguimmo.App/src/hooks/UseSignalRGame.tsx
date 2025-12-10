import { useCallback, useEffect, useState } from 'react'
import * as signalR from '@microsoft/signalr'
import { GAME_SERVER_URL } from '../lib/constants'
import type { ChatMessage } from '../types/chatMessage'
import type { GameState } from '../types/gamestate'

interface GameHookResult {
  connection: signalR.HubConnection | null;
  gameState: GameState;
  connectionStatus: 'Disconnected' | 'Connecting' | 'Connected' | 'Error';
  error: string | null;
  connect: (characterId: string) => void;
  sendMovement: (direction: string) => void;
  sendMessage: (message: string) => void;
  startAutoWalk: (destX: number, destY: number) => void;
  messages: ChatMessage[];
}

const initialGameState: GameState = {
  timeStamp: Date.now(),
  characters: [],
  mapTiles: []
};

export const useSignalRGame = (token: string): GameHookResult => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [gameState, setGameState] = useState<GameState>(initialGameState);
  const [connectionStatus, setConnectionStatus] = useState<GameHookResult['connectionStatus']>('Disconnected');
  const [error, setError] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([])

  const connect = useCallback((characterId: string) => {
    if (connectionStatus !== 'Disconnected') return;
    
    setConnectionStatus('Connecting');
    setError(null);

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${GAME_SERVER_URL}/gamehub`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    newConnection.start()
      .then(() => {
        newConnection.invoke("JoinCharacter", characterId);
        setConnection(newConnection);
        setConnectionStatus('Connected');
        console.log("SignalR Connected.");
      })
      .catch((err) => {
        console.error("SignalR Connection Error: ", err);
        setError(err.toString());
        setConnectionStatus('Error');
      });
  }, [connectionStatus]);
  
  useEffect(() => {
    if (!connection) return;

    connection.on('ReceiveGameState', (newState: GameState) => {
      setGameState(newState);
    });

    connection.on('ReceiveMessage', (timestamp: number, user: string, message: string) => {
      setMessages(prev => [...prev, { timestamp, user, message }])
    })

    connection.onreconnecting(() => setConnectionStatus('Connecting'));
    connection.onreconnected(() => setConnectionStatus('Connected'));
    connection.onclose(() => setConnectionStatus('Disconnected'));

    return () => {
      connection.off("ReceivegameState");
      connection.off("ReceiveMessage");
      connection.stop()
        .then(() => console.log("SignalR Disconnected."))
        .catch(err => console.error("SignalR Disconnect Error: ", err));
    };
  }, [connection]);

  const sendMovement = useCallback((direction: string) => {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      connection.invoke('MoveCharacter', direction)
        .catch(err => console.error(err));
    } else {
      console.warn("SignalR connection not ready to invoke MoveCharacter.");
    }
  }, [connection]);

  const sendMessage = useCallback((message: string) => {
      if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke('SendMessage', message)
          .catch(err => console.error(err));
      } else {
        console.warn("SignalR connection not ready to invoke SendMessage.");
      }
    }, [connection])

  const startAutoWalk = useCallback((destX: number, destY: number) => {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        // Call the C# GameHub method 'StartAutoWalk'
        connection.invoke('StartAutoWalk', destX, destY)
            .catch(err => console.error(err));
    }
  }, [connection])

  return { connection, gameState, connectionStatus, error, connect, sendMovement, sendMessage, startAutoWalk, messages };
};