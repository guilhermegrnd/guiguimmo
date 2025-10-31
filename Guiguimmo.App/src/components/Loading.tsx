import type { FC } from 'react'

export const Loading: FC = () => {
  return (
    <div className="flex justify-center items-center h-screen bg-gray-50">
      <div className="p-8 bg-white rounded-xl shadow-2xl">
        <p className="text-xl text-indigo-600 animate-pulse">Loading... Please wait.</p>
      </div>
    </div>
  );
}