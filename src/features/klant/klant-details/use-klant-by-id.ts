import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  throwIfNotOk,
} from "@/services";
import type { Ref } from "vue";
import { fetchKlantById } from "@/services/openklant2";
import type { Klant } from "@/services/openklant1/types";

export const useKlantById = (
  id: Ref<string>,
  gebruikKlantInteractiesApi: Ref<boolean | null>,
) => {
  //OK1 ////////////////////////////////////////////////////
  function mapKlant(obj: any): Klant {
    const { subjectIdentificatie, url } = obj ?? {};
    const { inpBsn, vestigingsNummer, innNnpId } = subjectIdentificatie ?? {};
    const urlSplit: string[] = url?.split("/") ?? [];

    return {
      ...obj,
      id: urlSplit[urlSplit.length - 1],
      _typeOfKlant: "klant",
      bsn: inpBsn,
      vestigingsnummer: vestigingsNummer,
      url: url,
      nietNatuurlijkPersoonIdentifier: innNnpId,
      emailadressen: [],
      telefoonnummers: [],
    };
  }

  function getKlantIdUrl(id?: string) {
    const klantenBaseUrl = "/api/klanten/api/v1/klanten";
    const klantRootUrl = new URL(document.location.href);
    klantRootUrl.pathname = klantenBaseUrl;

    if (!id) return "";
    const url = new URL(`${klantRootUrl}/${id}`);
    return url.toString();
  }

  function fetchKlantByIdOk1(url: string) {
    return fetchLoggedIn(url).then(throwIfNotOk).then(parseJson).then(mapKlant);
  }

  /////////////////////////////////////////////////////////

  const getApiSpecifickUrl = () => {
    if (gebruikKlantInteractiesApi.value === null) {
      return "";
    }

    if (gebruikKlantInteractiesApi.value) {
      return id.value || "";
    } else {
      return getKlantIdUrl(id.value);
    }
  };

  const fetchApiSpecificKlant = () => {
    // if (gebruikKlantInteractiesApi.value === null) {
    //   return null;
    // }

    if (gebruikKlantInteractiesApi.value) {
      return fetchKlantById;
    } else {
      return fetchKlantByIdOk1;
    }
  };

  const fetchIets = (
    url: string,
    gebruikKlantinteractiesApi: Ref<boolean | null>,
  ) => {
    if (gebruikKlantinteractiesApi.value) {
      return fetchKlantById(url);
    } else {
      return fetchKlantByIdOk1(url);
    }
  };

  // return ServiceResult.fromFetcher(getUrl, (u: string) =>
  //   fetchContactverzoeken(u, gebruikKlantInteractiesApi),
  // );

  return ServiceResult.fromFetcher(getApiSpecifickUrl, (u: string) =>
    fetchIets(u, gebruikKlantInteractiesApi),
  );
};
