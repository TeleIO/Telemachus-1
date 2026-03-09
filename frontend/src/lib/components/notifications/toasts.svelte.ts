interface ToastItem {
	id: number;
	message: string;
	type: 'info' | 'warning' | 'error' | 'success';
}

let nextId = 0;

class ToastStore {
	items: ToastItem[] = $state([]);
}

export const toasts = new ToastStore();

export function addToast(message: string, type: ToastItem['type'] = 'info'): void {
	toasts.items = [...toasts.items, { id: nextId++, message, type }];
}

export function removeToast(id: number): void {
	toasts.items = toasts.items.filter((t) => t.id !== id);
}
