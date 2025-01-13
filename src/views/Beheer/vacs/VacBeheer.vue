<template>
  <utrecht-heading :level="1">Vac</utrecht-heading>

  <simple-spinner v-if="loading" />

  <div v-else-if="error">
    Er is een fout opgetreden bij het ophalen van de Vac.
  </div>

  <template v-else>
    <beheer-form @submit="submit">
      <template #formFields>
        <label class="utrecht-form-label" for="vraag">Vraag</label>
        <ck-editor v-model="vac.record.data.vraag" required />

        <label class="utrecht-form-label" for="antwoord">Antwoord</label>
        <ck-editor v-model="vac.record.data.antwoord" required />
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
import { onMounted, ref } from "vue";
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
    data: Vac;
    startAt: string;
    index?: number;
    correctionFor?: number;
  };
};

const vacObjectenUrl = "/api/vacs/api/v2/objects";

const props = defineProps<{ uuid?: string }>();

const router = useRouter();

const vac = ref<VacObject>({
  record: {
    data: {
      vraag: "",
      antwoord: "",
    },
    startAt: new Date().toISOString().substring(0, 10), // ...
  },
});

const loading = ref(false);
const error = ref(false);

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

  if (props.uuid) {
    // Check correctionFor...
    vac.value = {
      ...vac.value,
      ...{
        record: {
          ...vac.value.record,
          correctionFor: vac.value.record.index,
        },
      },
    };
  }

  const result = await fetchLoggedIn(
    `${vacObjectenUrl}/${props.uuid ? props.uuid : ""}`,
    {
      method: props.uuid ? "PUT" : "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(vac.value),
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
