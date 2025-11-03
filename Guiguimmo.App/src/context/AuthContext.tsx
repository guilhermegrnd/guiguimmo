import React, { createContext, useContext, useState, useEffect, useCallback, useMemo } from 'react';
import type { ReactNode } from 'react';
import { userManager } from '../oidc/userManager';
import type { IOidcUser } from '../types';

interface AuthContextType {
  user: IOidcUser | null;
  isAuthenticated: boolean;
  loading: boolean;
  // videos: Video[];
  signIn: () => Promise<void>;
  signOut: () => Promise<void>;
  processSigninCallback: (url?: string) => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<IOidcUser | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);
  // const [videos, setVideos] = useState<Video[]>([]);

  const handleUserLoaded = useCallback((loadedUser: IOidcUser | null) => {
    setUser(loadedUser);
    setIsAuthenticated(!!loadedUser);
    setLoading(false);

    // if (loadedUser?.access_token) {
    //   fetchProtectedVideos(loadedUser.access_token)
    //     .then((data) => setVideos(data))
    //     .catch((err) => console.error('Error fetching videos in AuthProvider:', err));
    // } else {
    //   setVideos([]);
    // }
  }, []);

  useEffect(() => {
    userManager.clearStaleState();

    userManager.getUser().then(u => handleUserLoaded(u as IOidcUser)).catch(err => {
      console.error('Error loading stored user in AuthProvider:', err);
      handleUserLoaded(null);
    });

    const userLoadedHandler = (u: IOidcUser) => handleUserLoaded(u);
    userManager.events.addUserLoaded(userLoadedHandler);

    return () => userManager.events.removeUserLoaded(userLoadedHandler);
  }, [handleUserLoaded]);

  const signIn = useCallback(async () => {
    try {
      await userManager.signinRedirect();
    } catch (err) {
      console.error('SignIn error from AuthProvider:', err);
    }
  }, []);

  const signOut = useCallback(async () => {
    try {
      await userManager.signoutRedirect();
    } catch (err) {
      console.error('SignOut error from AuthProvider:', err);
      await userManager.removeUser();
      handleUserLoaded(null);
    }
  }, [handleUserLoaded]);

  const processSigninCallback = useCallback(async (url = globalThis.location.href) => {
    try {
      const signedInUser = await userManager.signinRedirectCallback(url);
      handleUserLoaded(signedInUser as IOidcUser);
      globalThis.history.replaceState({}, document.title, globalThis.location.origin);
    } catch (err) {
      console.error('Error in processSigninCallback:', err);
      handleUserLoaded(null);
      globalThis.history.replaceState({}, document.title, globalThis.location.origin);
    }
  }, [handleUserLoaded]);

  const value = useMemo(() => ({ user, isAuthenticated, loading, signIn, signOut, processSigninCallback }), [user, isAuthenticated, loading, signIn, signOut, processSigninCallback]);

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};

export default AuthContext;
