<template>
  <prompt-modal
    :dialog="cancelDialog"
    message="Weet je zeker dat je het proces wil annuleren? Niet opgeslagen gegevens zullen verloren gaan."
    cancel-message="Nee"
    confirm-message="Ja"
  />

  <utrecht-heading :level="1">{{
    props.id ? "Formulier bewerken" : "Formulier aanmaken"
  }}</utrecht-heading>
  <template v-if="loading"> <simple-spinner /></template>
  <div>
    <pre>
      <div style="white-space: pre-line">
      Hier kan je een template maken voor een contactverzoek. Houd er rekening mee dat
      dit template een aanvulling is op de standaard vragen.
      Deze hoef je hier dus niet toe te voegen. De standaardvragen zijn: 
      - Klantnaam
      - Telefoonnummer 1
      - Telefoonnummer 2 
      - Omschrijving telefoonnummer 2 
      - E-mailadres 
      - Interne toelichting voor medewerker
      </div>
    </pre>
  </div>
  <form class="container" @submit.prevent="submit">
    <label class="utrecht-form-label" for="titel"
      ><span>Titel *</span>
      <input
        class="utrecht-textbox utrecht-textbox--html-input"
        required
        type="text"
        id="titel"
        v-model="title"
      />
    </label>
    <!-- dropdown for afdelingen -->
    <label class="utrecht-form-label">
      <span>Afdeling *</span>
      <select
        class="utrecht-select utrecht-select--html-select"
        v-model="selectedAfdeling"
        required
      >
        <option disabled value="">Kies een afdeling</option>
        <option
          v-for="afdeling in afdelingen"
          :key="afdeling.id"
          :value="afdeling.naam"
        >
          {{ afdeling.naam }}
        </option>
      </select>
    </label>

    <!-- Loop through vragen and render label and input field -->
    <div v-for="(vraag, index) in vragen" :key="index">
      <div class="question-container">
        <div class="question-container-label utrecht-form-label">
          <span
            >Vraag {{ index + 1 }} - {{ setVraagTypeLabel(vraag.type) }} *</span
          >
        </div>
        <div class="question-container-button">
          <!-- delete button -->
          <utrecht-button
            appearance="secondary-action-button"
            class="icon icon-after trash icon-only"
            type="button"
            @click="removeVraag(vraag.id)"
          />
        </div>
      </div>

      <!-- Conditional part for Input -->
      <div
        v-if="
          isInputVraag(vraag) ||
          isTextareaVraag(vraag) ||
          isCheckboxVraag(vraag) ||
          isDropdownVraag(vraag)
        "
      >
        <input
          class="utrecht-textbox utrecht-textbox--html-input"
          required
          type="text"
          v-model="vraag.label"
          placeholder="Vul hier een label voor de vraag in"
        />
      </div>

      <!-- Conditional part for Dropdown -->
      <div v-if="isDropdownVraag(vraag)">
        <label class="utrecht-form-label">
          <span>Answer Options:</span>
        </label>
        <div
          v-for="(option, optionIndex) in vraag.options"
          :key="optionIndex"
          class="option-container"
        >
          <input
            class="utrecht-textbox utrecht-textbox--html-input"
            required
            type="text"
            v-model="vraag.options[optionIndex]"
            :placeholder="`Voeg hier optie ${optionIndex + 1} toe`"
          />
          <utrecht-button
            appearance="secondary-action-button"
            class="icon icon-after trash icon-only"
            :title="`Verwijder optie ${optionIndex + 1}`"
            type="button"
            @click="removeOption(vraag.id, optionIndex)"
          />
        </div>
        <utrecht-button
          appearance="secondary-action-button"
          class="icon icon-after plus"
          title="Antwoordoptie Toevoegen"
          type="button"
          @click="addOption(vraag.id)"
        >
          Antwoordoptie Toevoegen
        </utrecht-button>
      </div>
      <!-- Conditional part for checkbox -->
      <div v-if="isCheckboxVraag(vraag)">
        <label class="utrecht-form-label">
          <span>Answer Options:</span>
        </label>
        <div
          v-for="(option, optionIndex) in vraag.options"
          :key="optionIndex"
          class="option-container"
        >
          <input
            class="utrecht-textbox utrecht-textbox--html-input"
            required
            type="text"
            v-model="vraag.options[optionIndex]"
            :placeholder="`Voeg hier optie ${optionIndex + 1} toe`"
          />
          <utrecht-button
            appearance="secondary-action-button"
            class="icon icon-after trash icon-only"
            :title="`Verwijder optie ${optionIndex + 1}`"
            type="button"
            @click="removeOption(vraag.id, optionIndex)"
          />
        </div>
        <utrecht-button
          appearance="secondary-action-button"
          class="icon icon-after plus"
          title="Label Toevoegen"
          type="button"
          @click="addOption(vraag.id)"
        >
          Antwoordoptie Toevoegen
        </utrecht-button>
      </div>
    </div>
    <!-- Dropdown for vraag -->
    <label class="utrecht-form-label">
      <span>Vraag toevoegen</span>
      <select
        class="utrecht-select utrecht-select--html-select"
        v-model="selectedVraag"
        @change="handleVraagChange()"
      >
        <option disabled value="Vraag toevoegen">Kies een vraag</option>
        <option value="input">Open vraag kort</option>
        <option value="textarea">Open vraag lang</option>
        <option value="dropdown">Dropdown</option>
        <option value="checkbox">Checkbox</option>
      </select>
    </label>

    <menu>
      <li>
        <utrecht-button
          modelValue
          type="button"
          appearance="secondary-action-button"
          @click="revealCancelDialog"
        >
          Annuleren
        </utrecht-button>
      </li>

      <li>
        <utrecht-button appearance="primary-action-button" type="submit">
          Opslaan
        </utrecht-button>
      </li>
    </menu>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import {
  Heading as UtrechtHeading,
  Button as UtrechtButton,
} from "@utrecht/component-library-vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { fetchLoggedIn } from "@/services";
import { toast } from "@/stores/toast";
import { useRouter } from "vue-router";

const router = useRouter();
const props = defineProps<{ id?: string }>();

const selectedAfdeling = ref("");
const title = ref("");

const afdelingen = ref([
  { id: 1, naam: "Afdeling 1" },
  { id: 2, naam: "Afdeling 2" },
  { id: 3, naam: "Afdeling 3" },
  { id: 4, naam: "Afdeling 4" },
  { id: 5, naam: "Afdeling 5" },
]);

type Vraag = {
  id: number;
  type: string;
  label: string;
};

type InputVraag = Vraag;

type TextareaVraag = Vraag;

type DropdownVraag = Vraag & {
  options: string[];
};

type CheckboxVraag = Vraag & {
  options: string[];
};

const vragen = ref<Vraag[]>([]);
const selectedVraag = ref("Vraag toevoegen");

const submit = async () => {
  loading.value = true;

  try {
    const payload = {
      Naam: title.value,
      AfdelingId: selectedAfdeling.value,
      JsonVragen: JSON.stringify(vragen.value),
    };

    let result;
    const id = props.id;

    if (id) {
      result = await fetchLoggedIn(`/api/contactverzoekvragensets/${id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });
    } else {
      result = await fetchLoggedIn("/api/contactverzoekvragensets", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });
    }

    if (!result.ok) {
      handleError();
    } else {
      handleSuccess();
    }
  } catch (error) {
    handleError();
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  load();
});

let vraagCounter = 0;
const handleVraagChange = () => {
  vraagCounter++;

  const nieuweVraag: Vraag = {
    id: vraagCounter,
    type: selectedVraag.value,
    label: "",
    ...(selectedVraag.value === "dropdown" || selectedVraag.value === "checkbox"
      ? { options: ["", ""] }
      : {}),
  };

  vragen.value.push(nieuweVraag);
  selectedVraag.value = "Vraag toevoegen";
};

const removeVraag = (id: number) => {
  const index = vragen.value.findIndex((vraag) => vraag.id === id);

  if (index !== -1) {
    vragen.value.splice(index, 1);
    vragen.value.forEach((vraag, i) => {
      vraag.id = i + 1;
    });
  }
  vraagCounter = vragen.value.length;
};

const isInputVraag = (question: Vraag): question is InputVraag =>
  question.type === "input";

const isTextareaVraag = (question: Vraag): question is TextareaVraag =>
  question.type === "textarea";

const isCheckboxVraag = (question: Vraag): question is CheckboxVraag =>
  question.type === "checkbox" && "options" in question;

const isDropdownVraag = (question: Vraag): question is DropdownVraag =>
  question.type === "dropdown" && "options" in question;

const removeOption = (vraagId: number, optionIndex: number) => {
  const vraag = vragen.value.find((v) => v.id === vraagId);

  if (vraag && (isCheckboxVraag(vraag) || isDropdownVraag(vraag))) {
    const newOptions = [...vraag.options];
    newOptions.splice(optionIndex, 1);
    vraag.options = newOptions;
  }
};

const addOption = (vraagId: number) => {
  const vraag = vragen.value.find((v) => v.id === vraagId);

  if (vraag && (isCheckboxVraag(vraag) || isDropdownVraag(vraag))) {
    vraag.options.push("");
  }
};

const loading = ref<boolean>(true);

async function load() {
  loading.value = true;

  try {
    const id = props.id;
    if (id) {
      const response = await fetchLoggedIn(
        `/api/contactverzoekvragenset/${id}`,
      );
      const data = await response.json();

      title.value = data.naam;
      selectedAfdeling.value = data.afdelingId;
      vragen.value = JSON.parse(data.jsonVragen);
      vraagCounter = vragen.value.length;
    }
  } catch {
    handleError();
  } finally {
    loading.value = false;
  }
}

const handleError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const handleSuccess = () => {
  toast({
    text: "Contactverzoek formulier opgeslagen",
  });
  router.push("/Beheer/Contactverzoekformulieren/");
};

const setVraagTypeLabel = (type: string) => {
  switch (type) {
    case "input":
      return "Open vraag kort";
    case "textarea":
      return "Open vraag lang";
    case "dropdown":
      return "Dropdown";
    case "checkbox":
      return "Checkbox";
    default:
      return "";
  }
};

const navigateToContactverzoekformulieren = () => {
  router.push("/Beheer/Contactverzoekformulieren/");
};

const revealCancelDialog = () => {
  if (vragen.value.length > 0) {
    cancelDialog.reveal();
  } else {
    navigateToContactverzoekformulieren();
  }
};

const cancelDialog = useConfirmDialog();
cancelDialog.onConfirm(() => {
  navigateToContactverzoekformulieren();
});
</script>

<style lang="scss" scoped>
form {
  margin-top: var(--spacing-default);
}

label > span {
  display: block;
}

.question-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.option-container {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

menu {
  margin-top: var(--spacing-large);
  display: flex;
  gap: var(--spacing-default);
  justify-content: flex-end;
}
</style>
