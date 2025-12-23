"use client";

import React, { useEffect, useRef } from "react";

interface StreamChatProps {
  channel: string;
}

export const StreamChat: React.FC<StreamChatProps> = ({ channel }) => {
  const iframeRef = useRef<HTMLIFrameElement>(null);

  useEffect(() => {
    if (!iframeRef.current) return;

    iframeRef.current.src = `https://www.twitch.tv/embed/${channel}/chat?darkpopout&parent=${window.location.hostname}`;
  }, [channel]);

  return (
    <iframe
      ref={iframeRef}
      title="Twitch Chat"
      
      className="w-full h-full border-0"
    />
  );
};
