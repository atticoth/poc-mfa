import { useMemo, useState } from 'react';
import { api } from '../services/api';

export const ResetPasswordPage = () => {
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const params = useMemo(() => new URLSearchParams(window.location.search), []);
  const email = params.get('email');
  const token = params.get('token');

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!email || !token) {
      setMessage('Link inválido.');
      return;
    }

    await api.post('/auth/reset-password', { email, token, newPassword: password });
    setMessage('Senha atualizada.');
  };

  return (
    <div>
      <h1>Resetar senha</h1>
      {message && <p>{message}</p>}
      <form onSubmit={handleSubmit}>
        <input
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Nova senha"
          type="password"
        />
        <button type="submit">Resetar</button>
      </form>
    </div>
  );
};
