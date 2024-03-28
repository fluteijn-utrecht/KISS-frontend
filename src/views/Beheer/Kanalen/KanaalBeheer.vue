<template>
  <utrecht-heading :level="1">Kanaal</utrecht-heading>

  <template v-if="loading">
    <SimpleSpinner />
  </template>

  <template v-else-if="item">
    <form class="container" @submit.prevent="submit">
      <label for="titel" class="utrecht-form-label">
        <span>Naam</span>
        <input
          class="utrecht-textbox utrecht-textbox--html-input"
          type="text"
          id="titel"
          v-model="item.naam"
          required
        />
      </label>
      <menu>
        <li>
          <router-link
            to="/Beheer/kanalen/"
            class="utrecht-button utrecht-button--secondary-action"
          >
            Annuleren
          </router-link>
        </li>

        <li>
          <utrecht-button appearance="primary-action-button" type="submit">
            Opslaan
          </utrecht-button>
        </li>
      </menu>
    </form>
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
import {
  ServiceResult,
  fetchLoggedIn,
  mapServiceData,
  parseJson,
  throwIfNotOk,
} from "@/services";
import { useRouter } from "vue-router";
import SearchCombobox, {
  type DatalistItem,
} from "@/components/SearchCombobox.vue";

const props = defineProps<{ id?: string }>();

type Kanaal = {
  id?: string;
  naam?: string; 
};
const router = useRouter();

const loading = ref<boolean>(false);

const item = ref<Kanaal>();

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "opgeslagen",
  });
  return router.push("/Beheer/kanalen/");
};

const submit = async () => {
  loading.value = true;

  try {
    if (props.id) {
      const result = await fetchLoggedIn("/api/KanaalBewerken/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(item.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        return handleSuccess();
      }
    } else {
      const result = await fetchLoggedIn("/api/KanaalToevoegen/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(item.value),
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
  loading.value = true;
console.log(props)
  try {
    if (props.id) {
      //load link
      const response = await fetchLoggedIn("/api/KanaalBeheerDetails/" + props.id);
      if (response.status > 300) {
        showError();
        return;
      }
      const jsonData = await response.json();

      item.value = jsonData;
    } else {
      item.value = {};
    }
 
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
});
</script>

<style>

/* todo: algemene beheer styles? */

menu {
  margin-top: var(--spacing-large);
  display: flex;
  gap: var(--spacing-default);
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

.p-r {
  position: relative;
}
</style>
