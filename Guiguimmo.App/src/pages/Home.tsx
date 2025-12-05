import type { FC } from 'react'
import { CharacterSelection } from '../components/CharacterSelection'
import { useAuth } from '../context/AuthContext'

export const Home: FC = () => {
  const { signOut } = useAuth();

  return (
    <div className="min-h-screen flex flex-col">
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
      <main> 
        <CharacterSelection />
      </main>
    </div>
  );
}