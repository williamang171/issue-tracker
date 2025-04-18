'use client'

import { useMemo } from 'react';
import { useOne } from '@refinedev/core';
import { RESOURCE } from '@app/constants/resource';
import { getConstantObjectByKey } from '@app/utils/utils-constants';
import { ISSUE_STATUS } from '@app/constants/issue-status';
import { ISSUE_PRIORITY } from '@app/constants/issue-priority';
import { ISSUE_TYPE } from '@app/constants/issue-type';
import { useParams, useSearchParams } from 'next/navigation';

export const useGetProjectChartsData = () => {
    const { id } = useParams();
    const projectId = (Array.isArray(id) ? id[0] : id);

    const { data: issueStatusCountData, isFetched: issueStatusCountDataIsFetched } = useOne<
        {
            key: number;
            value: number;
        }[]
    >({
        resource: RESOURCE.dashboardIssuesStatusCount,
        id: projectId,
    });

    const { data: issuePriorityCountData, isFetched: issuePriorityCountDataIsFetched } = useOne<
        {
            key: number;
            value: number;
        }[]
    >({
        resource: RESOURCE.dashboardIssuesPriorityCount,
        id: projectId,
    });

    const { data: issueTypeCountData, isFetched: issueTypeCountDataIsFetched } = useOne<
        {
            key: number;
            value: number;
        }[]
    >({
        resource: RESOURCE.dashboardIssuesTypeCount,
        id: projectId,
    });

    const issueStatusCountDataForChart = useMemo(() => {
        if (!issueStatusCountData) {
            return [];
        }
        return issueStatusCountData.data.map((d) => {
            const obj = getConstantObjectByKey(ISSUE_STATUS, d.key);
            const label = obj?.label;
            const color = obj?.color;

            return {
                value: d.value,
                name: label,
                id: d.key,
                color,
            };
        });
    }, [issueStatusCountData]);

    const issuePriorityCountDataForChart = useMemo(() => {
        if (!issuePriorityCountData) {
            return [];
        }
        return issuePriorityCountData.data.map((d) => {
            const obj = getConstantObjectByKey(ISSUE_PRIORITY, d.key);
            const label = obj?.label;
            const color = obj?.color;

            return {
                value: d.value,
                name: label,
                id: d.key,
                color,
            };
        });
    }, [issuePriorityCountData]);

    const issueTypeCountDataForChart = useMemo(() => {
        if (!issueTypeCountData) {
            return [];
        }
        return issueTypeCountData.data.map((d) => {
            const obj = getConstantObjectByKey(ISSUE_TYPE, d.key);
            const label = obj?.label || d.key;
            const color = obj?.color;
            getConstantObjectByKey(ISSUE_TYPE, d.key)?.label || d.key;
            return {
                value: d.value,
                name: label,
                id: d.key,
                color: color,
            };
        });
    }, [issueTypeCountData]);

    return {
        issueStatusCountData: issueStatusCountDataForChart,
        issueStatusCountDataIsFetched: issueStatusCountDataIsFetched,
        issuePriorityCountData: issuePriorityCountDataForChart,
        issuePriorityCountDataIsFetched: issuePriorityCountDataIsFetched,
        issueTypeCountData: issueTypeCountDataForChart,
        issueTypeCountDataIsFetched: issueTypeCountDataIsFetched,
    };
};
