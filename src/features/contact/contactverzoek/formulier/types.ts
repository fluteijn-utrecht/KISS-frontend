export interface Vraag {
  description: string;
  questiontype: string;
}

export interface InputVraag extends Vraag {
  input: string;
}

export interface TextareaVraag extends Vraag {
  textarea?: string;
}

export interface DropdownVraag extends Vraag {
  options: string[];
  selectedDropdown: string;
}

export interface CheckboxVraag extends Vraag {
  options: string[];
  selectedCheckbox: string[];
}

export interface ContactVerzoekVragenSet {
  id: number;
  titel: string;
  vraagAntwoord: Vraag[];
  afdelingId: string;
}
