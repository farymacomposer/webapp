import { env } from '@shared/config/env.ts';
import cls from './TwitchPlayer.module.scss';
import { env } from '@shared/config/env.ts';

export const TwitchPlayer = () => {
  return (
    <div className={cls.stream}>
      <iframe
        src={`https://player.twitch.tv/?channel=farymacomposer&parent=${env.domen}`}
        allowFullScreen
        frameBorder="0"
        scrolling="no"
        className={cls.twitchPlayer}
      />
    </div>
  );
};
