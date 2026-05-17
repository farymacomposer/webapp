import { env } from '@shared/config/env.ts';
import cls from './TwitchChat.module.scss';

export const TwitchChat = () => {
  return (
    <div className={cls.chatWrapper}>
      <iframe
        id="chat_embed"
        src={`https://www.twitch.tv/embed/farymacomposer/chat?parent=${env.domen}`}
        allowFullScreen
        className={cls.chat}
      />
    </div>
  );
};
