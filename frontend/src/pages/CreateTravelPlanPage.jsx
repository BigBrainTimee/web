import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AlertMessage from '../components/AlertMessage';
import TravelPlanForm from '../components/TravelPlanForm';
import { useAuth } from '../context/AuthContext';
import { ApiError } from '../services/apiClient';
import * as travelPlanService from '../services/travelPlanService';

export default function CreateTravelPlanPage() {
  const { token } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState('');

  async function handleSubmit(payload) {
    setError('');

    try {
      const plan = await travelPlanService.createTravelPlan(token, payload);
      navigate(`/plans/${plan.id}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kreiranje plana nije uspelo.');
    }
  }

  return (
    <div className="page">
      <h1>Novi plan putovanja</h1>
      <AlertMessage message={error} onClose={() => setError('')} />
      <TravelPlanForm submitLabel="Kreiraj plan" onSubmit={handleSubmit} />
    </div>
  );
}
