"use client";

import { cn } from "@/lib/utils";
import { Logo } from "./header/logo";
import { NavItem } from "./header/nav-item";
import { RandomTrackButton } from "./header/random-track-button";
import { SearchInput } from "./header/search-input";
import { Avatar, Button } from "../ui";
import { AvatarFallback, AvatarImage } from "../ui/avatar";

interface Props {
  className?: string;
}

export const Header: React.FC<Props> = ({ className }) => {
  return (
    <header
      className={cn("flex items-center justify-between px-6 py-4", className)}
    >
      <div className="flex flex-row items-center gap-4">
        <Logo />
        <SearchInput />
        <RandomTrackButton onClick={() => alert("🎵 Рандомный трек!")} />
        <Button variant="default" className="text-black px-3 py-1 rounded">
          +
        </Button>
        <nav className="flex-1 flex justify-center gap-15 ml-30">
          <NavItem href="#">Главная</NavItem>
          <NavItem href="/stream">Стрим-space</NavItem>
          <NavItem href="/catalog">База треков</NavItem>
          <NavItem href="#">Помощь</NavItem>
        </nav>
      </div>

      <div className=""></div>

      <div className="ml-auto">
        <Avatar>
          <AvatarImage src="https://github.com/shadcn.png" alt="@shadcn" />
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
      </div>
    </header>
  );
};
