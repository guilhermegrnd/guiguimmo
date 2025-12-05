import type { FC } from 'react'
import { Loading } from '../components/Loading'
import { useAuth } from '../context/AuthContext'
import { TokenDetails } from '../components/TokenDetails'
import { ProfileClaims } from '../components/ProfileClaims'

export const Login: FC = () => {
  const { user, isAuthenticated, loading, signIn, signOut } = useAuth();

  if (loading) return (
    <Loading />
  );

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-8 font-['Inter']">
      <header className="py-6 border-b border-indigo-100 mb-8 flex justify-between items-center">
        <h1 className="text-3xl font-extrabold text-indigo-700">React OIDC Client (TSX)</h1>
        {isAuthenticated && (
          <button
            onClick={signOut}
            className="px-6 py-2 bg-red-600 text-white font-semibold rounded-lg shadow-md hover:bg-red-700 transition duration-150 transform hover:scale-[1.02]"
          >
            Sign Out
          </button>
        )}
      </header>

      <main className="max-w-4xl mx-auto">
        {isAuthenticated ? (
          <div className="bg-white rounded-2xl shadow-xl p-6 sm:p-10">
            <h2 className="text-3xl font-bold text-green-600 mb-6">Authentication Successful! 🎉</h2>

            <div className="space-y-4">
              <p className="text-lg text-gray-700">
                Welcome back, <span className="font-semibold text-indigo-600">{user?.profile?.name || user?.profile?.sub}</span>.
              </p>

              <TokenDetails user={user} />

              <ProfileClaims profile={user?.profile ?? { sub: 'unknown' }} />
            </div>
          </div>
        ) : (
          <div className="p-10 bg-white rounded-2xl shadow-xl text-center">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Welcome!</h2>
            <p className="text-gray-600 mb-6">You are not currently authenticated. Click below to sign in via the MVC Identity Server.</p>
            <button
              onClick={signIn}
              className="w-full sm:w-auto px-8 py-3 bg-indigo-600 text-white font-bold text-lg rounded-xl shadow-lg hover:bg-indigo-700 transition duration-200 transform hover:scale-105"
            >
              Sign In
            </button>
            <p className="mt-4 text-sm text-gray-400">(This action will redirect you to your Authority: <span className="font-mono text-xs">{''}</span>)</p>
          </div>
        )}
      </main>
    </div>
  );
};