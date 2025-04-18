import { useMemo } from "react";

export const useChartShowEmpty = (data: any[], isFetched?: boolean) => {
    return useMemo(() => {
        if (isFetched) {
            let total = 0;
            total = data.reduce((prev, cur) => {
                return prev + cur.value;
            }, 0);
            return total === 0;
        }
        return false;
    }, [data, isFetched]);
}