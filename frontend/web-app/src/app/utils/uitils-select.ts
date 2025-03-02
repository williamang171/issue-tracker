export const mapToSelectItemObject = (item: { id: number, label: string }) => {
    return {
        value: item.id,
        label: item.label
    }
}