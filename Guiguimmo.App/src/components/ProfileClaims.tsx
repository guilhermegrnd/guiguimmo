import type { FC } from 'react';
import type { IOidcUser } from '../types';

export const ProfileClaims: FC<{ profile: IOidcUser['profile'] }> = ({ profile }) => {
  return (
    <div className="border border-gray-200 rounded-xl p-4">
      <h3 className="text-xl font-semibold text-gray-800 mb-3">User Profile Claims</h3>
      <ul className="space-y-1 text-sm bg-gray-50 p-3 rounded">
        {Object.entries(profile ?? {}).map(([key, value]) => (
          <li key={key} className="flex">
            <span className="font-medium text-gray-600 w-32 shrink-0">{key}:</span>
            <span className="text-gray-900 break-all">{String(value)}</span>
          </li>
        ))}
      </ul>
    </div>
  );
};
