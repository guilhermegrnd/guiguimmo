import { useEffect } from 'react';
import type { FC } from 'react';
import { useAuth } from '../context/AuthContext';

export const SigninOidcCallback: FC = () => {
  const { processSigninCallback } = useAuth();

  useEffect(() => {
    processSigninCallback().catch(err => console.error('Error in SigninOidcCallback:', err));
  }, [processSigninCallback]);

  return (
    <div className="flex justify-center items-center h-screen bg-gray-50">
      <div className="p-8 bg-white rounded-xl shadow-2xl">
        <p className="text-xl text-indigo-600 animate-pulse">Processing authentication... Please wait.</p>
      </div>
    </div>
  );
};
