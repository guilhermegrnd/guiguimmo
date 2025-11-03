import type { FC, ReactElement } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { IOidcUser } from '../types';
import { Loading } from './Loading'

interface ProtectedRouteProps {
  children: ReactElement;
  roles?: string[];
}

const ensureHasRole = (user: IOidcUser | null, roles?: string[]) => {
  console.log('ensureHasRole', { user, roles });
  if (!roles || roles.length === 0) return true;
  if (!user) return false;
  const profile = (user.profile ?? {}) as Record<string, unknown>;
  const roleValue = profile['role'] ?? profile['roles'] ?? undefined;
  console.log('User role value:', roleValue);
  if (roleValue === undefined || roleValue === null) return false;
  if (Array.isArray(roleValue)) return roles.some(r => (roleValue as unknown[]).includes(r));
  if (typeof roleValue === 'string') return roles.includes(roleValue);
  if (typeof roleValue === 'number') return roles.includes(String(roleValue));
  return false;
};

export const ProtectedRoute: FC<ProtectedRouteProps> = ({ children, roles }) => {
  const { isAuthenticated, loading, user } = useAuth();
  const location = useLocation();

  if (loading) return <Loading />;

  if (!isAuthenticated) return <Navigate to="/" state={{ from: location }} replace />;

  if (!ensureHasRole(user, roles)) {
    return (
      <div className="p-8 bg-white rounded-xl shadow-md text-center text-red-600">
        <h3 className="text-lg font-semibold">Not authorized</h3>
        <p className="text-sm mt-2">You do not have the required role to view this page.</p>
      </div>
    );
  }

  return children;
};
