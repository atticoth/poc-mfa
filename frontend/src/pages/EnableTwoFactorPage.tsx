import { useState } from 'react';
import { api } from '../services/api';

export const EnableTwoFactorPage = () => {
  const [qrUri, setQrUri] = useState('');
  const [secret, setSecret] = useState('');

  const handleEnable = async () => {
    const response = await api.post('/auth/2fa/enable');
    setQrUri(response.data.qrUri);
    setSecret(response.data.secret);
  };

  return (
    <div>
      <h1>Ativar 2FA</h1>
      <button onClick={handleEnable}>Gerar QRCode</button>
      {qrUri && (
        <div>
          <p>Secret: {secret}</p>
          <img
            src={`https://api.qrserver.com/v1/create-qr-code/?data=${encodeURIComponent(qrUri)}`}
            alt="QR Code"
          />
        </div>
      )}
    </div>
  );
};
