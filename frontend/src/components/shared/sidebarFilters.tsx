"use client";

import { useState } from "react";
import { cn } from "@/lib/utils";
import { CheckboxFilter } from "./sidebar/checkboxFilter";
import { CheckboxFilterWithShowMore } from "./sidebar/checkboxFilterWithShowMore";
import { RangeFilter } from "./sidebar/rangeFilter";
import { TextFilter } from "./sidebar/textFilter";

const ratings = [
  "гениально",
  "атлична",
  "хорошечно",
  "нормас",
  "ну такое",
  "кринж-контент",
];
const genres = ["поп", "рок", "метал", "рэп", "электронное", "инди"];
const userRatings = ["отлично", "нормально", "так себе", "кринж"];
const tags = ["разнос от композитора", "рецензии пользователей"];
const origins = ["игровой ost", "аниме op/ed/ost", "фильм/сериал ost"];

interface Props {
  className?: string;
}

interface FiltersState {
  rating: Record<string, boolean>;
  userRating: Record<string, boolean>;
  genre: Record<string, boolean>;
  tags: Record<string, boolean>;
  origin: Record<string, boolean>;
  showMoreGenres: boolean;
  yearFrom: number;
  yearTo: number;
  country: string;
}

export const SidebarFilters: React.FC<Props> = ({ className }) => {
  const [openSections, setOpenSections] = useState<Record<string, boolean>>({
    rating: true,
    userRating: true,
    genre: true,
    year: true,
    country: true,
    tags: true,
    origin: true,
  });

  const [filters, setFilters] = useState<FiltersState>({
    rating: Object.fromEntries(ratings.map((v) => [v, false])),
    userRating: Object.fromEntries(userRatings.map((v) => [v, false])),
    genre: Object.fromEntries(genres.map((v) => [v, false])),
    tags: Object.fromEntries(tags.map((v) => [v, false])),
    origin: Object.fromEntries(origins.map((v) => [v, false])),
    showMoreGenres: false,
    yearFrom: 2017,
    yearTo: 2025,
    country: "Япония",
  });

  const toggleSection = (key: string) =>
    setOpenSections((prev) => ({ ...prev, [key]: !prev[key] }));

  return (
    <div
      className={cn(
        "rounded-xl bg-header px-4 py-6 space-y-4 shadow-md min-w-[250px]",
        className
      )}
    >
      <CheckboxFilter
        title="оценка композитора"
        options={ratings}
        values={filters.rating}
        onChange={(k, v) =>
          setFilters((prev) => ({
            ...prev,
            rating: { ...prev.rating, [k]: v },
          }))
        }
        isOpen={openSections.rating}
        toggleOpen={() => toggleSection("rating")}
      />

      <CheckboxFilter
        title="оценка пользователей"
        options={userRatings}
        values={filters.userRating}
        onChange={(k, v) =>
          setFilters((prev) => ({
            ...prev,
            userRating: { ...prev.userRating, [k]: v },
          }))
        }
        isOpen={openSections.userRating}
        toggleOpen={() => toggleSection("userRating")}
      />

      <CheckboxFilterWithShowMore
        title="жанр"
        options={genres}
        values={filters.genre}
        onChange={(k, v) =>
          setFilters((prev) => ({ ...prev, genre: { ...prev.genre, [k]: v } }))
        }
        isOpen={openSections.genre}
        toggleOpen={() => toggleSection("genre")}
      />

      <RangeFilter
        title="год релиза"
        from={filters.yearFrom}
        to={filters.yearTo}
        onChangeFrom={(v) => setFilters((prev) => ({ ...prev, yearFrom: v }))}
        onChangeTo={(v) => setFilters((prev) => ({ ...prev, yearTo: v }))}
        isOpen={openSections.year}
        toggleOpen={() => toggleSection("year")}
      />

      <TextFilter
        title="страна"
        value={filters.country}
        onChange={(v) => setFilters((prev) => ({ ...prev, country: v }))}
        isOpen={openSections.country}
        toggleOpen={() => toggleSection("country")}
      />

      <CheckboxFilter
        title="разнос"
        options={tags}
        values={filters.tags}
        onChange={(k, v) =>
          setFilters((prev) => ({ ...prev, tags: { ...prev.tags, [k]: v } }))
        }
        isOpen={openSections.tags}
        toggleOpen={() => toggleSection("tags")}
      />

      <CheckboxFilter
        title="происхождение"
        options={origins}
        values={filters.origin}
        onChange={(k, v) =>
          setFilters((prev) => ({
            ...prev,
            origin: { ...prev.origin, [k]: v },
          }))
        }
        isOpen={openSections.origin}
        toggleOpen={() => toggleSection("origin")}
      />

      <div className="flex justify-center mt-2">
        <button className="w-[100px] mt-2 py-2 rounded-sm bg-primary text-black font-semibold transition hover:bg-primary/80">
          найти
        </button>
      </div>
    </div>
  );
};
