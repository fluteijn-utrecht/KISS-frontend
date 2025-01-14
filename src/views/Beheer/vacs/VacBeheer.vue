<template>
  <utrecht-heading :level="1">Vac</utrecht-heading>

  <simple-spinner v-if="loading" />

  <div v-else-if="error">
    Er is een fout opgetreden bij het ophalen van de Vac.
  </div>

  <template v-else>
    <beheer-form @submit="submit">
      <template #formFields>
        <label class="utrecht-form-label"
          ><span>Vraag</span>
          <input
            class="utrecht-textbox utrecht-textbox--html-input"
            type="text"
            v-model="vac.record.data.vraag"
            required
        /></label>

        <label class="utrecht-form-label" for="antwoord">Antwoord</label>
        <ck-editor v-model="vac.record.data.antwoord" required />

        <fieldset>
          <legend>Afdelingen</legend>
          <ul>
            <li v-for="(value, key) in vac.record.data.afdelingen" :key="key">
              <label class="utrecht-form-label"
                ><span>Afdeling {{ key + 1 }}</span>
                <input
                  class="utrecht-textbox utrecht-textbox--html-input"
                  type="text"
                  v-model="value.afdelingNaam"
              /></label>
            </li>
          </ul>
        </fieldset>

        <label class="utrecht-form-label" for="toelichting">Toelichting</label>
        <ck-editor v-model="vac.record.data.toelichting" />

        <fieldset>
          <legend>Trefwoorden</legend>
          <ul>
            <li v-for="(value, key) in vac.record.data.trefwoorden" :key="key">
              <label class="utrecht-form-label"
                ><span>Trefwoord {{ key + 1 }}</span>
                <input
                  class="utrecht-textbox utrecht-textbox--html-input"
                  type="text"
                  v-model="value.trefwoord"
              /></label>
            </li>
          </ul>
        </fieldset>
      </template>

      <template #formMenuListItems>
        <li>
          <router-link
            to="/Beheer/vacs/"
            class="utrecht-button utrecht-button--secondary-action"
          >
            Annuleren
          </router-link>
        </li>

        <li>
          <utrecht-button
            appearance="primary-action-button"
            type="submit"
            @click="submit"
          >
            Opslaan
          </utrecht-button>
        </li>
      </template>
    </beheer-form>
  </template>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useRouter } from "vue-router";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import CkEditor from "@/components/ckeditor";
import BeheerForm from "@/components/beheer/BeheerForm.vue";
import { fetchLoggedIn, parseJson } from "@/services";
import type { Vac } from "@/features/search/types";
import { toast } from "@/stores/toast";

type VacObject = {
  record: {
    startAt: string;
    data: Vac;
    index?: number;
    correctionFor?: number;
  };
};

const vacObjectenUrl = "/api/vacs/api/v2/objects";

const props = defineProps<{ uuid?: string }>();
const router = useRouter();

const loading = ref(false);
const error = ref(false);

const afdelingen = Array.from({ length: 5 }, () => ({ afdelingNaam: "" }));
const trefwoorden = Array.from({ length: 10 }, () => ({ trefwoord: "" }));

const vac = ref<VacObject>({
  record: {
    startAt: new Date().toISOString().substring(0, 10),
    data: {
      vraag: "",
      antwoord: "",
      afdelingen,
      toelichting: "",
      trefwoorden,
    },
  },
});

// Update afdelingen and trefwoorden with fetched data while remaining array length
watch(
  () => ({
    afdelingData: vac.value.record.data.afdelingen,
    trefwoordData: vac.value.record.data.trefwoorden,
  }),
  ({ afdelingData, trefwoordData }) => {
    vac.value.record.data.afdelingen = [
      ...(afdelingData || []),
      ...afdelingen,
    ].slice(0, afdelingen.length);

    vac.value.record.data.trefwoorden = [
      ...(trefwoordData || []),
      ...trefwoorden,
    ].slice(0, trefwoorden.length);
  },
  { once: true },
);

const showError = () => {
  toast({
    text: "Er is een fout opgetreden bij het opslaan van de Vac. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "De Vac is opgeslagen.",
  });

  router.push("/Beheer/vacs");
};

const submit = async () => {
  loading.value = true;

  const createPayload = () => {
    const body = {
      ...vac.value,
      ...{
        record: {
          ...vac.value.record,
          data: {
            ...vac.value.record.data,
            afdelingen: vac.value.record.data.afdelingen?.filter(
              (afdeling) => afdeling.afdelingNaam.trim().length,
            ),
            trefwoorden: vac.value.record.data.trefwoorden?.filter(
              (trefwoord) => trefwoord.trefwoord.trim().length,
            ),
            status: "actief",
            doelgroep: "eu-burger",
          },
          correctionFor: props.uuid ? vac.value.record.index : undefined,
        },
      },
    };

    return JSON.stringify(body);
  };

  const result = await fetchLoggedIn(
    `${vacObjectenUrl}/${props.uuid ? props.uuid : ""}`,
    {
      method: props.uuid ? "PUT" : "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: createPayload(),
    },
  ).finally(() => (loading.value = false));

  result.ok ? handleSuccess() : showError();
};

onMounted(() => {
  if (!props.uuid) return;

  loading.value = true;

  fetchLoggedIn(`${vacObjectenUrl}/${props.uuid}`)
    .then(parseJson)
    .then((result: VacObject) => (vac.value = result))
    .catch(() => (error.value = true))
    .finally(() => (loading.value = false));
});
</script>
