import cls from './TwitchChat.module.scss';
import { env } from '@shared/config/env.ts';

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
