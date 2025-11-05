import type { FC } from 'react'
import { Link } from 'react-router-dom'
import { ROUTE_PATHS } from '../routes/paths'

export const Admin: FC = () => (
  <div className="p-8">
    <h2 className="text-2xl font-bold">Admin Area</h2>
    <p className="mt-2">Only users with the required role can see this.</p>
    <Link to={ROUTE_PATHS.HOME}>Back home</Link>
  </div>
);