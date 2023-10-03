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

export function camelCaseToWords(s: string | null | undefined) {
  if (!s) return s;
  const result = s.replace(/([A-Z])/g, " $1");
  return result.charAt(0).toUpperCase() + result.slice(1);
}
