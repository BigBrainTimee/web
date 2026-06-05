import { Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import { AuthProvider } from './context/AuthContext';
import CreateTravelPlanPage from './pages/CreateTravelPlanPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import TravelPlanDetailPage from './pages/TravelPlanDetailPage';
import TravelPlansPage from './pages/TravelPlansPage';

export default function App() {
  return (
    <AuthProvider>
      <Layout>
        <Routes>
          <Route path="/" element={<Navigate to="/plans" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/plans"
            element={(
              <ProtectedRoute>
                <TravelPlansPage />
              </ProtectedRoute>
            )}
          />
          <Route
            path="/plans/new"
            element={(
              <ProtectedRoute>
                <CreateTravelPlanPage />
              </ProtectedRoute>
            )}
          />
          <Route
            path="/plans/:id"
            element={(
              <ProtectedRoute>
                <TravelPlanDetailPage />
              </ProtectedRoute>
            )}
          />
          <Route path="*" element={<Navigate to="/plans" replace />} />
        </Routes>
      </Layout>
    </AuthProvider>
  );
}
