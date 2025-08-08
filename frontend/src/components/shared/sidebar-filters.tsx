"use client";

import { useState } from "react";
import { Checkbox, Label } from "../ui";
import { ChevronDown } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { cn } from "@/lib/utils";

interface Props {
  className?: string;
}

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

  // Состояния чекбоксов для каждого раздела
  const [ratingChecks, setRatingChecks] = useState<Record<string, boolean>>(
    Object.fromEntries(ratings.map((v) => [v, false]))
  );
  const [userRatingChecks, setUserRatingChecks] = useState<
    Record<string, boolean>
  >(Object.fromEntries(userRatings.map((v) => [v, false])));
  const [genreChecks, setGenreChecks] = useState<Record<string, boolean>>(
    Object.fromEntries(genres.map((v) => [v, false]))
  );
  const [tagChecks, setTagChecks] = useState<Record<string, boolean>>(
    Object.fromEntries(tags.map((v) => [v, false]))
  );
  const [originChecks, setOriginChecks] = useState<Record<string, boolean>>(
    Object.fromEntries(origins.map((v) => [v, false]))
  );

  const [showMoreGenres, setShowMoreGenres] = useState(false);
  const [yearFrom, setYearFrom] = useState(2017);
  const [yearTo, setYearTo] = useState(2025);
  const [country, setCountry] = useState("Япония");

  const toggleSection = (key: string) => {
    setOpenSections((prev) => ({
      ...prev,
      [key]: !prev[key],
    }));
  };

  return (
    <div
      className={cn(
        "rounded-xl bg-header px-4 py-6 space-y-4 shadow-md min-w-[250px]",
        className
      )}
    >
      {/* Оценка композитора */}
      <div>
        <button
          onClick={() => toggleSection("rating")}
          className="flex items-center w-full mb-2 ид-10"
        >
          <ChevronDown
            className={cn(
              "transition-transform",
              openSections.rating && "rotate-180"
            )}
          />
          <h2 className="font-semibold">оценка композитора</h2>
        </button>
        <AnimatePresence initial={false}>
          {openSections.rating && (
            <motion.div
              className="space-y-1 flex flex-col text-filter-text"
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: "auto", opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
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
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Оценка пользователей */}
      <div>
        <button
          onClick={() => toggleSection("userRating")}
          className="flex items-center w-full mb-2"
        >
          <ChevronDown
            className={cn(
              "transition-transform",
              openSections.userRating && "rotate-180"
            )}
          />
          <h2 className="font-semibold">оценка пользователей</h2>
        </button>
        <AnimatePresence initial={false}>
          {openSections.userRating && (
            <motion.div
              className="space-y-1 flex flex-col text-filter-text"
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: "auto", opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
              {userRatings.map((item) => (
                <div key={item} className="flex items-center gap-2">
                  <Checkbox
                    checked={userRatingChecks[item]}
                    onCheckedChange={(checked) =>
                      setUserRatingChecks((prev) => ({
                        ...prev,
                        [item]: Boolean(checked),
                      }))
                    }
                  />
                  <Label>{item}</Label>
                </div>
              ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Жанр */}
      <div>
        <button
          onClick={() => toggleSection("genre")}
          className="flex items-center w-full mb-2"
        >
          <ChevronDown
            className={cn(
              "transition-transform",
              openSections.genre && "rotate-180"
            )}
          />
          <h2 className="font-semibold">жанр</h2>
        </button>
        <AnimatePresence initial={false}>
          {openSections.genre && (
            <motion.div
              className="space-y-1 flex flex-col text-filter-text"
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: "auto", opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
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
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Год релиза */}
      <div>
        <button
          onClick={() => toggleSection("year")}
          className="flex items-center w-full mb-2"
        >
          <ChevronDown
            className={cn(
              "transition-transform",
              openSections.year && "rotate-180"
            )}
          />
          <h2 className="font-semibold">год релиза</h2>
        </button>
        <AnimatePresence initial={false}>
          {openSections.year && (
            <motion.div
              className="flex items-center gap-2 mb-2"
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: "auto", opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
              <input
                type="number"
                min={1900}
                max={yearTo}
                value={yearFrom}
                onChange={(e) => setYearFrom(Number(e.target.value))}
                className="w-16 rounded px-2 py-1 bg-card border-none text-sm"
              />
              <span>–</span>
              <input
                type="number"
                min={yearFrom}
                max={2100}
                value={yearTo}
                onChange={(e) => setYearTo(Number(e.target.value))}
                className="w-16 rounded px-2 py-1 bg-card border-none text-sm"
              />
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Страна */}
      <div>
        <button
          onClick={() => toggleSection("country")}
          className="flex items-center w-full mb-2"
        >
          {" "}
          <ChevronDown
            className={cn(
              "transition-transform",
              openSections.country && "rotate-180"
            )}
          />
          <h2 className="font-semibold">страна</h2>
        </button>
        <AnimatePresence initial={false}>
          {openSections.country && (
            <motion.div
              className="mb-2"
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: "auto", opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
              <input
                type="text"
                value={country}
                onChange={(e) => setCountry(e.target.value)}
                className="rounded px-2 py-1 bg-card border-none text-sm w-full"
              />
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Разносы */}
      <div>
        <button
          onClick={() => toggleSection("tags")}
          className="flex items-center w-full mb-2"
        >
          <ChevronDown
            className={cn(
              "transition-transform",
              openSections.tags && "rotate-180"
            )}
          />
          <h2 className="font-semibold">разнос</h2>
        </button>
        <AnimatePresence initial={false}>
          {openSections.tags && (
            <motion.div
              className="space-y-1 flex flex-col text-filter-text"
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: "auto", opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
              {tags.map((tag) => (
                <div key={tag} className="flex items-center gap-2">
                  <Checkbox
                    checked={tagChecks[tag]}
                    onCheckedChange={(checked) =>
                      setTagChecks((prev) => ({
                        ...prev,
                        [tag]: Boolean(checked),
                      }))
                    }
                  />
                  <Label>{tag}</Label>
                </div>
              ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Происхождение */}
      <div>
        <button
          onClick={() => toggleSection("origin")}
          className="flex items-center w-full mb-2"
        >
          <ChevronDown
            className={cn(
              "transition-transform",
              openSections.origin && "rotate-180"
            )}
          />
          <h2 className="font-semibold">происхождение</h2>
        </button>
        <AnimatePresence initial={false}>
          {openSections.origin && (
            <motion.div
              className="space-y-1 flex flex-col text-filter-text"
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: "auto", opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
              {origins.map((origin) => (
                <div key={origin} className="flex items-center gap-2">
                  <Checkbox
                    checked={originChecks[origin]}
                    onCheckedChange={(checked) =>
                      setOriginChecks((prev) => ({
                        ...prev,
                        [origin]: Boolean(checked),
                      }))
                    }
                  />
                  <Label>{origin}</Label>
                </div>
              ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      <div className="flex justify-center mt-2">
        <button className="w-[100] mt-2 py-2 rounded-sm bg-primary text-black font-semibold transition hover:bg-primary/80">
          найти
        </button>
      </div>
    </div>
  );
};
