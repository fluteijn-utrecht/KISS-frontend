<template>
  <h1>Bericht</h1>

  <template v-if="loading"> ..loading </template>

  <template v-else-if="success"> klaar. ga terug... </template>

  <template v-else-if="bericht">
    <label for="titel">Titel </label>
    <input type="text" id="titel" v-model="bericht.titel" />

    <label for="inhoud">Inhoud </label>
    <input type="text" id="inhoud" v-model="bericht.inhoud" />

    <label for="isBelangrijk"> Belangrijk </label>
    <input
      type="checkbox"
      id="isBelangrijk"
      name="isBelangrijk"
      v-model="bericht.isBelangrijk"
    />

    <label for="publicatieDatum"> PublicatieDatum </label>
    <input
      type="datetime-local"
      id="publicatieDatum"
      v-model="bericht.publicatieDatum"
    />

    <ul>
      <li v-for="skill in skills" :key="skill.id">
        <label :for="skill.id.toString()">{{ skill.naam }}</label>
        <input
          :id="skill.id.toString()"
          type="checkbox"
          :value="skill.id"
          v-model="bericht.skills"
        />
      </li>
    </ul>

    <input type="submit" value="ok" @click="submit" />

    <template if="error"> fout:... </template>
  </template>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

const props = defineProps(["id"]);

type berichtType = {
  id?: number;
  titel?: string;
  inhoud?: string;
  publicatieDatum?: Date;
  isBelangrijk?: boolean;
  skills: Array<number>;
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
      bericht.value = jsonData;
      //  bericht.value.skills= []
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

onMounted(() => {
  load();
});
</script>

<style></style>
