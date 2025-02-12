<template>
  <back-link />

  <utrecht-heading :level="1">Persoonsinformatie</utrecht-heading>

  <tab-list v-model="activeTab">
    <tab-list-data-item label="Contactgegevens" :data="klant">
      <template #success="{ data }">
        <klant-details :klant="data" />
      </template>
    </tab-list-data-item>

    <tab-list-data-item label="BRP gegevens" :data="persoon">
      <template #success="{ data }">
        <brp-gegevens v-if="data" :persoon="data" />
      </template>
    </tab-list-data-item>

    <tab-list-item label="Contactmomenten">
      <template #default="{ setError, setLoading, setDisabled }">
        <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>

        <contactmomenten-for-klant-url
          v-if="gebruikKlantInteracatiesApi != null"
          :klant-url="klantUrl"
          :gebruik-klant-interacties="gebruikKlantInteracatiesApi"
          @load="setDisabled(!$event?.page?.length)"
          @loading="setLoading"
          @error="setError"
        >
          <template #object="{ object }">
            <zaak-preview :zaakurl="object.object" />
          </template>
        </contactmomenten-for-klant-url>
      </template>
    </tab-list-item>

    <tab-list-data-item label="Zaken" :data="zaken" :disabled="(c) => !c.count">
      <template #success="{ data }">
        <utrecht-heading :level="2"> Zaken </utrecht-heading>

        <zaken-overzicht
          :zaken="data.page"
          :vraag="contactmomentStore.huidigContactmoment?.huidigeVraag"
        />
      </template>
    </tab-list-data-item>

    <tab-list-item label="Contactverzoeken">
      <template #default="{ setError, setLoading, setDisabled }">
        <utrecht-heading :level="2">Contactverzoeken</utrecht-heading>

        <contactverzoeken-for-klant-url
          v-if="gebruikKlantInteracatiesApi != null && klantUrl"
          :klant-url="klantUrl"
          :gebruik-klant-interacties="gebruikKlantInteracatiesApi"
          @load="setDisabled(!$event?.page?.length)"
          @loading="setLoading"
          @error="setError"
        >
          <template #object="{ object }">
            <zaak-preview v-if="object.object" :zaakurl="object.object" />
          </template>
        </contactverzoeken-for-klant-url>
      </template>
    </tab-list-item>
  </tab-list>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { KlantDetails, useKlantById } from "@/features/klant/klant-details";
import { useZakenByBsn } from "@/features/zaaksysteem";
import ZakenOverzicht from "@/features/zaaksysteem/ZakenOverzicht.vue";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListDataItem, TabListItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import {
  usePersoonByBsn,
  BrpGegevens,
} from "@/features/persoon/persoon-details";
import ContactverzoekenForKlantUrl from "@/features/contact/contactverzoek/overzicht/ContactverzoekenForKlantUrl.vue";
import ContactmomentenForKlantUrl from "@/features/contact/contactmoment/ContactmomentenForKlantUrl.vue";
import { getRegisterDetails } from "@/features/shared/systeemdetails";

const props = defineProps<{ persoonId: string }>();

const gebruikKlantInteracatiesApi = ref<boolean | null>(null);
const defaultSystemId = ref<string | null>(null);
const activeTab = ref("");
const klantId = computed(() => props.persoonId);
const contactmomentStore = useContactmomentStore();
const klant = useKlantById(
  klantId,
  defaultSystemId,
  gebruikKlantInteracatiesApi,
);

const klantUrl = computed(() => (klant.success ? klant.data.url ?? "" : ""));

onMounted(async () => {
  const { useKlantInteractiesApi, defaultSysteemId } =
    await getRegisterDetails();
  gebruikKlantInteracatiesApi.value = useKlantInteractiesApi;
  defaultSystemId.value = defaultSysteemId;
});

const getBsn = () =>
  !klant.success ||
  !klant.data.bsn ||
  gebruikKlantInteracatiesApi.value === null
    ? ""
    : klant.data.bsn;

const klantBsn = computed(getBsn);

const zaken = useZakenByBsn(klantBsn);
const persoon = usePersoonByBsn(getBsn);

watch(
  [() => klant.success && klant.data, () => persoon.success && persoon.data],
  ([k, p]) => {
    if (!k) return;
    contactmomentStore.setKlant({
      ...k,
      ...p,
      hasContactInformation:
        !!k.emailadressen.length || !!k.telefoonnummers.length,
    });
  },
  { immediate: true },
);
</script>
