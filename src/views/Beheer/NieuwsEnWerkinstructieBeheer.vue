<template>
  <utrecht-heading :level="1">Nieuws of werkinstructie</utrecht-heading>

  <template v-if="loading"> <simple-spinner /></template>
  <template v-else-if="success">
    <div class="container">
      <p>Het bericht is opgeslagen.</p>
      <router-link to="/Beheer/NieuwsEnWerkinstructies/">
        Terug naar het overzicht
      </router-link>
    </div>
  </template>
  <template v-else-if="bericht">
    <form class="container" @submit.prevent="submit">
      <fieldset>
        <legend>Type</legend>
        <label class="utrecht-form-label" v-for="t in berichtTypes" :key="t">
          <input
            required
            type="radio"
            name="berichtType"
            :value="t"
            v-model="bericht.type"
          />
          {{ t }}
        </label>
      </fieldset>

      <label class="utrecht-form-label" for="titel"
        ><span>Titel</span>
        <input required type="text" id="titel" v-model="bericht.titel" />
      </label>
      <label class="utrecht-form-label" for="inhoud">Inhoud </label>

      <!-- <div class="editorWithPreview">
        <div> -->
      <ckeditor
        :editor="editor"
        v-model="bericht.inhoud"
        :config="editorConfig"
      ></ckeditor>
      <!-- </div>
        <div class="preview" v-html="bericht.inhoud"></div>
      </div> -->
      <label class="utrecht-form-label" for="isBelangrijk">
        <input
          type="checkbox"
          id="isBelangrijk"
          name="isBelangrijk"
          v-model="bericht.isBelangrijk"
        />Belangrijk</label
      >

      <label class="utrecht-form-label" for="publicatieDatum">
        <span>Publicatiedatum</span>

        <input
          type="datetime-local"
          id="publicatieDatum"
          v-model="bericht.publicatieDatum"
        />
      </label>

      <label class="utrecht-form-label">
        <span>Publicatie-einddatum</span>

        <input type="datetime-local" v-model="bericht.publicatieEinddatum" />
      </label>

      <fieldset>
        <legend>Skills</legend>
        <label
          v-for="skill in skills"
          :key="skill.id"
          class="utrecht-form-label"
          :for="skill.id.toString()"
        >
          <input
            :id="skill.id.toString()"
            type="checkbox"
            :value="skill.id"
            v-model="bericht.skills"
          />
          {{ skill.naam }}</label
        >
      </fieldset>

      <ul>
        <li></li>
      </ul>

      <!-- <input type="submit" value="ok" @click="submit" /> -->

      <menu>
        <li>
          <router-link to="/Beheer/NieuwsEnWerkinstructies/">
            <utrecht-button appearance="secondary-action-button" type="button">
              Annuleren
            </utrecht-button>
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
//https://ckeditor.com/docs/ckeditor5/latest/installation/frameworks/vuejs-v3.html#quick-start
import ClassicEditor from "@ckeditor/ckeditor5-build-classic";
import { berichtTypes } from "@/features/werkbericht/types";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { toast } from "@/stores/toast";

const props = defineProps(["id"]);

const editor = ref(ClassicEditor);
//const editorData = ref("<p>Content of the editor.</p>");
const editorConfig = ref({
  toolbar: ["bold", "italic", "|", "NumberedList", "BulletedList", "|", "link"],
  link: { addTargetToExternalLinks: true, defaultProtocol: "https://" },
});

type berichtType = {
  id?: number;
  type?: string;
  titel?: string;
  inhoud?: string;
  publicatieDatum?: string;
  publicatieEinddatum?: string;
  isBelangrijk?: boolean;
  skills: Array<any>;
};

type skill = {
  id: number;
  naam: string;
};

const loading = ref<boolean>(true);
const success = ref<boolean>(false);

const bericht = ref<berichtType | null>(null);
const skills = ref<Array<skill>>([]);

const addTimezone = (s?: string) => s && new Date(s).toISOString();

const showError = () => {
  toast({
    text: "Er is een fout opgetreden. Probeer het later opnieuw.",
    type: "error",
  });
};

const submit = async () => {
  if (!bericht.value) return;
  loading.value = true;

  success.value = false;
  try {
    bericht.value.publicatieDatum = addTimezone(bericht.value.publicatieDatum);
    bericht.value.publicatieEinddatum = addTimezone(
      bericht.value.publicatieEinddatum
    );
    if (props.id) {
      const result = await fetch("/api/berichten/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(bericht.value),
      });

      if (result.status > 300) {
        showError();
      } else {
        success.value = true;
      }
    } else {
      const result = await fetch("/api/berichten/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(bericht.value),
      });
      if (result.status > 300) {
        showError();
      } else {
        success.value = true;
      }
    }
  } catch {
    showError();
  } finally {
    loading.value = false;
  }
};

async function load() {
  loading.value = true;

  try {
    if (props.id) {
      //load bericht if id provided
      const response = await fetch("/api/berichten/" + props.id);

      if (response.status > 300) {
        showError();
        return;
      }

      const jsonData = await response.json();

      jsonData.publicatieDatum = toHtmlInputDateTime(jsonData.publicatieDatum);
      jsonData.publicatieEinddatum = toHtmlInputDateTime(
        jsonData.publicatieEinddatum
      );
      bericht.value = jsonData;
      if (bericht.value) {
        bericht.value.skills = bericht.value.skills.map((s) => s.id);
      }
    } else {
      bericht.value = { skills: [] };
    }

    //load skils
    const skillsResponse = await fetch("/api/Skills");

    if (skillsResponse.status > 300) {
      showError();
      return;
    }

    const skillsJonData = await skillsResponse.json();
    skills.value = skillsJonData;
  } catch {
    toast({
      text: "Er is een fout opgetreden. Probeer het later opnieuw.",
      type: "error",
    });
  }
  loading.value = false;
}

function toHtmlInputDateTime(datumString?: string) {
  if (!datumString) return datumString;
  const datum = new Date(datumString);

  return `${datum.getFullYear()}-${("0" + (datum.getMonth() + 1)).slice(-2)}-${(
    "0" + datum.getDate()
  ).slice(-2)}T${("0" + datum.getHours()).slice(-2)}:${(
    "0" + datum.getMinutes()
  ).slice(-2)}`;
}

onMounted(() => {
  load();
});
</script>

<style lang="scss" scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}
// .preview {
//   background-color: var(--color-secondary);
//   padding: var(--spacing-default);
// }
// .editorWithPreview {
//   width: 100%;
//   display: flex;
//   gap: var(--spacing-default);
//   * {
//     flex: 1 1 0;
//   }
// }

:deep(.ck-editor ol),
:deep(.ck-editor ul) {
  padding-left: var(--spacing-default);
}

menu {
  margin-top: 2rem;
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

form {
  margin-top: var(--spacing-default);
}
label > span {
  display: block;
}
</style>
