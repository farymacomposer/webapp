"use client";

import { useState } from "react";
import { Checkbox, Label } from "../ui";

const ratings = [
  "гениально",
  "атлична",
  "хорошечно",
  "нормас",
  "ну такое",
  "кринж-контент",
];

const genres = ["поп", "рок", "метал", "рэп", "электронное", "инди"];

export const SidebarFilters: React.FC = () => {
  const [ratingChecks, setRatingChecks] = useState<Record<string, boolean>>({
    Гениально: false,
    Атлична: false,
    Хорошечно: false,
    Нормас: false,
    "Ну такое": false,
    "Кринж-контент": false,
  });

  const [genreChecks, setGenreChecks] = useState<Record<string, boolean>>({
    поп: false,
    рок: false,
    метал: false,
    рэп: false,
    электронное: false,
    инди: false,
  });

  const [showMoreGenres, setShowMoreGenres] = useState(false);

  return (
    <div className="rounded-xl bg-header px-4 py-6 space-y-4 shadow-md">
      <div>
        <h2 className="font-semibold mb-2">Оценка композитора</h2>
        <div className="space-y-1 flex flex-col text-filter-text">
          {ratings.map((rating) => (
            <div key={rating} className="flex items-center gap-2">
              <Checkbox
                checked={ratingChecks[rating]}
                onCheckedChange={(checked) =>
                  setRatingChecks((prev) => ({
                    ...prev,
                    [rating]: Boolean(checked),
                  }))
                }
              />
              <Label>{rating}</Label>
            </div>
          ))}
        </div>
      </div>

      <div>
        <h2 className="font-semibold mb-2">Жанр</h2>
        <div className="space-y-1 flex flex-col text-filter-text">
          {(showMoreGenres ? genres : genres.slice(0, 3)).map((genre) => (
            <div key={genre} className="flex items-center gap-2">
              <Checkbox
                checked={genreChecks[genre]}
                onCheckedChange={(checked) =>
                  setGenreChecks((prev) => ({
                    ...prev,
                    [genre]: Boolean(checked),
                  }))
                }
              />
              <Label>{genre}</Label>
            </div>
          ))}

          <button
            onClick={() => setShowMoreGenres(!showMoreGenres)}
            className="flex items-center gap-1 text-sm text-white mt-1 hover:opacity-80 cursor-pointer"
          >
            <span className="text-xl leading-none">•••</span>
            {showMoreGenres ? "скрыть" : "больше"}
          </button>
        </div>
      </div>
    </div>
  );
};
