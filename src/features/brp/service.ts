import type { Persoon } from "./types";

export function useBrpService() {
  if (!window.brpBaseUri) {
    console.error("brpBaseUri missing");
  }

  const findByNameAndBirthDay = (achternaam: string, geboortedatum: string) => {
    const url = `${window.brpBaseUri}?achternaam=${achternaam}&geboortedatum=${geboortedatum}`;

    return fetch(url, {
      headers: {
        Accept: "application/json+ld",
      },
    })
      .then((r) => {
        if (!r.ok) {
          throw new Error();
        }
        return r.json();
      })

      .then((json) => {
        return json.map((x: Persoon) => {
          return { achternaam: x.achternaam } as Persoon;
        });
      });
  };

  const findByPostalcodeAndHouseNumber = (
    postcode: string,
    huisnummer: string
  ) => {
    const url = `${window.brpBaseUri}?postcode=${postcode}&huisnummer=${huisnummer}`;

    return fetch(url)
      .then((r) => {
        if (!r.ok) {
          throw new Error();
        }
        return r.json();
      })

      .then((json) => {
        if (!Array.isArray(json.results)) {
          throw new Error(
            "Invalide json, verwacht een lijst: " + JSON.stringify(json.results)
          );
        }
        return json.results.map((x: Persoon) => x as Persoon);
      });
  };

  return {
    findByNameAndBirthDay,
    findByPostalcodeAndHouseNumber,
  };
}
