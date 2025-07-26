interface Props {
  onClick?: () => void;
}

export const RandomTrackButton: React.FC<Props> = ({ onClick }) => (
  <button onClick={onClick} className="bg-primary text-black px-3 py-1 rounded">
    случайный трек!
  </button>
);
