// components/filters/CheckboxFilterWithShowMore.tsx
"use client";

import { useState } from "react";
import { CheckboxFilter } from "./checkboxFilter";

interface Props {
  title: string;
  options: string[];
  values: Record<string, boolean>;
  onChange: (key: string, value: boolean) => void;
  isOpen: boolean;
  toggleOpen: () => void;
  defaultVisibleCount?: number;
}

export const CheckboxFilterWithShowMore: React.FC<Props> = ({
  title,
  options,
  values,
  onChange,
  isOpen,
  toggleOpen,
  defaultVisibleCount = 3,
}) => {
  const [showMore, setShowMore] = useState(false);

  const visibleOptions = showMore
    ? options
    : options.slice(0, defaultVisibleCount);

  return (
    <div>
      <CheckboxFilter
        title={title}
        options={visibleOptions}
        values={values}
        onChange={onChange}
        isOpen={isOpen}
        toggleOpen={toggleOpen}
      />
      {isOpen && options.length > defaultVisibleCount && (
        <button
          onClick={() => setShowMore((prev) => !prev)}
          className="flex items-center gap-1 text-sm text-white mt-1 hover:opacity-80 cursor-pointer"
        >
          <span className="text-xl leading-none">•••</span>
          {showMore ? "скрыть" : "больше"}
        </button>
      )}
    </div>
  );
};
