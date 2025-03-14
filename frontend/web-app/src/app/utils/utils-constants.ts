export function createConstantObject(key: number, label: string, color?: string) {
    return {
        key,
        id: key,
        label,
        color,
    };
}

export type ConstantObject = {
    [key: number]: {
        label: string;
        id: number;
        color?: string;
    };
};

function isKeyOfObject<T extends ConstantObject>(
    obj: T,
    key: number
): key is Extract<keyof T, number> {
    return Object.prototype.hasOwnProperty.call(obj, key);
}

export function getConstantObjectByKey<T extends ConstantObject>(obj: T, key: number) {
    if (key === undefined || key === null) {
        return;
    }

    if (isKeyOfObject(obj, key)) {
        return obj[key];
    }
    return;
}