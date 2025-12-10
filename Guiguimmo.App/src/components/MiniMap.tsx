
import React, { type FC } from 'react';
import type { GameState } from '../types/gamestate'

// Tile size for the mini-map grid (must match CSS)
const MINI_TILE_SIZE = 0.7; 

const getMiniTileColor = (tileValue: number) => {
    switch (tileValue) {
        case 1: return '#8c7'; // Grass
        case 2: return '#444'; // Wall
        case 3: return '#4af'; // Water
        default: return '#000';
    }
};

const MiniMap: FC<{ gameState: GameState, characterId: string }>  = ({ gameState, characterId }) => {
    // Find the current player's position
    const currentCharacter = gameState.characters.find(c => c.id === characterId);
    
    // Check if the map is available
    if (!gameState.mapTiles || gameState.mapTiles.length === 0) {
        return <div>Loading map...</div>;
    }

    return (
        <div className="mini-map-container">
            {/* 1. Render the Mini Map Tiles (simplified colors) */}
            {gameState.mapTiles.map((row, y) => (
                <React.Fragment key={y}>
                    {row.map((tileValue, x) => (
                        <div 
                            key={`${x}-${y}`} 
                            className="mini-map-tile"
                            style={{ backgroundColor: getMiniTileColor(parseInt(tileValue)) }}
                        />
                    ))}
                </React.Fragment>
            ))}

            {/* 2. Render the Player Marker */}
            {currentCharacter && (
                <div 
                    className="mini-map-player-dot" 
                    style={{ 
                        left: `${currentCharacter.position.x * MINI_TILE_SIZE}rem`, 
                        top: `${currentCharacter.position.y * MINI_TILE_SIZE}rem` 
                    }}
                />
            )}
        </div>
    );
};

export default MiniMap;