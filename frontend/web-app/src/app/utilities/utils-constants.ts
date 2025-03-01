export function createConstantObject(key: number, label: string, color?: string) {
    return {
        key,
        id: key,
        label,
        color,
    };
}

export type ConstantObject = {
    [key: string]: {
        label: string;
        id: string;
        color?: string;
    };
};

function isKeyOfObject<T extends ConstantObject>(
    obj: T,
    key: string
): key is Extract<keyof T, string> {
    return Object.prototype.hasOwnProperty.call(obj, key);
}

export function getConstantObjectByKey<T extends ConstantObject>(obj: T, key: string) {
    if (!key) {
        return;
    }
    const keyNormalized = key.toLowerCase();
    if (isKeyOfObject(obj, keyNormalized)) {
        return obj[keyNormalized];
    }
    return;
}