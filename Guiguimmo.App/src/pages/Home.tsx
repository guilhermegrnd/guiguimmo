import type { FC } from 'react'
import { GameClient } from './GameClient'

export const Home: FC = () => {
  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-8 font-['Inter']">
      <script src="https://cdn.tailwindcss.com"></script>
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <header className="py-6 border-b border-indigo-100 mb-8 flex justify-between items-center">
        <h1 className="text-3xl font-extrabold text-indigo-700">Home Page</h1>
      </header>
      <main className="max-w-4xl mx-auto">
        <div className="bg-white rounded-2xl shadow-xl p-6 sm:p-10">
          <h2 className="text-3xl font-bold text-green-600 mb-6">Welcome to the Home Page! 🎉</h2>
          <p className="text-lg tex]t-gray-700">
            You have successfully accessed a protected route.
          </p>
          <GameClient />
        </div>
      </main>
    </div>
  );
}