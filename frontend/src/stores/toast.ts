import { reactive } from "vue";

type MessageType = "confirm" | "error";

interface ToastParams {
  text: string;
  type?: MessageType;
  timeout?: number;
}

interface Message {
  readonly text: string;
  readonly type: MessageType;
  readonly key: number;
}

let key = 0;

const _messages = reactive<Message[]>([]);

export const messages = _messages as readonly Message[];

export function toast(params: ToastParams) {
  const m = {
    text: params.text,
    type: params.type || "confirm",
    key: (key += 1),
  };
  _messages.push(m);
  setTimeout(() => {
    const index = _messages.indexOf(m);
    if (index !== -1) {
      _messages.splice(index, 1);
    }
  }, params.timeout || 3000);
}
