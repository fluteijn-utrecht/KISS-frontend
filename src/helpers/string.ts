export function fullName(name: any) {
  const {
    voorletters = "",
    voornaam = "",
    voorvoegselAchternaam = "",
    achternaam = "",
  } = name || {};

  return [voornaam || voorletters, voorvoegselAchternaam, achternaam]
    .filter(Boolean)
    .join(" ");
}

export function camelCaseToSentence(s: string | null | undefined) {
  if (!s) return s;
  const words = s.replace(/([A-Z])/g, (c) => " " + c.toLowerCase());
  return words.charAt(0).toUpperCase() + words.slice(1);
}
