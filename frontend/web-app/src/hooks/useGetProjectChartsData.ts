'use client'

import { useMemo } from 'react';
import { useOne } from '@refinedev/core';
import { RESOURCE } from '@app/constants/resource';
import { getConstantObjectByKey } from '@app/utils/utils-constants';
import { ISSUE_STATUS } from '@app/constants/issue-status';
import { ISSUE_PRIORITY } from '@app/constants/issue-priority';
import { ISSUE_TYPE } from '@app/constants/issue-type';
import { useParams, useSearchParams } from 'next/navigation';
import { mockIssueStatusCountData } from '@mocks/mock-project-chart-data/mock-issue-status-count-data';
import { mockIssuePriorityCountData } from '@mocks/mock-project-chart-data/mock-issue-priority-count-data';
import { mockIssueTypeCountData } from '@mocks/mock-project-chart-data/mock-issue-type-count-data';

export const useGetProjectChartsData = () => {
    const { id } = useParams();
    const projectId = (Array.isArray(id) ? id[0] : id);

    // const { data: issueStatusCountData } = useOne<
    //     {
    //         status: number;
    //         count: number;
    //     }[]
    // >({
    //     resource: RESOURCE.dashboardIssuesStatusCount,
    //     id: projectId,
    // });

    // const { data: issuePriorityCountData } = useOne<
    //     {
    //         priority: number;
    //         count: number;
    //     }[]
    // >({
    //     resource: RESOURCE.dashboardIssuesStatusCount,
    //     id: projectId,
    // });

    // const { data: issueTypeCountData } = useOne<
    //     {
    //         type: number;
    //         count: number;
    //     }[]
    // >({
    //     resource: RESOURCE.dashboardIssuesTypeCount,
    //     id: projectId,
    // });

    // TODO: replace with actual data
    const issueStatusCountData = {
        data: mockIssueStatusCountData
    };
    const issuePriorityCountData = {
        data: mockIssuePriorityCountData
    };
    const issueTypeCountData = {
        data: mockIssueTypeCountData
    };

    const issueStatusCountDataForChart = useMemo(() => {
        if (!issueStatusCountData) {
            return [];
        }
        return issueStatusCountData.data.map((d) => {
            const obj = getConstantObjectByKey(ISSUE_STATUS, d.status);
            const label = obj?.label;
            const color = obj?.color;

            return {
                value: d.count,
                name: label,
                id: d.status,
                color,
            };
        });
    }, [issueStatusCountData]);

    const issuePriorityCountDataForChart = useMemo(() => {
        if (!issuePriorityCountData) {
            return [];
        }
        return issuePriorityCountData.data.map((d) => {
            const obj = getConstantObjectByKey(ISSUE_PRIORITY, d.priority);
            const label = obj?.label;
            const color = obj?.color;

            return {
                value: d.count,
                name: label,
                id: d.priority,
                color,
            };
        });
    }, [issuePriorityCountData]);

    const issueTypeCountDataForChart = useMemo(() => {
        if (!issueTypeCountData) {
            return [];
        }
        return issueTypeCountData.data.map((d) => {
            const obj = getConstantObjectByKey(ISSUE_TYPE, d.type);
            const label = obj?.label || d.type;
            const color = obj?.color;
            getConstantObjectByKey(ISSUE_TYPE, d.type)?.label || d.type;
            return {
                value: d.count,
                name: label,
                id: d.type,
                color: color,
            };
        });
    }, [issueTypeCountData]);

    // TODO: return actual data
    return {
        issueStatusCountData: issueStatusCountDataForChart,
        issuePriorityCountData: issuePriorityCountDataForChart,
        issueTypeCountData: issueTypeCountDataForChart,
    };
};
