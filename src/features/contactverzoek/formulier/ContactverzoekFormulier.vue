<template>
  <div class="container" @submit.prevent>    <form-fieldset class="radio-group">
      <form-fieldset-legend class="required"
        >Contactverzoek maken voor</form-fieldset-legend
      >
      <label>
        <input
          type="radio"
          value="afdeling"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.selectedOption"
        />
        Afdeling
      </label>
      <label>
        <input
          type="radio"
          value="groep"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.selectedOption"
        />
        Groep
      </label>
      <label>
        <input
          type="radio"
          value="medewerker"
          class="utrecht-radio-button utrecht-radio-button--html-input"
          v-model="form.selectedOption"
        />
        Medewerker
      </label>
    </form-fieldset>

     <!-- Afdeling -->
    <template v-if="form.selectedOption === 'afdeling'">
      <label class="utrecht-form-label">
        <span class="required">Afdeling</span>
        <service-data-search
          class="utrecht-textbox utrecht-textbox--html-input"
          :required="true"
          v-model="form.afdeling"
          placeholder="Zoek een afdeling"
          @update:model-value="onUpdateAfdeling"
          :get-data="useAfdelingen"
          :map-value="(x) => x?.naam"
          @keydown.enter="setEnterPressed"
          :map-description="(x) => x?.identificatie"
        />
      </label>

      <label :class="['utrecht-form-label', { disabled: !form.afdeling?.id }]">
        <span class="">Medewerker binnen afdeling</span>
        <medewerker-search
          class="utrecht-textbox utrecht-textbox--html-input"
          v-model="form.afdelingMedewerker"
          :afdeling-id="form.afdeling?.naam"
          @update:model-value="setActive"
          :required="!form.afdeling?.id"
          :isDisabled="!form.afdeling?.id" 
          :placeholder="form.afdeling?.id ? 'Zoek een medewerker' : 'Kies eerst een afdeling'"
        />
      </label>

  
    </template>

    <!-- Groep -->
    <template v-if="form.selectedOption === 'groep'">
      <service-data-wrapper :data="groepen">
        <template #init>
        </template>
        <template #success="{ data }">
          <label class="utrecht-form-label">
            <span class="required">Groep</span>
            <service-data-search
              class="utrecht-textbox utrecht-textbox--html-input"
              v-model="form.groep"
              :placeholder="
                !data.count ? 'Geen groepen gevonden' : 'Zoek een groep'
              "
              @update:model-value="setActive"
              :get-data="useGroepen" 
              :map-value="(x) => x?.naam"
              :map-description="(x) => x?.identificatie"
              ref="groepSearchRef"
              :disabled="!data.count"
            />
          </label>
        </template>
      </service-data-wrapper>
      <label :class="['utrecht-form-label', { disabled: !form.groep?.id }]">
        <span class="">Medewerker binnen groep</span>
        <medewerker-search
          class="utrecht-textbox utrecht-textbox--html-input"
          v-model="form.groepMedewerker"
          :groep-id="form.groep?.naam"
          @update:model-value="setActive"
          :required="!form.groep?.id"
          :isDisabled="!form.groep?.id" 
          :placeholder="form.groep?.id ? 'Zoek een medewerker' : 'Kies eerst een groep'"
        />
      </label>
    </template>

     <!-- Medewerker -->
     <template v-if="form.selectedOption === 'medewerker'">
     <label class="utrecht-form-label">
      <span class="required">Medewerker</span>
      <medewerker-search
        class="utrecht-textbox utrecht-textbox--html-input"
        required
        v-model="form.medewerker"
        @update:model-value="setActive"
      />
    </label>

      <div>
        <label for="groep" class="utrecht-form-label">
          <span class="required">Afdeling / groep </span>
          <select 
            id="groep" 
            class="utrecht-textbox utrecht-textbox--html-input" 
            v-model="form.groepAfdeling"
          >
            <option v-for="item in afdelingenGroepen" :value="item" :key="item">
              {{ item }}
            </option>
          </select>
      </label>
      </div>
    </template>

    <label class="utrecht-form-label notitieveld">
      <span class="required">Interne toelichting voor medewerker</span>
      <textarea
        v-model="form.interneToelichting"
        name="Interne toelichting"
        required
        class="utrecht-textarea utrecht-textarea--html-textarea"
        rows="5"
        @input="setActive"
      />
    </label>

    <form-fieldset>
      <service-data-wrapper :data="vragenSets" class="container">
        <template #success="{ data }">
          <!-- Dropdown for selecting Onderwerp -->
          <contactverzoek-onderwerpen
            :vragenSets="data"
            :afdelingId="form?.afdeling?.id"
            :prefill="!form.vragenSetChanged"
            v-model:modelValue="form.contactVerzoekVragenSet"
            @change="form.vragenSetChanged = true"
          />

          <!-- Dynamic fields based on selected Onderwerp -->
          <template v-if="form.contactVerzoekVragenSet">
            <template
              v-for="(item, index) in form.contactVerzoekVragenSet
                .vraagAntwoord"
              :key="index"
            >
              <label class="utrecht-form-label">
                <span>{{ item.description }}</span>
                <input
                  v-if="isInputVraag(item)"
                  class="utrecht-textbox utrecht-textbox--html-input"
                  type="text"
                  v-model="item.input"
                  @input="setActive"
                />
                <textarea
                  v-if="isTextareaVraag(item)"
                  class="utrecht-textarea"
                  v-model="item.textarea"
                  @input="setActive"
                ></textarea>
                <select
                  v-if="isDropdownVraag(item)"
                  class="utrecht-select"
                  v-model="item.selectedDropdown"
                  @input="setActive"
                >
                  <option v-for="option in item.options" :key="option">
                    {{ option }}
                  </option>
                </select>
                <div v-if="isCheckboxVraag(item)">
                  <label
                    class="utrecht-checkbox-button"
                    v-for="(option, optionIndex) in item.options"
                    :key="option"
                  >
                    <input
                      class="utrecht-checkbox-button"
                      type="checkbox"
                      :value="option"
                      v-model="item.selectedCheckbox[optionIndex]"
                    />
                    {{ option }}
                  </label>
                </div>
              </label>
            </template>
          </template>
        </template>
      </service-data-wrapper>
    </form-fieldset>

    <form-fieldset>
      <form-fieldset-legend>Contact opnemen met</form-fieldset-legend>
      <label class="utrecht-form-label">
        <span>Voornaam</span>
        <input
          v-model="form.voornaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Tussenvoegsel</span>
        <input
          v-model="form.voorvoegselAchternaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Achternaam</span>
        <input
          v-model="form.achternaam"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Organisatie</span>
        <input
          v-model="form.organisatie"
          type="tel"
          name="Naam"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Telefoonnummer 1</span>
        <input
          ref="telEl"
          v-model="form.telefoonnummer1"
          type="tel"
          name="Telefoonnummer 1"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Telefoonnummer 2</span>
        <input
          v-model="form.telefoonnummer2"
          type="tel"
          name="Telefoonnummer 2"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>Omschrijving telefoonnummer 2</span>
        <input
          v-model="form.omschrijvingTelefoonnummer2"
          name="Omschrijving telefoonnummer 2"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
      <label class="utrecht-form-label">
        <span>E-mailadres</span>
        <input
          v-model="form.emailadres"
          type="email"
          name="E-mailadres"
          class="utrecht-textbox utrecht-textbox--html-input"
          @input="setActive"
        />
      </label>
    </form-fieldset>
  </div>
</template>

<script lang="ts">
export default {
  inheritAttrs: false,
};
</script>

<script lang="ts" setup>
import MedewerkerSearch from "@/features/search/MedewerkerSearch.vue";
import type { ContactmomentContactVerzoek } from "@/stores/contactmoment";
import { ref } from "vue";
import { watch } from "vue";
import {
  FormFieldsetLegend,
  FormFieldset,
} from "@utrecht/component-library-vue";

import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import ServiceDataSearch from "@/components/ServiceDataSearch.vue";
import { whenever } from "@vueuse/core";
import { nextTick } from "vue";
import {
  useVragenSets,
  isInputVraag,
  isTextareaVraag,
  isDropdownVraag,
  isCheckboxVraag,
  useAfdelingenGroepen
} from "./service";

import { useAfdelingen } from "@/composables/afdelingen";
import { useGroepen } from "@/composables/groepen";
import ContactverzoekOnderwerpen from "./ContactverzoekOnderwerpen.vue";
import { computed } from 'vue'

const props = defineProps<{
  modelValue: ContactmomentContactVerzoek;
}>();

const form = ref<Partial<ContactmomentContactVerzoek>>({});

watch(
  () => props.modelValue,
  (v) => {
    form.value = v;
  },
  { immediate: true },
);

watch(
  () => form.value.afdeling,
  () => {
    setOnderwerp();
  },
);

const setActive = () => {
  form.value.isActive = true;
};

const onUpdateAfdeling = () => {
  form.value.contactVerzoekVragenSet = undefined;
  form.value.vragenSetChanged = false;
  setActive();
};

const telEl = ref<HTMLInputElement>();
const vragenSets = useVragenSets();

const setOnderwerp = () => {
  setActive();
};

const groepen = useGroepen(() => form.value.groep?.naam);


//const afdelingenGroepen = useAfdelingenGroepen(() => form.value.medewerker?.groepen[0].naam)

// const afdelingenGroepen = computed(() => {
//   const searchKey = form.value.medewerker?.groepen[0]?.groepsnaam;
//   if (!searchKey) return [];

//   const data = useAfdelingenGroepen(() => searchKey);
//   return data(); // Now calling the function to get the actual data
// });


const afdelingenGroepen = computed(() => {
  const afdelingenArray = form.value.medewerker?.afdelingen.map(afdeling => afdeling.afdelingnaam) || [];
  const groepenArray = form.value.medewerker?.groepen.map(groep => groep.groepsnaam) || [];

  const data = useAfdelingenGroepen([], []);
  return data;
});

// const afdelingenGroepen = computed(() => {
//     return useAfdelingenGroepen(() =>  form.value.groepAfdeling?.naam);
//   });

const groepSearchRef = ref();

const enterPressed = ref(false);
const setEnterPressed = () => {
  enterPressed.value = true;
};

// focus groep search element whenever it appears on the page (so when you select a Afdeling that has Groepen)
whenever(groepSearchRef, (v) => {
  if (!enterPressed.value) return;
  enterPressed.value = false;
  nextTick(() => {
    (v.$el as HTMLElement)?.getElementsByTagName("input")?.[0]?.focus();
  });
});

watch(
  [
    telEl,
    () =>
      !!form.value.telefoonnummer1 ||
      !!form.value.telefoonnummer2 ||
      !!form.value.emailadres,
  ],
  ([el, hasContact]) => {
    if (!el) return;
    el.setCustomValidity(
      hasContact ? "" : "Vul minimaal een telefoonnummer of een e-mailadres in",
    );
  },
);

// watch(
//   () => form.value.afdeling,
//   () => {
//     form.value.groep = undefined;
//   },
// );

// const afdelingId = computed(() => {
//   const afdelingen = props.modelValue.medewerker?.afdelingen;
//   if (Array.isArray(afdelingen) && afdelingen.length > 0) {
//     return afdelingen[0].afdelingId;
//   }
//   return null;
// });

// watch(
//   () => form.value.medewerker,
//   () => {
//     form.value.afdeling = {
//       id: afdelingId.value ?? "",
//       identificatie: "",
//       naam: "",
//     };
//     setOnderwerp();
//   },
// );

watch(
  () => form.value.afdeling,
  () => {
    form.value.afdelingMedewerker = undefined;
    setActive();
  },
);

watch(
  () => form.value.groep,
  () => {
    form.value.groepMedewerker = undefined;
    setActive();
  },
);

// // Watch the computed property for changes and update form accordingly
// watch(afdelingenGroepen, (newValue) => {
//   if (newValue.length === 1) {
//     const selected = newValue[0]; // Assuming the selection text includes 'Afdeling: ' or 'Groep: '
//     console.log("Auto-selected:", selected); // Example action

//   //   // Initialize or ensure the groepAfdeling object is set up properly
//   //   if (!form.value.groepAfdeling) {
//   //     form.value.groepAfdeling = { id: '', identificatie: '', naam: '' };
//   //   }

//   //   // Assign the name without the prefix to the form
//   //   form.value.groepAfdeling.naam = selected.includes("Afdeling: ") ? selected.replace("Afdeling: ", "") : selected.replace("Groep: ", "");
    
//   //   // Optionally, automatically select the radio button or update any other relevant part of the form
//   //   form.value.selectedOption = selected.includes("Afdeling: ") ? 'afdeling' : 'groep';
//   }
// });

// watch(
//   () => form.value.medewerker,
//   () => {
//     form.value.groepAfdeling?.naam = "burgerzaken" 
//     setActive();
//   },
// );

// watch(
//   () => form.value.medewerker,  // Watch the medewerker object for changes
//   (newVal, oldVal) => {
//     // console.log('Medewerker changed from', oldVal, 'to', newVal);
//     if (newVal && newVal.groepen && newVal.groepen.length > 0) {
      

//       if (form.value.medewerker?.groepen) {
//         console.log(newVal?.groepen)
//         form.value.medewerker.groepen = newVal.groepen;
//        }
//     }
//   },
//   { deep: true },
// );


// const currentMedewerker = computed(() => {
//   return form.value.selectedOption === 'afdeling' ? form.value.medewerker
//          : form.value.selectedOption === 'groep' ? form.value.groepMedewerker
//          : null;
// });

// // Watch for changes in selectedOption to manage medewerker state
// watch(() => form.value.selectedOption, (newVal, oldVal) => {
//   // Restore the appropriate medewerker based on the new selected option
//   form.value.medewerker = currentMedewerker.value;
// }, { immediate: false });

</script>

<style lang="scss" scoped>
.container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

textarea {
  resize: none;
}

fieldset {
  display: grid;
  gap: var(--spacing-extrasmall);
}

.radio-group > legend {
  font-size: inherit;
}

.utrecht-checkbox-button {
  display: flex !important;
}
</style>