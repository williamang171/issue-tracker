import { blue, green, yellow, orange, red } from '@ant-design/colors';
import { createConstantObject } from "../utils/utils-constants";

export const OPEN = 0;
export const IN_PROGRESS = 1;
export const RESOLVED = 2;
export const CLOSED = 3;

export const ISSUE_STATUS = {
    [OPEN]: createConstantObject(OPEN, 'Open', blue.primary),
    [IN_PROGRESS]: createConstantObject(IN_PROGRESS, 'In Progress', orange.primary),
    [RESOLVED]: createConstantObject(RESOLVED, 'Resolved', green[4]),
    [CLOSED]: createConstantObject(CLOSED, 'Closed', green[7]),
} as const;

export type Status = typeof ISSUE_STATUS[keyof typeof ISSUE_STATUS];

export const ISSUE_STATUS_ARRAY: Status[] = Object.values(ISSUE_STATUS);

export function isKeyOfProjectIssueStatus(key: number): key is keyof typeof ISSUE_STATUS {
    return key in ISSUE_STATUS;
}