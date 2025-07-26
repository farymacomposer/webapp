import Link from "next/link";

interface Props {
  href: string;
  children: React.ReactNode;
}

export const NavItem: React.FC<Props> = ({ href, children }) => (
  <Link href={href} className="hover:underline transition text-white">
    {children}
  </Link>
);
