export const renderEnumFieldLabel =
    <T extends Record<string, unknown>, V = string>
        (value: V, record: T, fieldKey: string, enumMap: { [x: number]: { label: string } }) => {
        if (fieldKey in record
            && typeof record[fieldKey] === 'number'
            && record[fieldKey] in enumMap) {
            return enumMap[record[fieldKey]].label
        }
        return value;
    }