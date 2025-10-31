import type { FC } from 'react';
import type { IOidcUser } from '../types';

export const TokenDetails: FC<{ user: IOidcUser | null }> = ({ user }) => {
  return (
    <div className="border border-gray-200 rounded-xl p-4">
      <h3 className="text-xl font-semibold text-gray-800 mb-3">Token Details</h3>
      <div className="space-y-2 text-sm break-all">
        <p className="font-mono bg-gray-100 p-2 rounded">
          <strong className="text-indigo-500 mr-2">Access Token:</strong> {user?.access_token ? user.access_token.substring(0, 40) + '...' : 'N/A'}
        </p>
        <p className="font-mono bg-gray-100 p-2 rounded">
          <strong className="text-indigo-500 mr-2">ID Token:</strong> {user?.id_token ? user.id_token.substring(0, 40) + '...' : 'N/A'}
        </p>
        <p className="text-sm">
          <strong className="text-indigo-500">Token expiration:</strong> <span className="font-medium text-gray-600">{user?.expires_at ? new Date(user.expires_at * 1000).toLocaleString() : 'N/A'}</span>
        </p>
      </div>
    </div>
  );
};
