import React from 'react';

export const Loading: React.FC = () => (
  <div
    style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 9999,
      fontSize: '18px',
    }}
  >
    Loading...
  </div>
);

export default Loading;
