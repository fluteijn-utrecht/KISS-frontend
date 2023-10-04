<template>
  <simple-spinner v-if="zaak.loading" />

  <application-message
    v-if="zaak.error"
    messageType="error"
    message="Er kon geen zaak gevonden worden"
  ></application-message>

  <template v-if="zaak.success">
    <back-link />

    <utrecht-heading :level="1"
      >Zaak {{ zaak.data.identificatie }}</utrecht-heading
    >

    <tab-list v-model="activeTab">
      <tab-list-item label="Algemeen">
        <zaak-algemeen :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-item
        label="Documenten"
        :disabled="!zaak.data.documenten?.length"
      >
        <zaak-documenten :zaak="zaak.data" />
      </tab-list-item>
      <tab-list-data-item
        label="Contactmomenten"
        :data="contactmomenten"
        :disabled="(c) => !c.count"
      >
        <template #success="{ data }">
          <div class="contactmomenten">
            <utrecht-heading :level="2"> Contactmomenten </utrecht-heading>
            <contactmomenten-overzicht :contactmomenten="data.page">
              <template #object="{ object }">
                <zaak-preview :zaakurl="object.object"></zaak-preview>
              </template>
            </contactmomenten-overzicht>
          </div>
        </template>
      </tab-list-data-item>
      <tab-list-data-item
        label="Notificaties"
        :data="notificaties"
        :disabled="(c) => !c.count"
      >
        <template #success="{ data }">
          <utrecht-heading :level="2"> Notificaties </utrecht-heading>
          <notificaties-overzicht :notificaties="data.page">
            <template v-slot:object="{ object }">
              <zaak-preview :zaakurl="object.object"></zaak-preview>
            </template>
          </notificaties-overzicht>
        </template>
      </tab-list-data-item>
      <tab-list-data-item
        label="Taken"
        :data="taken"
        :disabled="(c) => !c.count"
      >
        <template #success="{ data }">
          <utrecht-heading :level="2"> Taken</utrecht-heading>
          <klant-taken-overzicht :taken="data.page">
            <template v-slot:zaak="{ url }">
              <zaak-preview :zaakurl="url"></zaak-preview>
            </template>
          </klant-taken-overzicht>
        </template>
      </tab-list-data-item>
    </tab-list>
    <simple-spinner
      v-if="addStatus.loading || statussen.loading || zaak.loading"
    />
    <form
      class="hackathon"
      v-else-if="statussen.success && zaak.success"
      @submit.prevent="
        addStatus
          .submit({
            zaak: zaak.data.url,
            statustype,
          })
          .then(() => zaak.refresh())
      "
    >
      <label>
        Statustype
        <select v-model="statustype" required>
          <option
            v-for="{ url, omschrijving } in statussen.data.page"
            :value="url"
            :key="url"
          >
            {{ omschrijving }}
          </option>
        </select>
      </label>
      <button>Voeg status toe</button>
    </form>
    <simple-spinner v-if="createTaak.loading" />
    <form
      class="hackathon"
      v-else
      @submit.prevent="
        createTaak
          .submit({
            title,
            data: {},
            verwerker_taak_id: '123',
            status: 'open',
            formulier: {
              type: 'id',
              value: '123',
            },
            verloopdatum: new Date().toISOString(),
            verzonden_data: {},
            identificatie: zaak.data.aanvragerBsn
              ? {
                  type: 'bsn',
                  value: zaak.data.aanvragerBsn,
                }
              : {},
            zaak: zaak.data.self,
          })
          .then(() => taken.refresh())
          .then(() =>
            toast({
              text: 'Gelukt!',
            }),
          )
          .catch(() =>
            toast({
              text: 'er ging iets mis',
              type: 'error',
            }),
          )
      "
    >
      <label>
        Titel
        <input required v-model="title" />
      </label>
      <button>Voeg taak toe</button>
    </form>
  </template>
</template>

<script setup lang="ts">
import {
  useAddStatusToZaak,
  useZaakById,
  useZaakStatustypen,
} from "@/features/zaaksysteem/service";
import ApplicationMessage from "@/components/ApplicationMessage.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { computed, ref, watch } from "vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import ZaakDocumenten from "@/features/zaaksysteem/components/ZaakDocumenten.vue";
import ZaakAlgemeen from "@/features/zaaksysteem/components/ZaakAlgemeen.vue";
import { Heading as UtrechtHeading } from "@utrecht/component-library-vue";
import {
  ContactmomentenOverzicht,
  NotificatiesOverzicht,
  useContactmomentenByObjectUrl,
} from "@/features/contactmoment";
import ZaakPreview from "@/features/zaaksysteem/components/ZaakPreview.vue";
import { TabList, TabListItem, TabListDataItem } from "@/components/tabs";
import BackLink from "@/components/BackLink.vue";
import {
  useCreateTaak,
  useKlantTakenByZaakUrl,
} from "@/features/klanttaak/service";
import KlantTakenOverzicht from "@/features/klanttaak/KlantTakenOverzicht.vue";
import { toast } from "@/stores/toast";
const props = defineProps<{ zaakId: string }>();
const contactmomentStore = useContactmomentStore();
const zaak = useZaakById(computed(() => props.zaakId));
const zaakUrl = computed(() =>
  zaak.success && zaak.data.self ? zaak.data.self : "",
);
const contactmomenten = useContactmomentenByObjectUrl(zaakUrl, "klant");
const notificaties = useContactmomentenByObjectUrl(zaakUrl, "gemeente");

const activeTab = ref("");
const title = ref("");
const taken = useKlantTakenByZaakUrl(() => zaakUrl.value);
const statussen = useZaakStatustypen(() =>
  !zaak.success ? "" : zaak.data.zaaktypeUrl,
);
const addStatus = useAddStatusToZaak();
const statustype = ref("");

const createTaak = useCreateTaak();

watch(
  () => zaak.success && zaak.data,
  (z) => {
    if (!z || !contactmomentStore.huidigContactmoment) return;
    contactmomentStore.upsertZaak(
      z,
      contactmomentStore.huidigContactmoment.huidigeVraag,
    );
  },
  { immediate: true },
);
</script>
