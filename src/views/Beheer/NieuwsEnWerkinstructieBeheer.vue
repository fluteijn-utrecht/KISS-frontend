<template>
  <utrecht-heading :level="1">Nieuws of werkinstructie</utrecht-heading>

  <router-link to="/Beheer/NieuwsEnWerkinstructies/"
    >Terug naar het overzicht</router-link
  >

  <template v-if="loading"> ..loading </template>
  <template v-else-if="success">
    <div>Het bericht is opgeslagen.</div>
  </template>
  <template v-else-if="bericht">
    <form class="container" @submit.prevent="submit">
      <label class="utrecht-form-label" for="titel"
        ><span>Titel</span>
        <input type="text" id="titel" v-model="bericht.titel" />
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
        <span>PublicatieDatum</span>

        <input
          type="datetime-local"
          id="publicatieDatum"
          v-model="bericht.publicatieDatum"
        />
      </label>

      <ul>
        <li v-for="skill in skills" :key="skill.id">
          <label class="utrecht-form-label" :for="skill.id.toString()">
            <input
              :id="skill.id.toString()"
              type="checkbox"
              :value="skill.id"
              v-model="bericht.skills"
            />
            {{ skill.naam }}</label
          >
        </li>
      </ul>

      <!-- <input type="submit" value="ok" @click="submit" /> -->

      <menu>
        <!-- <li>
          <utrecht-button
            @click="cancelDialog.reveal"
            appearance="secondary-action-button"
            type="button"
          >
            Annuleren
          </utrecht-button>
        </li> -->

        <li>
          <utrecht-button appearance="primary-action-button" type="submit">
            Opslaan
          </utrecht-button>
        </li>
      </menu>
    </form>
    <template if="error"> fout:... </template>
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

const props = defineProps(["id"]);

const editor = ref(ClassicEditor);
//const editorData = ref("<p>Content of the editor.</p>");
const editorConfig = ref({
  toolbar: ["bold", "italic", "|", "NumberedList", "BulletedList", "|", "link"],
  link: { addTargetToExternalLinks: true, defaultProtocol: "https://" },
});

type berichtType = {
  id?: number;
  titel?: string;
  inhoud?: string;
  publicatieDatum?: string;
  isBelangrijk?: boolean;
  skills: Array<any>;
};

type skill = {
  id: number;
  naam: string;
};

const loading = ref<boolean>(true);
const success = ref<boolean>(false);
const error = ref<boolean>(false);

const bericht = ref<berichtType | null>(null);
const skills = ref<Array<skill>>([]);

const submit = async () => {
  loading.value = true;
  error.value = false;
  success.value = false;
  try {
    if (bericht.value?.publicatieDatum) {
      bericht.value.publicatieDatum = new Date(
        bericht.value?.publicatieDatum
      ).toISOString();
    }
    if (props.id) {
      await fetch("/api/berichten/" + props.id, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(bericht.value),
      });
    } else {
      await fetch("/api/berichten/", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(bericht.value),
      });
    }
    success.value = true;
  } catch {
    error.value = true;
  } finally {
    loading.value = false;
  }
};

async function load() {
  loading.value = true;
  error.value = false;
  try {
    if (props.id) {
      //load bericht if id provided
      const response = await fetch("/api/berichten/" + props.id);
      const jsonData = await response.json();

      if (jsonData.publicatieDatum) {
        jsonData.publicatieDatum = toHtmlInputDateTime(
          jsonData.publicatieDatum
        );
      }
      bericht.value = jsonData;
      if (bericht.value) {
        bericht.value.skills = bericht.value.skills.map((s) => s.id);
      }
    } else {
      bericht.value = { skills: [] };
    }

    //load skils
    const skillsResponse = await fetch("/api/Skills");
    const skillsJonData = await skillsResponse.json();
    skills.value = skillsJonData;
  } catch {
    error.value = true;
  }
  loading.value = false;
}

function toHtmlInputDateTime(datumString: string) {
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
