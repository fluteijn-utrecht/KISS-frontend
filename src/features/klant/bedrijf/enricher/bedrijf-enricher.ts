import type { Klant } from "../../types";
import { useKlantByIdentifier } from "../../service";
import { combineEnrichers } from "@/services";
import { useBedrijfByIdentifier } from "../service/UseGetOndernememing";
import type { Bedrijf } from "../types";
import {
  NietNatuurlijkPersoonIdentifiers,
  usePreferredNietNatuurlijkPersoonIdentifier,
} from "../service/UsePreferredNietNatuurlijkPersoonIdentifier";

const isKlant = (klantOfBedrijf: Klant | Bedrijf): klantOfBedrijf is Klant => {
  return klantOfBedrijf._typeOfKlant === "klant";
};

//dit hebben we alleen nodig als we niet op vestigingsnummer maar een andere identifier
//moeten matchen. Door incompatabiliteit tussen esuite en openklant
//moet de gebruikte matching gegeven configurabel zijn.
//aangezien de combineenrichers geen async functie support voor
//het vaststellen van de gedeelde identifier, moeten we  het te gebruiken veld dan toch mar preventief
//hier alvast binnenhalen
const preferredNietNatuurlijkPersoonIdentifier =
  await usePreferredNietNatuurlijkPersoonIdentifier();

export const useEnrichedBedrijf = combineEnrichers(
  useKlantByIdentifier,
  useBedrijfByIdentifier,
  GetSharedIdentifier, //de property waarmee je het bijbehorende object in de andere bron gaat zoeken
  isKlant,
);

function GetSharedIdentifier(
  klantofbedrijf: Klant | Bedrijf,
): BedrijfSearchParameter | undefined {
  //als er een vestigingsnummer is dan is dat mooi
  //zowel kvk als de klantregisters kennen dit
  if ("vestigingsnummer" in klantofbedrijf && klantofbedrijf.vestigingsnummer) {
    return { vestigingsnummer: klantofbedrijf.vestigingsnummer };
  } else {
    //als we gevegens hebben van een bedrijf (kvk data) en we
    //willen daar de bijbehorende klant bij zoeken
    //dan moeten we afhanekelijk van de context waarin KISS draait (openklant of e-suite)
    //het kvknummer (e-suite) of rsin (openklant) gebruiken om de bijbehordengegeevesn te zoeken
    // welk gegeven gebruikt moet worden kunnen we opvragen

    if (klantofbedrijf._typeOfKlant === "bedrijf") {
      if (
        preferredNietNatuurlijkPersoonIdentifier.NietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.rsin
      ) {
        //dus.... we hebben een bedrijf en we willen de bijbehorende klant zoeken
        //op basis van de rsin die in openklant innNnpId heet

        if ("rsin" in klantofbedrijf && klantofbedrijf.rsin) {
          console.log(
            "we hebben een kvk record en willen het bijbehorende klant record zoeken adhv de rsin van de kvk die moet matchen met de innNnpId van openklant",
          );
          return { innNnpId: klantofbedrijf.rsin };
        }
      }

      if (
        preferredNietNatuurlijkPersoonIdentifier.NietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.kvkNummer
      ) {
        //dus.... we hebben een bedrijf en we willen de bijbehorende klant zoeken
        //op basis van de rsin die in openklant innNnpId heet

        if ("rsin" in klantofbedrijf && klantofbedrijf.kvkNummer) {
          console.log(
            "we hebben een kvk record en willen het bijbehorende klant record zoeken adhv het kvknummer van de kvk die moet matchen met het kvknummer in de e-suite",
          );
          return { kvkNummer: klantofbedrijf.kvkNummer };
        }
      }
    }

    if (klantofbedrijf._typeOfKlant === "klant") {
      if (
        preferredNietNatuurlijkPersoonIdentifier.NietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.rsin
      ) {
        //dus.... we hebben een klant en we willen de bijbehorende onderneming zoeken bij de kvk
        //op basis van de rsin die in openklant innNnpId heet

        if ("rsin" in klantofbedrijf && klantofbedrijf.innNnpId) {
          console.log(
            "we hebben een klant record en willen het bijbehorende kvk record zoeken adhv het innNnPid van openklant die moet matchen met de rsin in de kvk",
          );
          return { rsin: klantofbedrijf.innNnpId };
        }
      }

      if (
        preferredNietNatuurlijkPersoonIdentifier.NietNatuurlijkPersoonIdentifier ===
        NietNatuurlijkPersoonIdentifiers.kvkNummer
      ) {
        //dus.... we hebben een klant en we willen de bijbehorende onderneming zoeken bij de kvk
        //op basis van het kvknummer

        if ("rsin" in klantofbedrijf && klantofbedrijf.kvkNummer) {
          console.log(
            "we hebben een klant record en willen het bijbehorende kvk record zoeken adhv het kvknummer van de e-suite die moet matchen met het kvknummer in de kvk",
          );
          return { kvkNummer: klantofbedrijf.kvkNummer };
        }
      }
    }
  }

  // if (klantofbedrijf.vestigingsnummer) {
  //   return { vestigingsnummer: klantofbedrijf.vestigingsnummer };
  // } else if (klantofbedrijf.innNnpId) {
  //   return { innNnpId: klantofbedrijf.innNnpId };
  // } else if (klantofbedrijf.kvkNummer) {
  //   return { kvkNummer: klantofbedrijf.kvkNummer };
  // }
  return;
}

export type BedrijfSearchParameter =
  | {
      vestigingsnummer: string;
    }
  | {
      kvkNummer: string;
    }
  | {
      innNnpId: string;
    }
  | {
      rsin: string;
    };
