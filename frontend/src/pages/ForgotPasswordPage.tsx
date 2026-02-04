import { useState } from 'react';
import { api } from '../services/api';

export const ForgotPasswordPage = () => {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    await api.post('/auth/forgot-password', {
      email,
      resetBaseUrl: `${window.location.origin}/reset-password`
    });
    setMessage('Se o email existir, enviamos um link.');
  };

  return (
    <div>
      <h1>Esqueci a senha</h1>
      {message && <p>{message}</p>}
      <form onSubmit={handleSubmit}>
        <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
        <button type="submit">Enviar</button>
      </form>
    </div>
  );
};
