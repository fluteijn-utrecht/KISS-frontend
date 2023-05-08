export interface Werkbericht {
  id: string;
  title: string;
  date: Date;
  content: string;
  type: string;
  skills: { id: number; naam: string }[];
  read: boolean;
  url: string;
  featured: boolean;
}
