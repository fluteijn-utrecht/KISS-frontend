<template>
  <utrecht-heading :level="1">Link</utrecht-heading>

  <template v-if="loading"><SimpleSpinner /></template>

  <template v-else>
    <form class="container" @submit.prevent="submit">
      <label for="titel" class="utrecht-form-label"
        ><span>Titel</span>
        <input type="text" id="titel" v-model="link.titel" required
      /></label>

      <label for="naam" class="utrecht-form-label"
        ><span>Url</span>
        <input type="text" id="naam" v-model="link.url" required
      /></label>

      <label for="naam" class="utrecht-form-label"
        ><span>Caterorie</span>
        <input type="text" id="naam" v-model="link.categorie" required
      /></label>
    </form>

    <menu>
      <li>
        <router-link to="/Beheer/links/">
          <utrecht-button appearance="secondary-action-button" type="button">
            Annuleren
          </utrecht-button>
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
    </menu>
  </template>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";
import { fetchLoggedIn } from "@/services";
import { useRouter } from "vue-router";
const props = defineProps(["id"]);

type linkType = {
  id?: number;
  titel: string;
  url: string;
  categorie: string;
};
const router = useRouter();

const loading = ref<boolean>(false);

const link = ref<linkType>({ titel: "", url: "", categorie: "" });

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "link opgeslagen",
  });
  return router.push("/Beheer/links/");
};

const submit = async () => {
  loading.value = true;

  try {
    if (props.id) {
      const result = await fetchLoggedIn("/api/links/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(link.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    } else {
      const result = await fetchLoggedIn("/api/links/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(link.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    }
    return handleSuccess();
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  if (props.id) {
    loading.value = true;
    try {
      const response = await fetchLoggedIn("/api/links/" + props.id);
      if (response.status > 300) {
        showError();
        return;
      }
      const jsonData = await response.json();
      link.value = jsonData;
    } catch {
      showError();
    } finally {
      loading.value = false;
    }
  }
});
</script>

<style>
menu {
  margin-top: 2rem;
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

form {
  margin-top: var(--spacing-default);
}
label > span {
  display: block;
}
</style>
