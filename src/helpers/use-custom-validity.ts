import { ref, watchEffect } from "vue";

export const useCustomValidity = (inputTypeValidatorMap: {
  [key: string]: (input: HTMLInputElement) => void;
}) => {
  const formRef = ref<HTMLElement>();

  const queryInputs = (type: string) =>
    (formRef.value?.querySelectorAll(`[type='${type}']`) ||
      []) as NodeListOf<HTMLInputElement>;

  const setCustomValidity = () =>
    Object.entries(inputTypeValidatorMap).forEach(([type, validator]) =>
      queryInputs(type).forEach((input) => validator(input)),
    );

  watchEffect(() => setCustomValidity());

  return {
    formRef,
    setCustomValidity,
  };
};
