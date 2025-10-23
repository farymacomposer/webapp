'use client';

import React, { useEffect, useRef } from 'react';

interface StreamVideoProps {
  channel: string;
}

export const StreamVideo: React.FC<StreamVideoProps> = ({ channel }) => {
  const iframeRef = useRef<HTMLIFrameElement>(null);

  useEffect(() => {
    if (!iframeRef.current) return;

    iframeRef.current.src = `https://player.twitch.tv/?channel=${channel}&parent=${window.location.hostname}&muted=true`;
  }, [channel]);

  return (
    <iframe
      ref={iframeRef}
      allowFullScreen
      className="w-full h-full border-0"
      title="Twitch Stream"
    />
  );
};
